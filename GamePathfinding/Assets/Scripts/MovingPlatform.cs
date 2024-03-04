using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField]
    private Vector3[] Positions;
    [SerializeField]
    private float DockDuration = 2f;
    [SerializeField]
    private float MoveSpeed = 0.01f;

    private BoxCollider boundary;

    private List<NavMeshAgent> AgentsOnPlatform = new List<NavMeshAgent>();

    private void Start()
    {
        boundary = GetComponent<BoxCollider>();
        StartCoroutine(MovePlatform());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<NavMeshAgent>(out NavMeshAgent agent))
        {
            AgentsOnPlatform.Add(agent);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<NavMeshAgent>(out NavMeshAgent agent))
        {
            AgentsOnPlatform.Remove(agent);
        }
    }

    private IEnumerator MovePlatform()
    {
        transform.position = Positions[0];
        int positionIndex = 0;
        int lastPositionIndex;
        WaitForSeconds Wait = new WaitForSeconds(DockDuration);

        while(true)
        {
            lastPositionIndex = positionIndex;
            positionIndex++;
            if (positionIndex >= Positions.Length)
            {
                positionIndex = 0;
            }

            Vector3 platformMoveDirection = (Positions[positionIndex] - Positions[lastPositionIndex]).normalized;
            float distance = Vector3.Distance(transform.position, Positions[positionIndex]);
            float distanceTraveled = 0;

            Vector3 warpOffsetDirection = Vector3.zero;
            float warpOffsetDistance = 0;
            float warpOffsetPrevPercent = 0;
            float warpOffsetPercent = 0;
            float warpOffsetIncrement = 0;
            float startTime = Time.time;

            while(distanceTraveled < distance)
            {
                transform.position += platformMoveDirection * MoveSpeed * Time.deltaTime;
                distanceTraveled += platformMoveDirection.magnitude * MoveSpeed * Time.deltaTime;

                for (int i = 0; i < AgentsOnPlatform.Count; i++)
                {
                    //AgentsOnPlatform[i].destination += platformMoveDirection * MoveSpeed;
                    // Check if the user has selected a destination within the bounds of the platform
                    if (AgentsOnPlatform[i].destination != AgentsOnPlatform[i].transform.position - Vector3.up && boundary.bounds.Contains(AgentsOnPlatform[i].destination))
                    {
                        // Adjust the warp position by the new destination
                        warpOffsetDirection = AgentsOnPlatform[i].destination - AgentsOnPlatform[i].transform.position;
                        warpOffsetDirection.y = 0;
                        warpOffsetDistance = warpOffsetDirection.magnitude;
                        warpOffsetIncrement = AgentsOnPlatform[i].speed / warpOffsetDistance;
                        warpOffsetDirection.Normalize();
                        warpOffsetPercent = 0;
                        startTime = Time.time;
                    }
                    warpOffsetPrevPercent = warpOffsetPercent;
                    warpOffsetPercent = Mathf.SmoothStep(0, 1, warpOffsetIncrement * (Time.time - startTime));
                    float warpOffsetStep = warpOffsetDistance * (warpOffsetPercent - warpOffsetPrevPercent);
                    if (warpOffsetPercent >= 1)
                    {
                        warpOffsetStep = warpOffsetDistance * (1 - warpOffsetPrevPercent);
                        warpOffsetPercent = 1;
                    }
                    AgentsOnPlatform[i].Warp(AgentsOnPlatform[i].transform.position + platformMoveDirection * MoveSpeed * Time.deltaTime + warpOffsetDirection * warpOffsetStep);
                }

                yield return null;
            }

            yield return Wait;
        }
    }
}
