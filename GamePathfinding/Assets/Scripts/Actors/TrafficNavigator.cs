using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficNavigator : Kinematic
{
    FollowPath myMoveType;
    Separation mySeparateType;
    LookWhereGoing myRotateType;

    public TrafficGraph graph;

    public float regenerateDist = 1f;

    public float maxAngularAccel = 100f;
    public float maxRot = 45f;

    public Path path;
    public float pathOffset = 1f;
    public bool predictive = false;

    private TrafficNode[] nodes;
    private TrafficNode start;
    private TrafficNode goal;

    private float travelTimer = -1f;

    // Start is called before the first frame update
    void Start()
    {
        nodes = FindObjectsOfType<TrafficNode>();

        selectNodes();

        graph = new TrafficGraph();
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

        TrafficNavigator[] navigators = FindObjectsOfType<TrafficNavigator>();
        List<Kinematic> others = new List<Kinematic>();
        foreach (TrafficNavigator nav in navigators)
            if (nav != this)
                others.Add(nav);
        mySeparateType = new Separation();
        mySeparateType.character = this;
        mySeparateType.targets = others;
        mySeparateType.maxAcceleration = 3f;

        myRotateType = new LookWhereGoing();
        myRotateType.character = this;
        myRotateType.target = myTarget;
        myRotateType.maxAngularAcceleration = maxAngularAccel;
        myRotateType.maxRotation = maxRot;
    }

    // Update is called once per frame
    protected override void Update()
    {
        // Every time we get within the regenerate distance of a node other than the start node, update the travel time
        foreach (TrafficNode node in nodes)
        {
            if (Vector3.Distance(transform.position, node.transform.position) < regenerateDist && node != start)
            {

            }
        }

        // If we've reached the end of the path, run the algorithm again, heading to a random node
        if (Vector3.Distance(transform.position, goal.transform.position) < regenerateDist)
        {
            selectNodes();

            // Update graph costs
            graph.updateCosts();

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
        steeringUpdate.linear = myMoveType.getSteering().linear + 100*mySeparateType.getSteering().linear;
        steeringUpdate.angular = myRotateType.getSteering().angular;
        base.Update();
    }

    private void selectNodes()
    {
        // Start at the closest node
        int startIdx = -1;
        float minDist = float.MaxValue;
        for (int idx = 0; idx < nodes.Length; idx++)
        {
            TrafficNode node = nodes[idx];
            float dist = Vector3.Distance(node.transform.position, transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                start = node;
                startIdx = idx;
            }
        }
        // Pick a random node to end at
        int randInt = Random.Range(0, nodes.Length - 1); // Subtract one since we want to skip start
        // Effectively skip start by adding one if we're above or at the start index
        if (randInt >= startIdx) ++randInt;
        goal = nodes[randInt];
        Debug.Log("Distance to start: " + Vector3.Distance(transform.position, start.transform.position) + " Distance to goal: " + Vector3.Distance(transform.position, goal.transform.position));
    }
}
