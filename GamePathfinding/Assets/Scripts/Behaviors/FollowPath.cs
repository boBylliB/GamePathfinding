using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPath : Seek
{
    // The distance along the path to generate a target
    // Theoretically this could be negative,
    // but in my implementation the path offset SHOULD BE POSITIVE
    // This is because, to save on memory, path targets that we pass are removed automatically
    public float pathOffset = 1f;
    // Whether or not to use a predicted future position or the current position
    public bool predictive = false;

    // The Path class to use
    public Path path;

    float predictionTime = 0.1f;
    float currentParam = 0f;

    // Overrides Seek's target position with the targeted point on the path
    protected override Vector3 getTargetPosition(out bool valid)
    {
        valid = true;
        // Predict the future character position, or use the current position, based on the predictive boolean
        Vector3 pos = predictive ? character.transform.position + character.linearVelocity * predictionTime : character.transform.position;

        // Find the current position on the path
        currentParam = path.GetParam(pos, currentParam);

        // Offset it
        float targetParam = currentParam + pathOffset;

        // Get the target position
        // Default to the predicted future position, to not change anything about our motion if the path doesn't exist
        Vector3 targetPosition = path.GetPosition(targetParam, pos);

        return targetPosition;
    }
}
