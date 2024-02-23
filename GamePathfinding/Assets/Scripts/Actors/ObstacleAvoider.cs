using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleAvoider : Kinematic
{
    ObstacleAvoidance myMoveType;
    LookWhereGoing myRotateType;

    // Minimum distance to a wall
    public float avoidDist = 30f;
    // The distance to look for collisions
    public float lookAhead = 10f;
    // The line renderer to use for debugging raycasts
    public LineRenderer lr;
    public Material hitMat;
    public Material missMat;

    // Start is called before the first frame update
    void Start()
    {
        myMoveType = new ObstacleAvoidance();
        myMoveType.character = this;
        myMoveType.avoidDist = avoidDist;
        myMoveType.lookAhead = lookAhead;
        myMoveType.lr = lr;
        myMoveType.hitMat = hitMat;
        myMoveType.missMat = missMat;
        myMoveType.debug = true;
        myMoveType.ignoredTags = new List<string>();

        myRotateType = new LookWhereGoing();
        myRotateType.character = this;
        myRotateType.target = myTarget;
    }

    // Update is called once per frame
    protected override void Update()
    {
        steeringUpdate = new SteeringOutput();
        steeringUpdate.linear = myMoveType.getSteering().linear;
        steeringUpdate.angular = myRotateType.getSteering().angular;
        base.Update();
    }
}
