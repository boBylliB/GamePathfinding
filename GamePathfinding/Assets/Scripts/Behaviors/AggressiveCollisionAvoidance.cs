using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggressiveCollisionAvoidance : SteeringBehavior
{
    public Kinematic character;
    float maxAcceleration = 1f;

    public List<Kinematic> targets;

    // The collision radius of a character
    // This assumes that all characters have the same collision radius
    public float radius = 1f;

    public bool debug = false;
    public LineRenderer lr1;
    public LineRenderer lr2;
    public Material relPosMat;
    public Material relVelMat;

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
        Vector3 minRelPos = Vector3.zero;
        Vector3 minRelVel = Vector3.zero;

        // Loop through all targets
        // I've commented out unnecessary variables that aren't used in my actual implementation
        // They're left in for documentation and in case they're needed later, but it should atleast make this loop less intensive
        foreach (Kinematic target in targets)
        {
            // Calculate time to collision
            Vector3 relPos = target.transform.position - character.transform.position;
            Vector3 relVel = character.linearVelocity - target.linearVelocity;
            float relSpd = relVel.magnitude;
            float timeToColl = Vector3.Dot(relPos, relVel) / (relSpd * relSpd);

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
                minRelPos = relPos;
                minRelVel = relVel;
            }
        }

        // If we have no target, exit
        if (minTarget == null) return result;

        // If we're using this method instead of standard collision avoidance, then odds are we're dealing with targets that actively want to hit us
        // In this case, we need to be super dramatic about our avoidance

        // Steer to increase the angle between the relative velocity vector and the relative position vector

        // Use the relative position vector and the "up" vector to create a plane to check against
        Vector3 checkPlane = Vector3.Cross(minRelPos, Vector3.up).normalized;
        // Use the dot product of that plane's normal and the relative velocity vector to determine the sign of our steering vector
        float direction = Mathf.Sign(Vector3.Dot(checkPlane, minRelVel));
        // Move either left or right based on that information
        result.linear = direction * Vector3.Cross(character.linearVelocity, Vector3.up).normalized;
        if (minTarget.linearVelocity.magnitude > 0 && Vector3.Dot(minTarget.linearVelocity, character.linearVelocity) > -0.4)
        {
            // If we're approaching more of a right angle or rear-end collision, turning isn't always ideal
            // In that scenario, we also mix in a bit of acceleration directly away from the target
            result.linear += -minRelPos;
        }

        // Full send in the chosen direction
        result.linear.Normalize();
        result.linear *= maxAcceleration;

        // Debug visualization
        if (debug)
        {
            lr1.SetPosition(0, character.transform.position);
            lr2.SetPosition(0, character.transform.position);
            lr1.SetPosition(1, minTarget.transform.position);
            lr2.SetPosition(1, character.transform.position + minRelVel.normalized * 2);
            lr1.material = relPosMat;
            lr2.material = relVelMat;
        }

        return result;
    }
}
