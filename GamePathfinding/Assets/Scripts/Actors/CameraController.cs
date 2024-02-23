using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : Kinematic
{
    CameraBuffer myMoveType;

    // Start is called before the first frame update
    void Start()
    {
        myMoveType = new CameraBuffer();
        myMoveType.camera = this;
        myMoveType.target = myTarget;
    }

    // Update is called once per frame
    protected override void Update()
    {
        steeringUpdate = new SteeringOutput();
        steeringUpdate.linear = myMoveType.getSteering().linear;
        steeringUpdate.angular = 0;
        base.Update();
    }
}
