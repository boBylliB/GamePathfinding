using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockCOM : Kinematic
{
    public List<Kinematic> flock;

    public float sepWeight = 1f;
    public float alignWeight = 1f;
    public float matchWeight = 1f;
    public float cohesionWeight = 1f;
    public float avoidWeight = 1f;
    public float dodgeWeight = 1f;

    public void Start()
    {
        // Get all members of the flock
        Flocker[] flockArray = FindObjectsOfType<Flocker>();
        foreach (Flocker member in flockArray)
        {
            flock.Add(member);
        }
        Debug.Log("Found " + flock.Count + " members of the flock");
    }

    protected override void Update()
    {
        // We're pretty much completely overriding standard kinematic behavior
        // Since this is supposed to represent the center of mass of a group of kinematics,
        // we aren't exactly always going to follow the same rules as a single kinematic
        // This is why we set position and velocity directly instead of using accelerations

        // Move to the average position of the flock
        Vector3 position = Vector3.zero;
        foreach (Flocker member in flock)
            position += member.transform.position;
        position /= flock.Count;
        transform.position = position;

        // Set velocity to the average velocity of the flock
        Vector3 velocity = Vector3.zero;
        foreach (Flocker member in flock)
            velocity += member.linearVelocity;
        velocity /= flock.Count;
        linearVelocity = velocity;

        // Rotate to the average orientation of the flock
        Vector3 orientation = Vector3.zero;
        foreach (Flocker member in flock)
            orientation += member.transform.eulerAngles;
        orientation /= flock.Count;
        transform.eulerAngles = orientation;

        // Set rotation to the average rotation of the flock
        float rotation = 0;
        foreach (Flocker member in flock)
            rotation += member.angularVelocity;
        rotation /= flock.Count;
        angularVelocity = rotation;
    }
}
