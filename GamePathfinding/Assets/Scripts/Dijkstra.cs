using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dijkstra
{
    public static List<Connection> pathfind(Graph graph, Node start, Node goal)
    {
        // Initialize the record for the start node
        NodeRecord startRecord = new NodeRecord();
        startRecord.node = start;
        startRecord.connection = null;
        startRecord.costSoFar = 0f;

        // Initialize the open and closed lists
        PathfindingList open = new PathfindingList();
        open.Add(startRecord);
        PathfindingList closed = new PathfindingList();

        // Iterate through processing each node
        NodeRecord current = null;
        while (open.Count() > 0)
        {
            // Find the smallest element in the open list
            current = open.smallestRecord();

            // If it is the goal node, then terminate
            if (current.node == goal)
                break;

            // Otherwise, get outgoing connections
            List<Connection> connections = graph.getConnections(current.node);

            // Loop through each connection in turn
            foreach (Connection connection in connections)
            {
                // Get the cost estimate for the end node
                Node endNode = connection.toNode;
                float endNodeCost = current.costSoFar + connection.cost;

                // Skip if the node is closed or if the route is worse
                // If no node was found, we have an unvisited node, and we should make a record
                NodeRecord endNodeRecord = open.Find(endNode);
                bool recordExisted = false;
                if (closed.Find(endNode) != null)
                    continue;
                else if (endNodeRecord != null)
                {
                    recordExisted = true;
                    if (endNodeRecord.costSoFar <= endNodeCost)
                        continue;
                }
                else
                {
                    endNodeRecord = new NodeRecord();
                    endNodeRecord.node = endNode;
                }

                // Update the node
                endNodeRecord.costSoFar = endNodeCost;
                endNodeRecord.connection = connection;

                // Add it to the open list
                if (!recordExisted)
                    open.Add(endNodeRecord);
            }

            // We've finished looking at the connections for the current node, so add it to the closed list and remove it from the open list
            open.Remove(current);
            closed.Add(current);
        }

        // We're here if we've either found the goal, or if we have no more nodes to search, and we need to determine which
        if (current == null || current.node != goal)
            return null;
        else
        {
            // Compile a list of path connections
            List<Connection> path = new List<Connection>();

            // Work back along the path, accumulating connections
            while (current.node != start)
            {
                path.Add(current.connection);
                Node fromNode = current.connection.fromNode;
                current = closed.Find(fromNode);
            }

            // Reverse the path and return it
            path.Reverse();
            return path;
        }
    }
}
public class PathfindingList
{
    private List<NodeRecord> records;
    private NodeRecord minRecord;
    private bool minInitialized;

    public PathfindingList()
    {
        minInitialized = false;
        records = new List<NodeRecord>();
    }

    public List<NodeRecord> getRecords()
    {
        return records;
    }
    public NodeRecord smallestRecord()
    {
        return minRecord;
    }
    public void Add(NodeRecord record)
    {
        records.Add(record);
        if (!minInitialized || record.costSoFar < minRecord.costSoFar)
            minRecord = record;
    }
    public void Remove(NodeRecord record)
    {
        records.Remove(record);
        if (minRecord.node == record.node)
        {
            // If the minimum record is removed, this is the only time we actually have to search the list for a minimum
            List<NodeRecord> tempList = records;
            tempList.Sort();
            minRecord = tempList[0];
        }
    }
    public NodeRecord Get(int index)
    {
        return records[index];
    }
    public int Count()
    {
        return records.Count;
    }
    public bool Contains(NodeRecord record)
    {
        return records.Contains(record);
    }
    public NodeRecord Find(Node node)
    {
        return records.Find(x => x.node == node);
    }
    public List<NodeRecord> Reverse()
    {
        List<NodeRecord> tempList = records;
        tempList.Reverse();
        return tempList;
    }
}
public class NodeRecord : IComparable<NodeRecord>
{
    public Node node;
    public Connection connection;
    public float costSoFar;
    
    // hit some WACK error with trying the built in sort functionality instead of just copy-pasting in my previous quicksort implementation
    // "its more efficient" "its more readable" "its better practice" - statements dreamed up by the utterly deranged
    // i am burdened by the best practices of my time
    // In other news, thanks again to Professor Slease for the fantastically documented CompareTo implementation that i can yoink- i mean, utilize
    public int CompareTo(NodeRecord other)
    {
        // Implementation pulled almost verbatim from https://github.com/bslease/Dynamic_Steering/blob/master/Dynamic%20Steering%20Basics/Assets/Scripts/Dijkstra.cs

        // CompareTo returns a value that indicates the relative order of the objects being compared.
        // The return value has these meanings:
        //   negative - this instance precedes the other in sort order
        //   zero     - this instance occurs in the same position in the sort order as other
        //   positive - this instance follows other in the sort order

        // This is a standard implementation feature I couldn't find an explanation for
        if (other == null)
        {
            return 1;
        }

        // We want to sort lowest costsofar to highest, so:
        //   if our costsofar is lower than other, return a negative value
        //   if we're exactly the same, return 0
        //   if our costsofar is larger than other, return a positive value
        return (int)(costSoFar - other.costSoFar);
    }
}