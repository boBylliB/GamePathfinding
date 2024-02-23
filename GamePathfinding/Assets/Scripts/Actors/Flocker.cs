using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flocker : Kinematic
{
    PrioritizedFlocking myMoveType;
    LookWhereGoing myRotateType;

    public FlockCOM centerOfMass;
    public float targetSpeed = 3f;

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

    public float maxAngularAcceleration = 10f;

    public float epsilon = 0.1f;

    private int iterationCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        myMoveType = new PrioritizedFlocking();
        // We're using this solely for movement, so we can ignore angular accelerations
        myMoveType.ignoreAngular = true;
        myMoveType.linearEpsilon = epsilon;
        myMoveType.character = this;
        myMoveType.flock = centerOfMass.flock;
        myMoveType.centerOfMass = centerOfMass;
        myMoveType.targetSpeed = targetSpeed;
        myMoveType.sepWeight = centerOfMass.sepWeight;
        myMoveType.alignWeight = centerOfMass.alignWeight;
        myMoveType.matchWeight = centerOfMass.matchWeight;
        myMoveType.cohesionWeight = centerOfMass.cohesionWeight;
        myMoveType.avoidWeight = centerOfMass.avoidWeight;
        myMoveType.dodgeWeight = centerOfMass.dodgeWeight;
        myMoveType.avoidDist = avoidDist;
        myMoveType.lookAhead = lookAhead;
        myMoveType.lr = lr;
        myMoveType.hitMat = hitMat;
        myMoveType.missMat = missMat;
        myMoveType.debug = debug;
        myMoveType.ignoredTags = ignoredTags;
        myMoveType.numRays = numRays;
        myMoveType.rayAngle = rayAngle;
        myMoveType.radius = radius;
        myMoveType.sepThreshold = sepThreshold;

        myRotateType = new LookWhereGoing();
        myRotateType.character = this;
        myRotateType.target = myTarget;
        myRotateType.maxAngularAcceleration = maxAngularAcceleration;
        myRotateType.maxRotation = maxAngularVelocity;
    }

    // Update is called once per frame
    protected override void Update()
    {
        steeringUpdate = new SteeringOutput();
        steeringUpdate.linear = myMoveType.getSteering().linear;
        myMoveType.iterationCount = iterationCount++ % numRays;
        steeringUpdate.angular = myRotateType.getSteering().angular;
        base.Update();
        // Simple double check to make sure the y level is still 0
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }
}
