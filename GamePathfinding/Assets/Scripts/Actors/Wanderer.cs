using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wanderer : Kinematic
{
    // The radius and forward offset of the wander circle
    public float wanderOffset = 0f;
    public float wanderRadius = 0f;

    // The maximum rate of change of the wander orientation
    public float wanderRate = 0f;

    Wander mySteeringType;

    // Start is called before the first frame update
    void Start()
    {
        mySteeringType = new Wander();
        mySteeringType.character = this;
        // If any of the settings are set to 0, leave the default value
        if (wanderOffset != 0)
        {
            mySteeringType.wanderOffset = wanderOffset;
        }
        if (wanderRadius != 0)
        {
            mySteeringType.wanderRadius = wanderRadius;
        }
        if (wanderRate != 0)
        {
            mySteeringType.wanderRate = wanderRate;
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        steeringUpdate = new SteeringOutput();
        steeringUpdate.linear = mySteeringType.getSteering().linear;
        steeringUpdate.angular = mySteeringType.getSteering().angular;
        base.Update();
    }
}