using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Separation : SteeringBehavior
{
    public Kinematic character;
    public float maxAcceleration = 1f;

    public List<Kinematic> targets;

    public bool debug = false;
    public LineRenderer lr;
    public Material lrMat;

    // the threshold to take action
    public float threshold = 5f; // 5

    // the constant coefficient of decay for the inverse square law
    public float decayCoefficient = 100f;

    // exponential decay instead of inverse square
    public bool expDecay = false;
    public float expMax = 100f;
    // Note: increasing this scale will decrease the slope of the curve
    public float expScale = 2f;

    public override SteeringOutput getSteering()
    {
        SteeringOutput result = new SteeringOutput();

        if (debug)
            lr.material = lrMat;

        foreach (Kinematic target in targets)
        {
            Vector3 direction = character.transform.position - target.transform.position;
            float distance = direction.magnitude;

            if (distance < threshold)
            {
                if (expDecay)
                {
                    float strength = Mathf.Exp(Mathf.Log(expMax, (float)System.Math.E) - distance / expScale);
                    direction.Normalize();
                    result.linear += strength * direction;
                    if (debug)
                    {
                        lr.SetPosition(0, character.transform.position);
                        lr.SetPosition(1, target.transform.position);
                        lr.material.color = new Color(lr.material.color.r, lr.material.color.g, lr.material.color.b, strength / expMax);
                    }
                }
                else
                {
                    // calculate the strength of repulsion
                    float strength = Mathf.Min(decayCoefficient / (distance * distance), maxAcceleration);
                    direction.Normalize();
                    result.linear += strength * direction;
                    if (debug)
                    {
                        lr.SetPosition(0, character.transform.position);
                        lr.SetPosition(1, target.transform.position);
                        lr.material.color = new Color(lr.material.color.r, lr.material.color.g, lr.material.color.b, 1 - distance / threshold);
                    }
                }
            }
        }

        return result;
    }
}
