using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionAvoider : Kinematic
{
    CollisionAvoidance myMoveType;
    LookWhereGoing myRotateType;

    // The targets to avoid
    public List<Kinematic> targets;
    // The collision radius of a character
    public float radius = 1f;

    public LineRenderer lr;

    // Start is called before the first frame update
    void Start()
    {
        myMoveType = new CollisionAvoidance();
        myMoveType.character = this;
        myMoveType.targets = targets;
        myMoveType.radius = radius;

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
