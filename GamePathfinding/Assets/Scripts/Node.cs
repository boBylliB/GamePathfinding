using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Node> linkedNodes;

    // Really liked Professor Slease's implementation, so I kinda just nabbed it
    private void OnDrawGizmos()
    {
        foreach (Node node in linkedNodes)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, (node.transform.position - transform.position).normalized * 2);
        }
    }
}
