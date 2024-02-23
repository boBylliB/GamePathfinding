using System;
using System.Collections.Generic;
using UnityEngine;

public class Impact
{
    public GameObject gameObject;
    public Vector3 impactPoint;
    public Vector3 impactNormal;
    public float timeDiff;
    public bool bounce;
    public Vector3 bounceVec;

    public Impact(GameObject gameObject, Vector3 impactPoint, Vector3 impactNormal, float timeDiff, bool bounce, Vector3 bounceVec)
    {
        this.gameObject = gameObject;
        this.impactPoint = impactPoint;
        this.impactNormal = impactNormal;
        this.timeDiff = timeDiff;
        this.bounce = bounce;
        this.bounceVec = bounceVec;
    }
    public Impact(Vector3 impactPoint, float timeDiff)
    {
        this.gameObject = null;
        this.impactPoint = impactPoint;
        this.impactNormal = Vector3.zero;
        this.timeDiff = timeDiff;
        this.bounce = false;
        this.bounceVec = Vector3.zero;
    }
    public Impact()
    {
        this.gameObject = null;
        this.impactPoint = Vector3.zero;
        this.impactNormal = Vector3.zero;
        this.timeDiff = 0;
        this.bounce = false;
        this.bounceVec = Vector3.zero;
    }
}

public class Impactable
{
    public GameObject gameObject;
    public bool bounceable;

    public Impactable(GameObject gameObject, bool bounceable)
    {
        this.gameObject = gameObject;
        this.bounceable = bounceable;
    }
}

public class Trajectory
{
    // Public-facing variables
    public Vector3 startPoint;
    public Vector3 launchVec;
    public List<Impactable> obstacles;
    public List<Impact> impacts;
    // Any tags to ignore
    public List<string> ignoredTags;

    // Constructors
    public Trajectory()
    {
        startPoint = Vector3.zero;
        launchVec = Vector3.zero;
        obstacles = new List<Impactable>();
        impacts = new List<Impact>();
    }
    public Trajectory(Vector3 start, Vector3 launch, Vector3 impact, float impactTime)
    {
        startPoint = start;
        launchVec = launch;
        obstacles = new List<Impactable>();
        impacts = new List<Impact>();
        impacts.Add(new Impact(impact, impactTime));
    }
    // Public-facing class functions
    // Calculates points along the trajectory given a number of points to generate per trajectory segment
    public List<Vector3> calculatePoints(int pointsPerSegment)
    {
        List<Vector3> points = new List<Vector3>();

        Trajectory traj = this;
        for (int idx = 0; idx < impacts.Count; idx++)
        {
            points.AddRange(calculateSegmentPoints(traj, pointsPerSegment));
            if (idx < impacts.Count - 1)
            {
                traj.launchVec = traj.impacts[0].bounceVec;
                traj.startPoint = traj.impacts[0].impactPoint;
                traj.impacts[0] = traj.impacts[1];
            }
        }

        return points;
    }
    // Displays the trajectory using a given linerenderer and number of line segments to use per trajectory segment
    public void displayTrajectory(LineRenderer lr, int linesPerSegment)
    {
        // Calculate trajectory points
        List<Vector3> points = calculatePoints(linesPerSegment + 1);
        // Set the number of line positions to the number of trajectory points
        lr.positionCount = points.Count;
        // Iterate through all trajectory segments, setting positions to points as we go
        for (int idx = 0; idx < points.Count; idx++)
        {
            lr.SetPosition(idx, points[idx]);
        }
    }

