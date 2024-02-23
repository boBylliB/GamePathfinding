using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedMatch : SteeringBehavior
{
    public Kinematic character;
    public float targetSpeed;

    // The speed difference threshold for max acceleration
    public float maxDiff = 1f;

    public float maxAccel = 1f;

    public override SteeringOutput getSteering()
    {
        SteeringOutput result = new SteeringOutput();

        // Find speed difference
        float speedDiff = targetSpeed - character.linearVelocity.magnitude;

        // if we are outside the velocity threshold, then move at max acceleration
        if (speedDiff > maxDiff)
        {
            result.linear = character.linearVelocity.normalized * maxAccel;
        }
        else // otherwise calculate a scaled acceleration
        {
            result.linear = character.linearVelocity.normalized * maxAccel * (speedDiff - maxDiff) / maxDiff;
        }

        return result;
    }
}
