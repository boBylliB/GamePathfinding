using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficNode : Node
{
    public float travelSpeed = 1f;
    public int dataPoints = 3;
    public float dataTimeout = 30f;
    public List<float> travelTimes;

    private List<List<TravelTimeData>> travelTimeData;

    private struct TravelTimeData
    {
        public float time { get; set; }
        public float timestamp { get; set; }
    }

    public void Start()
    {
        travelTimes = new List<float>();
        travelTimeData = new List<List<TravelTimeData>>();
        foreach (Node toNode in linkedNodes)
        {
            // Default to a travel time based on the travel speed
            travelTimes.Add(Vector3.Distance(transform.position, toNode.transform.position) / travelSpeed);
            // Add empty data list for each
            travelTimeData.Add(new List<TravelTimeData>());
        }
    }

    public void addTime(Node toNode, float time)
    {
        // Find the linked node index
        int nodeIdx = -1;
        for (int idx = 0; idx < linkedNodes.Count; idx++)
            if (linkedNodes[idx] == toNode)
                nodeIdx = idx;
        // If the linked node wasn't found, throw away the data
        if (nodeIdx == -1)
        {
            Debug.LogWarning("Trying to add time from node " + name + " to node " + toNode.name + " failed, not in linked nodes list!");
            return;
        }

        // Add the time to the data for that node
        TravelTimeData datapoint = new TravelTimeData();
        datapoint.time = time;
        datapoint.timestamp = Time.time;
        travelTimeData[nodeIdx].Add(datapoint);
        updateTime(nodeIdx);
    }
    
    private void updateTime(int nodeIdx)
    {
        // Remove datapoints if we're over the data limit
        while (travelTimeData[nodeIdx].Count > dataPoints)
            travelTimeData[nodeIdx].RemoveAt(0);
        // Remove stale datapoints
        for (int idx = 0; idx < travelTimeData[nodeIdx].Count; idx++)
            if (Time.time - travelTimeData[nodeIdx][idx].timestamp > dataTimeout)
                travelTimeData[nodeIdx].RemoveAt(idx);
        // Update the average travel time for that node
        if (travelTimeData[nodeIdx].Count > 0)
        {
            float mean = 0f;
            foreach (TravelTimeData dataPoint in travelTimeData[nodeIdx])
                mean += dataPoint.time;
            mean /= travelTimeData[nodeIdx].Count;
            travelTimes[nodeIdx] = mean;
        }
        else
        {
            // Default to a travel time based on the travel speed
            travelTimes[nodeIdx] = Vector3.Distance(transform.position, linkedNodes[nodeIdx].transform.position) / travelSpeed;
        }
    }
}