    // Public-facing static functions
    // Calculates firing vector options given exit speed, target point, and (optionally) obstacles to avoid
    // If obstacles to avoid are given, the game object calculating the firing solution also has to be provided
    public static List<Trajectory> calculateFiringSolutions(Vector3 start, Vector3 target, float exitSpd, List<GameObject> obstacles = null, GameObject source = null, float uncertainty = 0.2f)
    {
        List<Trajectory> results = new List<Trajectory>();

        // Get candidate times
        List<float> impactTimes = calculateImpactTimes(start, target, exitSpd);

        // Return an empty list if no times were found
        if (impactTimes.Count == 0)
            return results;

        Vector3 delta = target - start;

        // Add positive times to results, as negative times aren't possible
        if (impactTimes[0] >= 0)
        {
            Vector3 launch = (2 * delta - Physics.gravity * impactTimes[0] * impactTimes[0]) / (2 * exitSpd * impactTimes[0]);
            launch = launch.normalized * exitSpd;
            Trajectory traj0 = new Trajectory(start, launch, target, impactTimes[0]);
            // Check for intersections with obstacles
            if (obstacles != null && source != null)
            {
                Impact impact = findFirstIntersection(traj0, source, uncertainty);
                // If the impact point is within the given uncertainty of the target point, or no impact was found, the trajectory is good
                if (impact == null || Vector3.Distance(impact.impactPoint, target) <= uncertainty)
                    results.Add(traj0);
            }
            else results.Add(traj0);
        }
        if (impactTimes.Count > 1 && impactTimes[1] >= 0)
        {
            Vector3 launch = (2 * delta - Physics.gravity * impactTimes[1] * impactTimes[1]) / (2 * exitSpd * impactTimes[1]);
            launch = launch.normalized * exitSpd;
            Trajectory traj1 = new Trajectory(start, launch, target, impactTimes[1]);
            // Check for intersections with obstacles
            if (obstacles != null && source != null)
            {
                Impact impact = findFirstIntersection(traj1, source, uncertainty);
                // If the impact point is within the given uncertainty of the target point, or no impact was found, the trajectory is good
                if (impact == null || Vector3.Distance(impact.impactPoint, target) <= uncertainty)
                    results.Add(traj1);
            }
            else results.Add(traj1);
        }

        return results;
    }
    // Calculates firing vector options given exit speed, target point, and obstacles to take into account, including ricochets
    public static List<Trajectory> calculateFiringSolutions(Vector3 start, Vector3 target, float exitSpd, List<GameObject> noBounceObstacles, List<GameObject> bounceObstacles, GameObject source, int maxDepth = 4, float rayDist = 0.2f, float uncertainty = 0.2f)
    {
        // This is utilizing a recursive search, prioritizing solutions with less bounces and therefore less cycles
        // This public-facing function just provides a slightly easier-to-use entry point
        // Please look at the first helper function for details
        List<MirrorPlane> planes = new List<MirrorPlane>();
        foreach (GameObject obstacle in noBounceObstacles)
        {
            // Cast a ray to the center of the obstacle, to determine the normal to utilize
            // This makes a huge simplification, only working for plane walls, but it was necessary for the current scope
            RaycastHit hitInfo;
            Collider coll = obstacle.gameObject.GetComponent<Collider>();
            Vector3 delta = obstacle.gameObject.transform.position - start;
            Ray ray = new Ray(start, obstacle.gameObject.transform.position - start);
            if (coll.Raycast(ray, out hitInfo, delta.magnitude * 1.1f))
            {
                MirrorPlane plane = new MirrorPlane(hitInfo.normal, coll.bounds, false);
                planes.Add(plane);
            }
            else
            {
                // Something went wrong
                Debug.LogError("Failed to raycast to collider! Object name: " + obstacle.gameObject.name);
            }
        }
        foreach (GameObject obstacle in bounceObstacles)
        {
            // Cast a ray to the center of the obstacle, to determine the normal to utilize
            // This makes a huge simplification, only working for large walls, but it was necessary for the current scope
            RaycastHit hitInfo;
            Collider coll = obstacle.gameObject.GetComponent<Collider>();
            Vector3 delta = obstacle.gameObject.transform.position - start;
            Ray ray = new Ray(start, obstacle.gameObject.transform.position - start);
            if (coll.Raycast(ray, out hitInfo, delta.magnitude * 1.1f))
            {
                MirrorPlane plane = new MirrorPlane(hitInfo.normal, coll.bounds, true);
                planes.Add(plane);
            }
            else
            {
                // Something went wrong
                Debug.LogError("Failed to raycast to collider! Object name: " + obstacle.gameObject.name);
            }
        }
        List<MirrorOption> startOption = new List<MirrorOption>();
        startOption.Add(new MirrorOption(planes, target));
        // Grab potential trajectories
        List<Trajectory> trajectories = checkForRicochets(start, exitSpd, source, maxDepth, 0, uncertainty, startOption);
        // We want to update these trajectories to include the bounces they take, so we'll pass it through the update function to calculate actual bounces
        for (int idx = 0; idx < trajectories.Count; idx++)
            trajectories[idx] = updateTrajectory(trajectories[idx], maxDepth, source, rayDist);

        return trajectories;
    }


