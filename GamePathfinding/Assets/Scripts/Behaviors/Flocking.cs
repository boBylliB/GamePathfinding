using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flocking : BlendedBehavior
{
    public Kinematic character;
    public List<Kinematic> flock;
    public FlockCOM centerOfMass;
    public float targetSpeed;

    // The collision radius of a character
    public float sepThreshold = 10f;

    public float sepWeight = 1f;
    public float alignWeight = 1f;
    public float matchWeight = 1f;
    public float cohesionWeight = 1f;

    public override List<SteeringBehavior> getBehaviors()
    {
        List<SteeringBehavior> behaviors = new List<SteeringBehavior>();

        // Separation
        Separation sep = new Separation();
        sep.character = character;
        sep.targets = flock;
        sep.weight = sepWeight;
        sep.threshold = sepThreshold;
        sep.maxAcceleration = 3f;
        behaviors.Add(sep);

        // Alignment
        VelocityAlign align = new VelocityAlign();
        align.character = character;
        align.target = centerOfMass;
        align.weight = alignWeight;
        behaviors.Add(align);

        // Speed Matching
        SpeedMatch match = new SpeedMatch();
        match.character = character;
        match.targetSpeed = targetSpeed;
        match.weight = matchWeight;
        behaviors.Add(match);

        // Cohesion
        Pursue cohesion = new Pursue();
        cohesion.character = character;
        cohesion.target = centerOfMass.gameObject;
        cohesion.weight = cohesionWeight;
        behaviors.Add(cohesion);

        return behaviors;
    }
}
