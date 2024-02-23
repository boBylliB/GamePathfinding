using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Kinematic
{
    UserControlled myMoveType;
    Firing myBallisticType;

    public float maxAcceleration = 1f;
    public float frictionPercent = 1f;

    // Start is called before the first frame update
    void Start()
    {
        myMoveType = new UserControlled();
        myMoveType.character = this;
        myMoveType.maxAcceleration = maxAcceleration;
        myMoveType.frictionPercent = frictionPercent;
    }

    // Update is called once per frame
    protected override void Update()
    {
        steeringUpdate = new SteeringOutput();
        steeringUpdate.linear = myMoveType.getSteering().linear;
        base.Update();
    }
}
