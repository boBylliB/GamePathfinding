using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BallisticBehavior
{
    public abstract Trajectory getTrajectory();
    public float weight = 1f;
}