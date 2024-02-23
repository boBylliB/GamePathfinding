using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prioritization : SteeringBehavior
{
    public float linearEpsilon = 0.1f;
    public float angularEpsilon = 0.1f;

    // If this is set to true, the priority system will treat linear and angular accelerations separately
    public bool splitPriority = false;
    // If this is set to true, only linear accelerations will be considered for the threshold
    public bool ignoreAngular = false;
    // If this is set to true, only angular accelerations will be considered for the threshold
    public bool ignoreLinear = false;

    // This function should be overridden by all prioritized behaviors to set the behavior groups to utilize
    public virtual List<BlendedBehavior> getBehaviors()
    {
        return new List<BlendedBehavior>();
    }
    public override SteeringOutput getSteering()
    {
        SteeringOutput result = new SteeringOutput();
        SteeringOutput current = new SteeringOutput();
        bool linearSet = false;
        bool angularSet = false;

        // Return the first group that outputs a steering acceleration higher than epsilon
        foreach (BlendedBehavior behaviorGroup in getBehaviors())
        {
            current = behaviorGroup.getSteering();

            bool linearThreshold = current.linear.magnitude > linearEpsilon;
            bool angularThreshold = Mathf.Abs(current.angular) > angularEpsilon;

            if (splitPriority && linearThreshold)
            {
                result.linear = current.linear;
                linearSet = true;
            }
            else if (splitPriority && angularThreshold)
            {
                result.angular = current.angular;
                angularSet = true;
            }
            else if (linearThreshold || angularThreshold)
            {
                result = current;
                linearSet = true;
                angularSet = true;
            }

            // Return early if both fields are set
            if (linearSet && angularSet || linearSet && ignoreAngular || angularSet && ignoreLinear)
                return result;
        }

        // Set any unset outputs to the last group
        if (!linearSet)
            result.linear = current.linear;
        if (!angularSet)
            result.angular = current.angular;

        return result;
    }
}
