using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// mostly following Habrador tutorial
// https://www.habrador.com/tutorials/unity-boat-tutorial/

public class Buoyancy : MonoBehaviour
{
    public GameObject buoyantObject;
    public GameObject underwaterObject;
    public GameObject aboveWaterObject;

    private BuoyancyMesh buoyancyMesh;
    private Mesh underwaterMesh;
    private Mesh aboveWaterMesh;

    private Rigidbody rb;

    public GameObject centreOfMass;

    public float C_d;

    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }
    void Start()
    {
        if(centreOfMass != null)
        {
            rb.centerOfMass = centreOfMass.transform.position;
        }

        buoyancyMesh = new BuoyancyMesh(gameObject, underwaterObject, aboveWaterObject, rb);
        underwaterMesh = gameObject.GetComponent<MeshFilter>().mesh;
        aboveWaterMesh = gameObject.GetComponent<MeshFilter>().mesh;
    }

    private void Update()
    {
        if(centreOfMass != null)
        {
            rb.centerOfMass = centreOfMass.transform.position;
        }
        buoyancyMesh.GenerateMeshes();
        //buoyancyMesh.DisplayMesh(underwaterMesh, "underwater mesh", buoyancyMesh.underwaterTriangles);
        //buoyancyMesh.DisplayMesh(abovewaterMesh, "above water mesh", buoyancyMesh.aboveWaterTriangles);
    }

    void FixedUpdate()
    {
        if(buoyancyMesh.underwaterTriangles.Count > 0)
        {
            AddUnderwaterForces();
        }
        if(buoyancyMesh.aboveWaterTriangles.Count > 0)
        {
            AddAboveWaterForces();
        }
    }

    void AddUnderwaterForces()
    {
        List<BuoyancyTriangle> underwaterTriangles = buoyancyMesh.underwaterTriangles;

        float C_f = BuoyancyPhysics.ResistanceCoefficient(
            BuoyancyPhysics.RHO_WATER,
            rb.velocity.magnitude,
            buoyancyMesh.CalculateUnderwaterLength());

        for(int i=0; i< underwaterTriangles.Count; i++)
        {
            BuoyancyTriangle triangleData = underwaterTriangles[i];

            Vector3 force = Vector3.zero;
            force += BuoyancyPhysics.BuoyancyForce(BuoyancyPhysics.RHO_WATER, triangleData);

            force += BuoyancyPhysics.WaterFrictionalResistance(BuoyancyPhysics.RHO_WATER, triangleData, C_f);

            force += BuoyancyPhysics.PressureDrag(triangleData);

            // TODO: slamming forces
            //force +=

            rb.AddForceAtPosition(force, triangleData.centre);

            //Debug.DrawRay(triangleData.centre, triangleData.normal * 3f, Color.white);
            //Debug.DrawRay(triangleData.centre, buoyancyForce.normalized * -3f, Color.blue);
        }
    }

    void AddAboveWaterForces()
    {
        List<BuoyancyTriangle> aboveWaterTriangleData = buoyancyMesh.aboveWaterTriangles;

        for (int i = 0; i < aboveWaterTriangleData.Count; i++)
        {
            BuoyancyTriangle triangleData = aboveWaterTriangleData[i];

            Vector3 force = Vector3.zero;
            force += BuoyancyPhysics.AirResistance(BuoyancyPhysics.RHO_AIR, triangleData, C_d);
            rb.AddForceAtPosition(force, triangleData.centre);
        }
    }

}
