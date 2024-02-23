using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityAlign : SteeringBehavior
{
    public Kinematic character;
    public Kinematic target;

    // The velocity difference threshold for max acceleration
    public float maxVelDiff = 1f;

    public float maxAccel = 1f;

    public override SteeringOutput getSteering()
    {
        SteeringOutput result = new SteeringOutput();

        // Steer to decrease the angle between the target velocity vector and the current velocity vector

        // Use the target velocity vector and the "up" vector to create a plane to check against
        Vector3 checkPlane = Vector3.Cross(target.linearVelocity, Vector3.up).normalized;
        // Use the dot product of that plane's normal and our current velocity vector to determine the sign of our steering vector
        float direction = Mathf.Sign(Vector3.Dot(checkPlane, character.linearVelocity));
        // Move either left or right based on that information
        result.linear = -direction * Vector3.Cross(character.linearVelocity, Vector3.up).normalized;

        // if we are outside the velocity threshold, then move at max acceleration
        if ((target.linearVelocity - character.linearVelocity).magnitude > maxVelDiff)
        {
            result.linear *= maxAccel;
        }
        else // otherwise calculate a scaled acceleration
        {
            result.linear *= maxAccel * ((target.linearVelocity - character.linearVelocity).magnitude - maxVelDiff) / maxVelDiff;
        }

        return result;
    }
}
