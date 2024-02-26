using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficNavigator : Kinematic
{
    //FollowPath myMoveType;
    Seek myMoveType;
    Separation mySeparateType;
    LookWhereGoing myRotateType;

    public TrafficGraph graph;

    public float regenerateDist = 1f;

    public float maxAngularAccel = 100f;
    public float maxRot = 45f;

    public Path path;
    public float pathOffset = 1f;
    public bool predictive = false;

    public float endSelectDist = 10f;
    public GameObject endMarkerPrefab;
    private GameObject endMarker;

    public float sepThreshold = 1f;
    public float friction = 1f;

    public Material mat;
    public Color startColor;
    public Color endColor;

    private TrafficNode[] nodes;
    private TrafficNode start;
    private TrafficNode goal;
    private int startIndex;
    private int endIndex;

    private List<Connection> foundPath;
    private int pathIter = 0;
    private float travelTimer = -1f;

    // Start is called before the first frame update
    void Start()
    {
        nodes = FindObjectsOfType<TrafficNode>();

        // Start at the closest node
        startIndex = -1;
        float minDist = float.MaxValue;
        for (int idx = 0; idx < nodes.Length; idx++)
        {
            TrafficNode node = nodes[idx];
            float dist = Vector3.Distance(node.transform.position, transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                start = node;
                startIndex = idx;
            }
        }

        selectEndNode();

        endMarker = Instantiate(endMarkerPrefab, goal.transform.position, Quaternion.identity);

        graph = new TrafficGraph();
        graph.generateGraph();
        foundPath = Dijkstra.pathfind(graph, start, goal);
        foreach (Connection connection in foundPath)
        {
            path.createPathTarget(connection.fromNode.transform.position);
        }
        path.createPathTarget(goal.transform.position);

        //myMoveType = new FollowPath();
        //myMoveType.character = this;
        //myMoveType.path = path;
        //myMoveType.pathOffset = pathOffset;
        //myMoveType.predictive = predictive;
        myMoveType = new Seek();
        myMoveType.character = this;
        myMoveType.target = foundPath[0].fromNode.gameObject;

        TrafficNavigator[] navigators = FindObjectsOfType<TrafficNavigator>();
        List<Kinematic> others = new List<Kinematic>();
        foreach (TrafficNavigator nav in navigators)
            if (nav != this)
                others.Add(nav);
        mySeparateType = new Separation();
        mySeparateType.character = this;
        mySeparateType.targets = others;
        mySeparateType.threshold = sepThreshold;
        mySeparateType.maxAcceleration = 1f;

        myRotateType = new LookWhereGoing();
        myRotateType.character = this;
        myRotateType.target = myTarget;
        myRotateType.maxAngularAcceleration = maxAngularAccel;
        myRotateType.maxRotation = maxRot;

        Material copyOfMat = new Material(mat);
        mat = copyOfMat;
        GetComponent<Renderer>().material = mat;
    }

    // Update is called once per frame
    protected override void Update()
    {
        travelTimer += Time.deltaTime;
        Debug.Log("PathTargetCount = " + path.pathTargets.Count + " pathIter = " + pathIter + " FoundPathCount = " + foundPath.Count + " name = " + name);
        if (Vector3.Distance(transform.position, foundPath[0].fromNode.transform.position) < regenerateDist)
            myMoveType.target = foundPath[0].toNode.gameObject;
        // Every time we get within the regenerate distance of our target node, update the travel time
        if (Vector3.Distance(transform.position, foundPath[pathIter].toNode.transform.position) < regenerateDist)
        {
            foundPath[pathIter].fromNode.GetComponent<TrafficNode>().addTime(foundPath[pathIter].toNode, travelTimer);
            foundPath[pathIter].toNode.GetComponent<TrafficNode>().addTime(foundPath[pathIter].fromNode, travelTimer);
            pathIter++;
            travelTimer = 0;

            if (pathIter < foundPath.Count)
                myMoveType.target = foundPath[pathIter].toNode.gameObject;
        }

        // If we've reached the end of the path, run the algorithm again, heading to a random node
        if (Vector3.Distance(transform.position, goal.transform.position) < regenerateDist)
        {
            Debug.Log("Reached End");
            // Start at our current goal
            startIndex = endIndex;
            start = nodes[startIndex];

            selectEndNode();
            endMarker.transform.position = goal.transform.position;

            // Update graph costs
            graph.updateCosts();

            foundPath = Dijkstra.pathfind(graph, start, goal);
            foreach (Connection connection in foundPath)
            {
                // Deliberately avoid duplicating the new start target
                if (connection.fromNode == start) continue;

                path.createPathTarget(connection.fromNode.transform.position);
            }
            path.createPathTarget(goal.transform.position);

            pathIter = 0;
            travelTimer = 0;

            myMoveType.target = foundPath[0].fromNode.gameObject;
        }

        //float param = myMoveType.currentParam % 1;
        //mat.color = startColor * (1 - param) + endColor * param;

        steeringUpdate = new SteeringOutput();
        // Experimental mode that uses the separate steering output to cause a slowdown
        steeringUpdate.linear = myMoveType.getSteering().linear * (1 - Mathf.Min(mySeparateType.getSteering().linear.magnitude, 0.99f));// + 50*mySeparateType.getSteering().linear;
        // Apply friction
        steeringUpdate.linear -= linearVelocity * friction;
        steeringUpdate.angular = myRotateType.getSteering().angular;
        base.Update();
    }

    private void selectEndNode()
    {
        // Grab selectable nodes
        List<TrafficNode> selectableNodes = new List<TrafficNode>();
        foreach (TrafficNode node in nodes)
        {
            if (node.selectable && node != start && Vector3.Distance(transform.position, node.transform.position) >= endSelectDist)
                selectableNodes.Add(node);
        }
        // Pick a random node to end at
        int randInt = Random.Range(0, selectableNodes.Count);
        goal = selectableNodes[randInt];
        for (int idx = 0; idx < nodes.Length; idx++)
            if (goal == nodes[idx])
                endIndex = idx;
        Debug.Log("Distance to start: " + Vector3.Distance(transform.position, start.transform.position) + " Distance to goal: " + Vector3.Distance(transform.position, goal.transform.position));
    }

    private void OnDrawGizmos()
    {
        if (goal == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, (goal.transform.position - transform.position).normalized * 2);
    }
}
