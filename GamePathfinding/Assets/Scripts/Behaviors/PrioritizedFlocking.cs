using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrioritizedFlocking : Prioritization
{
    public Kinematic character;
    public List<Kinematic> flock;
    public FlockCOM centerOfMass;
    public float targetSpeed;

    // The collision radius of a character
    public float radius = 1f;
    public float sepThreshold = 10f;

    // Minimum distance to a wall
    public float avoidDist = 30f;
    // The distance to look for collisions
    public float lookAhead = 10f;
    // Tags to ignore
    public List<string> ignoredTags;
    // The line renderer to use for debugging raycasts
    public bool debug = false;
    public LineRenderer lr;
    public Material hitMat;
    public Material missMat;

    // The number of rays to cast
    public int numRays = 1;
    // The ray spread angle
    public float rayAngle = 10f;
    // The current iteration count
    public int iterationCount = 0;

    public float sepWeight = 1f;
    public float alignWeight = 1f;
    public float matchWeight = 1f;
    public float cohesionWeight = 1f;
    public float avoidWeight = 1f;
    public float dodgeWeight = 1f;

    public override List<BlendedBehavior> getBehaviors()
    {
        List<BlendedBehavior> groups = new List<BlendedBehavior>();

        // Avoidance
        GeneralAvoidance avoid = new GeneralAvoidance();
        avoid.character = character;
        avoid.flock = centerOfMass.flock;
        avoid.avoidWeight = centerOfMass.avoidWeight;
        avoid.dodgeWeight = centerOfMass.dodgeWeight;
        avoid.avoidDist = avoidDist;
        avoid.lookAhead = lookAhead;
        avoid.lr = lr;
        avoid.hitMat = hitMat;
        avoid.missMat = missMat;
        avoid.debug = debug;
        avoid.ignoredTags = ignoredTags;
        avoid.numRays = numRays;
        avoid.rayAngle = rayAngle;
        avoid.radius = radius;
        avoid.iterationCount = iterationCount;
        groups.Add(avoid);

        // Flocking
        Flocking flocking = new Flocking();
        flocking.character = character;
        flocking.flock = centerOfMass.flock;
        flocking.centerOfMass = centerOfMass;
        flocking.targetSpeed = targetSpeed;
        flocking.sepWeight = centerOfMass.sepWeight;
        flocking.alignWeight = centerOfMass.alignWeight;
        flocking.matchWeight = centerOfMass.matchWeight;
        flocking.cohesionWeight = centerOfMass.cohesionWeight;
        flocking.sepThreshold = sepThreshold;
        groups.Add(flocking);

        // Wander
        // This is last, and we don't really actually care how effective this wander is, so we keep the default settings
        // Using the built-in blended behavior workaround for a single behavior
        Wander wander = new Wander();
        BlendedBehavior wanderBlend = new BlendedBehavior();
        wander.character = character;
        wanderBlend.defaultBehavior = wander;
        groups.Add(wanderBlend);

        return groups;
    }
}
