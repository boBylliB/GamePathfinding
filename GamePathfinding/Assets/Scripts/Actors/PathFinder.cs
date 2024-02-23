using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder : Kinematic
{
    FollowPath myMoveType;
    LookWhereGoing myRotateType;

    public Graph graph;
    public Node start;
    public Node goal;

    public float regenerateDist = 1f;

    public float maxAngularAccel = 100f;
    public float maxRot = 45f;

    public Path path;
    public float pathOffset = 1f;
    public bool predictive = false;

    // Start is called before the first frame update
    void Start()
    {
        graph = new Graph();
        graph.generateGraph();
        List<Connection> foundPath = Dijkstra.pathfind(graph, start, goal);
        foreach (Connection connection in foundPath)
        {
            path.createPathTarget(connection.fromNode.transform.position);
        }
        path.createPathTarget(goal.transform.position);

        myMoveType = new FollowPath();
        myMoveType.character = this;
        myMoveType.path = path;
        myMoveType.pathOffset = pathOffset;
        myMoveType.predictive = predictive;

        myRotateType = new LookWhereGoing();
        myRotateType.character = this;
        myRotateType.target = myTarget;
        myRotateType.maxAngularAcceleration = maxAngularAccel;
        myRotateType.maxRotation = maxRot;
    }

    // Update is called once per frame
    protected override void Update()
    {
        // If we've reached the end of the path, run the algorithm again, heading the opposite direction
        if (path.pathTargets.Count < 2 || Vector3.Distance(transform.position, goal.transform.position) < regenerateDist)
        {
            Node temp = start;
            start = goal;
            goal = temp;

            List<Connection> foundPath = Dijkstra.pathfind(graph, start, goal);
            foreach (Connection connection in foundPath)
            {
                // Deliberately avoid duplicating the new start target
                if (connection.fromNode == start) continue;

                path.createPathTarget(connection.fromNode.transform.position);
            }
            path.createPathTarget(goal.transform.position);
        }

        steeringUpdate = new SteeringOutput();
        steeringUpdate.linear = myMoveType.getSteering().linear;
        steeringUpdate.angular = myRotateType.getSteering().angular;
        base.Update();
    }
}
