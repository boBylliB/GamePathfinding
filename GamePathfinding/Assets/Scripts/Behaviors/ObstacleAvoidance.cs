using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleAvoidance : Seek
{
    // Minimum distance to a wall
    public float avoidDist = 30f;
    // The distance to look for collisions
    public float lookAhead = 10f;
    // For debug purposes, I'm also rendering a line
    public bool debug = false;
    public LineRenderer lr;
    public Material hitMat;
    public Material missMat;
    // Any tags to ignore
    public List<string> ignoredTags;

    // The number of rays to cast
    public int numRays = 1;
    // The ray spread angle
    public float rayAngle = 10f;

    public int currentRay = 0;

    // Takes in an orientation angle (in degrees) and outputs a normalized vector pointing in that direction
    private Vector3 angleToVector(float angle)
    {
        // Since we're about to normalize it, magnitude doesn't matter
        Vector3 result = new Vector3(-Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
        return result.normalized;
    }
    // Takes in a vector and outputs an orientation angle (in degrees)
    private float vectorToAngle(Vector3 vector)
    {
        // Normalize the vector just to make sure
        // Have to make sure we're using a coordinate system where angle 0 is at the z axis and it increases counterclockwise around the y vector
        float result = Mathf.Atan2(-vector.normalized.x, vector.normalized.z) * Mathf.Rad2Deg;
        return result;
    }
    protected override Vector3 getTargetPosition(out bool valid)
    {
        // Calculate the raycast direction
        // If there's only one ray, we can ignore this step
        Vector3 raycastDir = character.linearVelocity.normalized;
        if (numRays > 1)
        {
            // Offset the ray from negative to positive angles, evenly
            float raySpread = rayAngle / (numRays - 1);
            float rayOffset = (-rayAngle / 2) + raySpread * currentRay;
            // Get the angle of the current forward direction
            float currentAngle = vectorToAngle(raycastDir);
            // Add the offset angle
            currentAngle += rayOffset;
            // Set the new raycast direction
            raycastDir = angleToVector(currentAngle);
        }
        // We want to store raycast data to get the point of intersection
        RaycastHit hitInfo;
        if (debug)
        {
            // Set debug line renderer stuff
            lr.SetPosition(0, character.transform.position);
            lr.SetPosition(1, raycastDir * lookAhead + character.transform.position);
        }
        Vector3 targetPoint = Vector3.zero;
        // Raycast ahead to check for a collision
        if (Physics.Raycast(character.transform.position, raycastDir, out hitInfo, lookAhead) && !ignoredTags.Contains(hitInfo.collider.tag))
        {
            if (debug)
                lr.material = hitMat;
            valid = true;
            targetPoint = hitInfo.point + hitInfo.normal * avoidDist;
            // Deliberately remove any y values
            targetPoint.y = 0;
        }
        else
        {
            if (debug)
                lr.material = missMat;
            // If there's no collision, set the seek position to be invalid
            valid = false;
            targetPoint = Vector3.positiveInfinity;
        }

        return targetPoint;
    }
}
