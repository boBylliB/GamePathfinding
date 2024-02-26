using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficGraphController : MonoBehaviour
{
    public TrafficGraph graph;
    public float refreshTime = 3f;

    private float timer = 0f;

    // Update is called once per frame
    void Update()
    {
        // Just update the costs periodically
        timer += Time.deltaTime;
        if (timer > refreshTime)
        {
            graph.updateCosts();
            timer = 0f;
        }
    }
}
