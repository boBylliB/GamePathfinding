using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBuffer : SteeringBehavior
{
    // based on the separation behavior
    // instead of moving an object away from other objects, we move the camera to keep an object within a bounding box
    public Kinematic camera;
    float maxAcceleration = 100f;

    public GameObject target;

    // the threshold to take action
    float widthPercent = 0.7f;
    float heightPercent = 0.7f;

    // the constant coefficient of decay for the inverse square law
    float decayCoefficient = 200f;

    public override SteeringOutput getSteering()
    {
        SteeringOutput result = new SteeringOutput();

        Vector3 screenCenter = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, camera.transform.position.y - target.transform.position.y) / 2);
        Vector3 topLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, camera.transform.position.y - target.transform.position.y));
        Vector3 bottomRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, camera.transform.position.y - target.transform.position.y));

        float screenWidth = bottomRight.x - topLeft.x;
        float screenHeight = topLeft.z - bottomRight.z;

        Vector3 difference = target.transform.position - screenCenter;

        if (Mathf.Abs(difference.x) > widthPercent * screenWidth / 2)
        {
            float distance = Mathf.Min(target.transform.position.x - topLeft.x, bottomRight.x - target.transform.position.x);
            // calculate the strength of repulsion
            float strength = Mathf.Min(decayCoefficient / (distance * distance), maxAcceleration);
            result.linear += new Vector3(strength * Mathf.Sign(difference.x), 0, 0);
        }
        else
        {
            float distance = Mathf.Abs(difference.x);
            // calculate the slowdown strength based on distance of the object to the center
            float strength = Mathf.Min(decayCoefficient / (distance * distance), maxAcceleration);
            result.linear += new Vector3(strength * -Mathf.Sign(camera.linearVelocity.x), 0, 0);
        }
        if (Mathf.Abs(difference.z) > heightPercent * screenHeight / 2)
        {
            float distance = Mathf.Min(target.transform.position.z - bottomRight.z, topLeft.z - target.transform.position.z);
            // calculate the strength of repulsion
            float strength = Mathf.Min(decayCoefficient / (distance * distance), maxAcceleration);
            result.linear += new Vector3(0, 0, strength * Mathf.Sign(difference.z));
        }
        else
        {
            float distance = Mathf.Abs(difference.z);
            // calculate the slowdown strength based on distance of the object to the center
            float strength = Mathf.Min(decayCoefficient / (distance * distance), maxAcceleration);
            result.linear += new Vector3(0, 0, strength * -Mathf.Sign(camera.linearVelocity.z));
        }

        return result;
    }
}
