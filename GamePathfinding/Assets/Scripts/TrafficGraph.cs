using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficGraph : Graph
{
    public float travelSpeed = 1f;

    protected override float calculateCost(Node fromNode, Node toNode)
    {
        // Find the related TrafficNode
        TrafficNode fromTrafficNode = fromNode.GetComponent<TrafficNode>();
        // Find the index of the toNode in the linked nodes
        int nodeIdx = -1;
        for (int idx = 0; idx < fromTrafficNode.linkedNodes.Count; idx++)
            if (fromTrafficNode.linkedNodes[idx] == toNode)
                nodeIdx = idx;
        // If the linked node wasn't found, default to basic distance divided by travel speed
        if (nodeIdx == -1)
        {
            Debug.LogWarning("Trying to calculate cost from node " + fromTrafficNode.name + " to node " + toNode.name + " failed, not in linked nodes list!");
            return Vector3.Distance(fromTrafficNode.transform.position, toNode.transform.position) / travelSpeed;
        }
        // Otherwise, return the related travel time
        return fromTrafficNode.travelTimes[nodeIdx];
    }
}
