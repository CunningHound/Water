using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// mostly following Habrador tutorial
// https://www.habrador.com/tutorials/unity-boat-tutorial/

public struct BuoyancyTriangle
{
    public Vector3 p1;
    public Vector3 p2; 
    public Vector3 p3;

    public Vector3 centre;

    public float distanceToSurface;
    public Vector3 normal;

    public float area;

    public Vector3 velocity;
    public Vector3 velocityDir;
    public float cosTheta;

    public BuoyancyTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Rigidbody rb)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;

        this.centre = (p1 + p2 + p3) / 3f;

        this.distanceToSurface = -this.centre.y; // TODO

        this.normal = Vector3.Cross(p2 - p1, p3 - p1).normalized;

        float a = Vector3.Distance(p1, p2);
        float b = Vector3.Distance(p3, p1);
        this.area = (a * b * Mathf.Sin(Vector3.Angle(p2 - p1, p3 - p1) * Mathf.Deg2Rad)) / 2f;

        if(float.IsNaN(this.area))
        {
            Debug.LogError("got NaN area for triangle " + p1 + ", " + p2 + ", " + p3);
        }

        this.velocity = BuoyancyPhysics.GetTriangleVelocity(rb, this.centre);
        this.velocityDir = velocity.normalized;
        this.cosTheta = Vector3.Dot(this.velocityDir, this.normal);
    }
}

public class SlammingForceData
{
    //The area of the original triangles - calculate once in the beginning because always the same
    public float originalArea;
    //How much area of a triangle in the whole boat is submerged
    public float submergedArea;
    //Same as above but previous time step
    public float previousSubmergedArea;
    //Need to save the center of the triangle to calculate the velocity
    public Vector3 triangleCenter;
    //Velocity
    public Vector3 velocity;
    //Same as above but previous time step
    public Vector3 previousVelocity;
}
