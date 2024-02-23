using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower : Kinematic
{
    FollowPath myMoveType;
    LookWhereGoing myRotateType;

    public Path path;
    public float pathOffset = 1f;
    public bool predictive = false;

    // Start is called before the first frame update
    void Start()
    {
        myMoveType = new FollowPath();
        myMoveType.character = this;
        myMoveType.path = path;
        myMoveType.pathOffset = pathOffset;
        myMoveType.predictive = predictive;

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