    // Internal helper functions
    private static List<Trajectory> checkForRicochets(Vector3 start, float exitSpd, GameObject source, int maxDepth, int currentDepth, float uncertainty, List<MirrorOption> mirrorOptions)
    {
        List<Trajectory> results = new List<Trajectory>();
        List<MirrorOption> nextMirrorOptions = new List<MirrorOption>();
        Debug.Log("Current Recursion Depth = " + currentDepth);
        // Loop through all given mirror options
        foreach (MirrorOption option in mirrorOptions)
        {
            // Check for a direct firing solution first
            // We can do this by looking for a direct path to the target
            bool foundIntersect = false;
            Vector3 delta = option.target - start;
            foreach (MirrorPlane plane in option.obstacles)
            {
                if (plane.checkForIntersect(start, delta))
                {
                    foundIntersect = true;
                    Debug.Log("No direct solution found, intersecting the plane located at " + plane.bounds.center);
                    break;
                }
            }

            // If no direct firing solutions were found, we look for ricochets without recursing any further
            if (foundIntersect)
            {
                // This implementation is heavily influenced by the ideas presented in the following stackexchange post:
                // https://softwareengineering.stackexchange.com/questions/290628/algorithm-for-calculating-a-bullet-path-to-a-target-with-max-2-ricochets
                // However, my implementation being translated to 3D, assumes a few things:
                // - Bounceable walls are always vertical
                // - Bounceable walls are always infinitely high

                // Loop through all bounceable obstacles
                for (int idx = 0; idx < option.obstacles.Count; idx++)
                {
                    MirrorPlane obstacle = option.obstacles[idx];
                    if (!obstacle.bounceable) continue;

                    // If the target position is on the opposite side of the plane, a ricochet isn't possible
                    if (Vector3.Dot(option.target - obstacle.bounds.center, obstacle.normal) * Vector3.Dot(start - obstacle.bounds.center, obstacle.normal) < 0) break;
                    // Create a "mirrored" target
                    Vector3 mirroredTarget = obstacle.mirrorPoint(option.target);
                    // Check for a path to that mirrored target
                    foundIntersect = false;
                    delta = mirroredTarget - start;
                    for (int jdx = 0; jdx < option.obstacles.Count; jdx++)
                    {
                        // Skip this obstacle
                        if (idx == jdx) continue;

                        if (option.obstacles[jdx].checkForIntersect(start, delta))
                        {
                            foundIntersect = true;
                            Debug.Log("no mirrored trajectory");
                            break;
                        }
                    }

                    // If there's a path found to the mirrored target, use it
                    if (!foundIntersect)
                    {
                        results.AddRange(calculateFiringSolutions(start, mirroredTarget, exitSpd));
                        Debug.Log("Mirrored solution returned " + results.Count + " options");
                    }
                    
                    // If no launch vectors have been found, make a new mirror option utilizing this obstacle
                    if (results.Count == 0 && foundIntersect)
                    {
                        // Mirror all obstacles about the selected plane
                        List<MirrorPlane> mirroredObstacles = new List<MirrorPlane>();
                        for (int jdx = 0; jdx < option.obstacles.Count; jdx++)
                        {
                            // Skip this obstacle
                            if (idx == jdx) continue;
                            // Create a mirrored copy of the obstacle
                            Vector3 mirroredNormal = option.obstacles[jdx].normal - 2 * Vector3.Dot(option.obstacles[jdx].normal, option.obstacles[idx].normal) * option.obstacles[idx].normal;
                            Bounds mirroredBounds = option.obstacles[jdx].bounds;
                            mirroredBounds.min = option.obstacles[idx].mirrorPoint(mirroredBounds.min);
                            mirroredBounds.max = option.obstacles[idx].mirrorPoint(mirroredBounds.max);
                            mirroredObstacles.Add(new MirrorPlane(mirroredNormal, mirroredBounds, option.obstacles[jdx].bounceable));
                        }
                        // Append the new mirror option to the list
                        nextMirrorOptions.Add(new MirrorOption(mirroredObstacles, mirroredTarget));
                    }
                }
            }
            else
            {
                // If we get here, then a direct firing solution should have been found
                results = calculateFiringSolutions(start, option.target, exitSpd);
                Debug.Log("Direct solution returned " + results.Count + " options");
                // If the direct path returns solutions, we're done
                if (results.Count > 0) break;
            }
        }
        
        // If no results have been found so far, then we try to recurse, if we haven't hit the max depth
        if (results.Count == 0 && currentDepth < maxDepth)
        {
            Debug.Log("Recursing with " + nextMirrorOptions.Count + " options");
            results.AddRange(checkForRicochets(start, exitSpd, source, maxDepth, currentDepth + 1, uncertainty, nextMirrorOptions));
        }

        return results;
    }
    private static Trajectory updateTrajectory(Trajectory trajectory, int maxIterations, GameObject ignored, float rayDist)
    {
        Impact lastImpact = null;
        Trajectory lastTrajectory = trajectory;
        Trajectory output = trajectory;

        // Loop until a noBounceObstacle is hit
        for (int idx = 0; idx < maxIterations; idx++)
        {
            // Find next intersection
            Impact impact = findFirstIntersection(lastTrajectory, ignored, rayDist);
            // If no intersection was found, then the previous impact must be the last impact
            if (impact == null)
                return output;
            // If the impact is not a bounce, then it is the last impact
            if (!impact.bounce)
            {
                output.impacts.Add(impact);
                return output;
            }
            // Otherwise, prep for the next loop iteration
            lastImpact = impact;
            lastTrajectory.launchVec = lastImpact.bounceVec;
            lastTrajectory.startPoint = lastImpact.impactPoint;
            lastTrajectory.impacts.Add(lastImpact);
            output.impacts.Add(lastImpact);
        }
        // If we exit the loop, we hit the iteration limit and should return the last impact found
        return output;
    }
    private static Impact findFinalImpact(Trajectory trajectory, int maxIterations, GameObject ignored, float rayDist)
    {
        Impact lastImpact = null;
        Trajectory lastTrajectory = trajectory;
        
        // Loop until a noBounceObstacle is hit
        for (int idx = 0; idx < maxIterations; idx++)
        {
            // Find next intersection
            Impact impact = findFirstIntersection(lastTrajectory, ignored, rayDist);
            // If no intersection was found, then the previous impact must be the last impact
            if (impact == null)
                return lastImpact;
            // If the impact is not a bounce, then it is the last impact
            if (!impact.bounce)
                return impact;
            // Otherwise, prep for the next loop iteration
            lastImpact = impact;
            lastTrajectory.launchVec = lastImpact.bounceVec;
            lastTrajectory.startPoint = lastImpact.impactPoint;
            lastTrajectory.impacts.Add(lastImpact);
        }
        // If we exit the loop, we hit the iteration limit and should return the last impact found
        return lastImpact;
    }
    private static Impact findFirstIntersection(Trajectory traj, GameObject ignored, float rayDist)
    {
        // Since we're trying to find the first intersection, the trajectory given is likely incomplete
        // The only information we require is the launch vector, start position, and objects to take into account
        // NOTE: This simplifies quite a bit for a first attempt, and is not great at some objects or scenarios

        // Calculate some statistics to know the bounds of our impact checking
        Bounds bounds = new Bounds();
        foreach (Impactable obstacle in traj.obstacles)
        {
            Collider coll = obstacle.gameObject.GetComponent<Collider>();
            bounds.Encapsulate(coll.bounds);
        }

        // To find intersections, we iterate along the trajectory, casting rays forward
        // Once our position is outside of our bounding box, we know we didn't hit anything
        float timeIter = 0f;
        Vector3 iterPos = traj.startPoint;
        Vector3 vel = traj.launchVec;
        float hitDist = -1f;
        RaycastHit hitInfo;
        while (bounds.Contains(iterPos))
        {
            // Cast a ray in the direction of the current projectile velocity, scaled by the given ray distance
            if (Physics.Raycast(iterPos, vel.normalized, out hitInfo, rayDist) && (traj.ignoredTags == null || !traj.ignoredTags.Contains(hitInfo.collider.tag)))
            {
                // Find the distance until ray intersection
                hitDist = hitInfo.distance;
                // Increase the time by 90% of the estimated time to impact
                timeIter += vel.magnitude / (hitDist * 0.9f);
                // Update the other iteration variables
                iterPos = traj.startPoint + vel * timeIter + 0.5f * Physics.gravity * timeIter * timeIter;
                vel = traj.launchVec + Physics.gravity * timeIter;
                // Break out of the while loop
                break;
            }
            // Increase the time by the current projectile speed divided by the desired ray distance
            // It seems to be beneficial to have a bit of overlap here, so we aim to overlap 1/4 of the ray distance
            timeIter += vel.magnitude / (rayDist * 3 / 4);
            // Get the new projectile position and speed from the ballistic trajectory equations
            iterPos = traj.startPoint + vel * timeIter + 0.5f * Physics.gravity * timeIter * timeIter;
            vel = traj.launchVec + Physics.gravity * timeIter;
        }

        if (hitDist > 0)
        {
            // If we get here, we found a hit earlier, and have been positioned right in front of the predicted hit location
            // Cast a ray in the direction of the projectile velocity
            if (Physics.Raycast(iterPos, vel.normalized, out hitInfo, hitDist) && (traj.ignoredTags == null || !traj.ignoredTags.Contains(hitInfo.collider.tag)))
            {
                // If we hit something, grab the impact info and return it
                Impact impact = new Impact();
                // We can grab some info directly from the raycast
                impact.gameObject = hitInfo.collider.gameObject;
                impact.impactPoint = hitInfo.point;
                impact.impactNormal = hitInfo.normal;
                // Estimate the time to impact, seeing as we now have a start point, end point, and launch vector
                impact.timeDiff = calculateImpactTime(traj.startPoint, impact.impactPoint, traj.launchVec);
                // Search for the object in the obstacle list
                Impactable hitObstacle = traj.obstacles.Find(x => x.gameObject.Equals(impact.gameObject));
                // If the obstacle isn't in the list, we assume we can't bounce off of it
                if (hitObstacle == null || !hitObstacle.bounceable)
                {
                    // If there isn't a bounce, we're done, and can return
                    return impact;
                }
                else
                {
                    // If the obstacle is a bounce, we just need to calculate the bounce vector
                    impact.bounce = true;
                    // Project the object velocity vector onto the impact normal
                    float projSpd = Vector3.Dot(vel, impact.impactNormal);
                    // Apply 2 times the inverse of this projected vector
                    impact.bounceVec = vel - 2 * projSpd * impact.impactNormal;
                    // Now we're done and can return the impact
                    return impact;
                }
            }
            else
            {
                // If we get here, something went wrong
                // Normally I'd throw an error, but for a proof of concept like this, I shall simply return null and log something
                Debug.Log("Predicted an impact that wasn't found.");
                return null;
            }
        }
        else
        {
            // If we get here, we found nothing, and return null
            return null;
        }
    }
    private static List<Vector3> calculateSegmentPoints(Trajectory traj, int numPoints)
    {
        // Calculate the timestep between points
        float timeStep = traj.impacts[0].timeDiff / (numPoints - 1);
        // Iterate over the trajectory to generate the points
        List<Vector3> points = new List<Vector3>();
        float timeIter = 0f;
        for (int idx = 0; idx < numPoints; idx++)
        {
            points.Add(traj.startPoint + traj.launchVec * timeIter + 0.5f * Physics.gravity * timeIter * timeIter);
            timeIter += timeStep;
        }

        return points;
    }
    private static float calculateImpactTime(Vector3 startPoint, Vector3 endPoint, Vector3 launchVec)
    {
        // Find candidate times
        List<float> impactTimes = calculateImpactTimes(startPoint, endPoint, launchVec.magnitude);
        // Return negative infinity if no times were found
        if (impactTimes.Count == 0)
            return float.NegativeInfinity;
        // Calculate the time until the x component would reach its final value
        float xTime = launchVec.x / Mathf.Abs(endPoint.x - startPoint.x);
        // Use this xTime to select the correct impact time
        if (impactTimes.Count == 1 || Mathf.Abs(impactTimes[0] - xTime) < Mathf.Abs(impactTimes[1] - xTime))
            return impactTimes[0];
        else
            return impactTimes[1];
    }
    private static List<float> calculateImpactTimes(Vector3 startPoint, Vector3 endPoint, float launchSpeed)
    {
        List<float> results = new List<float>();

        Vector3 delta = endPoint - startPoint;

        // Calculate the a,b,c coefficients of a conventional quadratic equation
        float a = Physics.gravity.sqrMagnitude;
        float b = -4 * (Vector3.Dot(Physics.gravity, delta) + launchSpeed * launchSpeed);
        float c = 4 * delta.sqrMagnitude;

        // Check for a lack of real solutions, and return an empty list if so
        float b2minus4ac = b * b - 4 * a * c;
        if (b2minus4ac < 0)
            return results;

        // Find candidate times
        float time0 = Mathf.Sqrt((-b + Mathf.Sqrt(b2minus4ac)) / (2 * a));
        float time1 = Mathf.Sqrt((-b - Mathf.Sqrt(b2minus4ac)) / (2 * a));

        // Add positive times to results, as negative times aren't possible
        if (time0 >= 0)
            results.Add(time0);
        if (time1 >= 0)
            results.Add(time1);

        return results;
    }

