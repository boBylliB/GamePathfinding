using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionAvoidance : SteeringBehavior
{
    public Kinematic character;
    float maxAcceleration = 1f;

    public List<Kinematic> targets;

    // The collision radius of a character
    // This assumes that all characters have the same collision radius
    public float radius = 1f;

    public override SteeringOutput getSteering()
    {
        SteeringOutput result = new SteeringOutput();

        // Preset result values to 0, in case of early exit
        result.linear = Vector3.zero;
        result.angular = 0;

        // If we aren't moving, exit
        if (character.linearVelocity.magnitude <= 0) return result;

        // Store the first collision time
        float minTime = float.MaxValue;

        // Store the target that collides at that time
        // Also store other data we need
        Kinematic minTarget = null;
        //float minSep = 0;
        //float minDist = 0;
        //Vector3 minRelPos = Vector3.zero;
        //Vector3 minRelVel = Vector3.zero;

        // Loop through all targets
        // I've commented out unnecessary variables that aren't used in my actual implementation
        // They're left in for documentation and in case they're needed later, but it should atleast make this loop less intensive
        foreach (Kinematic target in targets)
        {
            // Calculate time to collision
            Vector3 relPos = target.transform.position - character.transform.position;
            Vector3 relVel = character.linearVelocity - target.linearVelocity;
            float relSpd = relVel.magnitude;
            float timeToColl = Vector3.Dot(relPos, relVel) / (relSpd*relSpd);

            // Check if it will be a collision at all
            float dist = relPos.magnitude;
            float sep = dist - relSpd * timeToColl;
            if (sep > 2 * radius) continue;

            // Check if it is the shortest
            if (timeToColl > 0 && timeToColl < minTime)
            {
                // Store data as minimum
                minTime = timeToColl;
                minTarget = target;
                //minSep = sep;
                //minDist = dist;
                //minRelPos = relPos;
                //minRelVel = relVel;
            }
        }

        // If we have no target, exit
        if (minTarget == null) return result;

        // Millington actively steers towards the target, which isn't ideal
        // I had an idea for a steering behavior that uses the fact that we can already obtain both velocity vectors
        // It's based on traffic with cars, where the simple rule of "move right" applies in almost all scenarios
        // (Oddly enough, my implementation seems to always steer left instead of right, but it still works the same way, just feels weirdly British)
        // However, this assumes that all potential collisions are either also following this rule or are stationary

        // Steer to the right
        // If we define a plane based on our velocity vector and a vector pointing straight up,
        // we can use the normal vector of that plane to define a perpendicular vector
        result.linear = Vector3.Cross(character.linearVelocity, Vector3.up).normalized * maxAcceleration;

        // If the angle between velocity vectors is less than 90 degrees, this may not be enough
        // Therefore, we figure out if we are the right or left vector, and speedup/slow down accordingly
        // I also throw a check in here to make sure the target is actually moving, since if it isn't then this extra step is unnecessary
        if (minTarget.linearVelocity.magnitude > 0 && Vector3.Dot(minTarget.linearVelocity, character.linearVelocity) > 0)
        {
            // We can once again use a plane, although this time defined by the target's linear velocity and the up vector
            // If we're to the right of the target's velocity vector, then our velocity vector will be pointing towards this plane
            // This means the dot product of our velocity vector and the normal vector of this plane is dependent on what side of the plane we're on
            // To figure out if we need to speed up or slow down, we can just use the sign of this dot product, and we'll get opposing results for either side
            result.linear += Mathf.Sign(Vector3.Dot(Vector3.Cross(minTarget.linearVelocity, Vector3.up).normalized, character.linearVelocity)) * character.linearVelocity.normalized * maxAcceleration;
        }

        return result;
    }
}
