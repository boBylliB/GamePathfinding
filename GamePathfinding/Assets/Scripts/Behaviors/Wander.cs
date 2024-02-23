using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// based on Millington pp. 75-76
public class Wander : Face
{
    // The radius and forward offset of the wander circle
    public float wanderOffset = 5f;
    public float wanderRadius = 3f;

    // The maximum rate of change of the wander orientation
    public float wanderRate = 3f;

    // The current orientation of the wander target
    float wanderOrientation = 0f;

    // The character's maximum acceleration
    float maxAcceleration = 100f;

    // Takes in an orientation angle (in degrees) and outputs a normalized vector pointing in that direction
    private Vector3 angleToVector(float angle)
    {
        // Since we're about to normalize it, magnitude doesn't matter
        // Therefore, I just assume one side of the "triangle" is 1
        Vector3 result = new Vector3(Mathf.Tan(angle * Mathf.Deg2Rad), 0, 1);
        return result.normalized;
    }

    // This function is used to override the Face target position, that way a new implementation isn't needed
    public override Vector3 getTargetPosition()
    {
        // Update the wander orientation
        wanderOrientation += Random.Range(-1f, 1f) * wanderRate;

        // Calculate the combined target orientation
        float targetOrientation = wanderOrientation + character.transform.eulerAngles.y;

        // Calculate the center of the wander circle
        Vector3 targetPoint = character.transform.position + wanderOffset * angleToVector(character.transform.eulerAngles.y);

        // Calculate the target location
        targetPoint += wanderRadius * angleToVector(targetOrientation);
        
        // Use Face to align to the target point
        return targetPoint;
    }

    // This function overrides Align's zero linear with full acceleration
    public override Vector3 getDesiredLinear()
    {
        return maxAcceleration * angleToVector(character.transform.eulerAngles.y);
    }
}