    // Internal data structures and classes
    private struct MirrorOption
    {
        public MirrorOption(List<MirrorPlane> obstacles, Vector3 target)
        {
            this.obstacles = obstacles;
            this.target = target;
        }

        public List<MirrorPlane> obstacles { get; }
        public Vector3 target { get; }
    }
    private class MirrorPlane
    {
        public Vector3 normal;
        public bool bounceable;
        public Bounds bounds;
        public MirrorPlane(Vector3 normal, Bounds bounds, bool bounceable)
        {
            Vector3 newNormal = new Vector3(normal.x, 0, normal.z);
            this.normal = newNormal.normalized;
            this.bounceable = bounceable;
            this.bounds = bounds;
        }
        public bool checkForIntersect(Vector3 origin, Vector3 dir)
        {
            // If, somehow, the vector and plane are parallel, we assume no intersection
            if (Vector3.Dot(normal, dir) == 0) return false;
            // Otherwise, we can calculate the point on the plane that the line intersects
            float d = Vector3.Dot(bounds.center - origin, normal) / Vector3.Dot(dir.normalized, normal);
            Vector3 intersection = origin + dir.normalized * d;
            // If the intersection point is within the bounds of the plane, and the distance is shorter than the distance to target, there is an intersection
            return (bounds.Contains(intersection) && d < dir.magnitude);
        }
        public Vector3 mirrorPoint(Vector3 point)
        {
            // Calculate the shortest distance from the point to the plane
            float dist = Vector3.Dot(normal, point) - Vector3.Dot(normal, bounds.center);
            // Shift the point in the negative normal direction by twice the calculated distance
            return point - 2 * dist * normal;
        }
    }
}
