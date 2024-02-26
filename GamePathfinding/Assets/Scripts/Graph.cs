using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph
{
    List<Connection> graph;

    public List<Connection> getConnections(Node fromNode)
    {
        List<Connection> connections = new List<Connection>();
        connections.AddRange(graph.FindAll(x => x.fromNode == fromNode));
        return connections;
    }

    public void generateGraph()
    {
        graph = new List<Connection>();

        Node[] nodes = GameObject.FindObjectsOfType<Node>();
        foreach (Node fromNode in nodes)
        {
            foreach (Node toNode in fromNode.linkedNodes)
            {
                Connection connection = new Connection();
                connection.cost = calculateCost(fromNode, toNode);
                connection.toNode = toNode;
                connection.fromNode = fromNode;
                graph.Add(connection);
            }
        }
    }

    public void updateCosts()
    {
        foreach (Connection connection in graph)
            connection.cost = calculateCost(connection.fromNode, connection.toNode);
    }

    protected virtual float calculateCost(Node fromNode, Node toNode)
    {
        // Default to a simple cost based on distance
        return Vector3.Distance(fromNode.transform.position, toNode.transform.position);
    }
}
public class Connection
{
    public Node fromNode;
    public Node toNode;
    public float cost;
}