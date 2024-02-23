using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralAvoidance : BlendedBehavior
{
    public Kinematic character;
    public List<Kinematic> flock;

    // The collision radius of a character
    public float radius = 1f;

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

    public float avoidWeight = 1f;
    public float dodgeWeight = 1f;

    public override List<SteeringBehavior> getBehaviors()
    {
        List<SteeringBehavior> behaviors = new List<SteeringBehavior>();

        // Wall avoidance
        ObstacleAvoidance avoid = new ObstacleAvoidance();
        avoid.character = character;
        avoid.avoidDist = avoidDist;
        avoid.lookAhead = lookAhead;
        avoid.lr = lr;
        avoid.hitMat = hitMat;
        avoid.missMat = missMat;
        avoid.debug = debug;
        avoid.ignoredTags = ignoredTags;
        avoid.numRays = numRays;
        avoid.rayAngle = rayAngle;
        avoid.currentRay = iterationCount;
        behaviors.Add(avoid);

        // Collision avoidance
        AggressiveCollisionAvoidance dodge = new AggressiveCollisionAvoidance();
        dodge.character = character;
        dodge.targets = flock;
        dodge.radius = radius;
        behaviors.Add(dodge);

        return behaviors;
    }
}
