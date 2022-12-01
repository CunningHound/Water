using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// mostly following Habrador tutorial
// https://www.habrador.com/tutorials/unity-boat-tutorial/

public class BuoyancyPhysics
{
    public const float RHO_WATER = 1027f;
    public const float RHO_AIR = 1.225f;

    [Header("Pressure Drag")]
    public const float C_PD1 = 10f;
    public const float C_PD2 = 100f;
    public const float f_P = 0.5f;

    [Header("Suction Drag")]
    public const float C_SD1 = 10f;
    public const float C_SD2 = 100f;
    public const float f_S = 0.5f;

    public static Vector3 GetTriangleVelocity(Rigidbody rb, Vector3 triangleCentre)
    {
        Vector3 v_rb = rb.velocity;
        Vector3 omega_rb = rb.angularVelocity;

        Vector3 r_rbt = triangleCentre - rb.worldCenterOfMass;
        Vector3 v_t = v_rb + Vector3.Cross(omega_rb, r_rbt);
        return v_t;
    }

    public static float GetTriangleArea(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float a = Vector3.Distance(p1, p2);
        float b = Vector3.Distance(p3, p1);

        float area = a * b * Mathf.Sin(Vector3.Angle(p2 - p1, p3 - p1) * Mathf.Deg2Rad) / 2f;
        return area;
    }

    public static Vector3 BuoyancyForce(float rho, BuoyancyTriangle triangleData)
    {
        //Debug.Log("rho = " + rho);
        //Debug.Log("gravity = " + Physics.gravity.y);
        //Debug.Log("distanceToSurface = " + triangleData.distanceToSurface);
        //Debug.Log("area = " + triangleData.area);
        //Debug.Log("normal = " + triangleData.normal);
        //if (float.IsNaN(triangleData.area))
        //{
        //    Debug.Log("no area for triangle " + triangleData.p1 + "," + triangleData.p2 + "," + triangleData.p3);
        //}
        //else
        //{
        //    Debug.Log("got triangle with valid area");
        //}
        Vector3 buoyancyForce = rho * Physics.gravity.y * triangleData.distanceToSurface * triangleData.area * triangleData.normal;

        //The vertical component of the hydrostatic forces don't cancel out but the horizontal do
        buoyancyForce.x = 0f;
        buoyancyForce.z = 0f;

        //Debug.Log("triangleData.distanceToSurface = " + triangleData.distanceToSurface + ", area= " + triangleData.area + ", normal = " + triangleData.normal);
        //Debug.Log("force = " + buoyancyForce);
        return buoyancyForce;
    }

    public static Vector3 WaterFrictionalResistance(float rho, BuoyancyTriangle triangleData, float C_f)
    {
        Vector3 v = triangleData.velocity;
        Vector3 n = triangleData.normal;

        Vector3 velocityTangent = (Vector3.Cross(v, Vector3.Cross(n,v)/v.magnitude)).normalized * -1f;

        Vector3 v_f = triangleData.velocity.magnitude * velocityTangent;

        Vector3 force = 0.5f * rho * v_f.magnitude * v_f * triangleData.area * C_f;

        return EnsureValidForce(force);
    }

    public static Vector3 PressureDrag(BuoyancyTriangle triangleData)
    {
        float speed = triangleData.velocity.magnitude;

        Vector3 pressureDragForce = Vector3.zero;

        if(triangleData.cosTheta > 0f)
        {
            pressureDragForce = -(C_PD1 * speed + C_PD2 * speed * speed) * triangleData.area * Mathf.Pow(triangleData.cosTheta, f_P) * triangleData.normal;
        }
        else
        {
            pressureDragForce = (C_SD1 * speed + C_SD2 * speed * speed) * triangleData.area * Mathf.Pow(-triangleData.cosTheta, f_S) * triangleData.normal;
        }
        return EnsureValidForce(pressureDragForce);
    }

    public static Vector3 SlammingForce(SlammingForceData slammingData, BuoyancyTriangle triangleData, float boatArea, float boatMass)
    {
        // TODO: slamming force
        return new Vector3();
    }

    public static float ResistanceCoefficient(float rho, float velocity, float length)
    {
        float nu = 0.000001f;

        float Rn = velocity * length / nu;

        float C_f = 0.075f / Mathf.Pow((Mathf.Log10(Rn) - 2f), 2f);
        return C_f;
    }

    public static Vector3 AirResistance(float rho, BuoyancyTriangle triangleData, float C_d)
    {
        //no air resistance if there's no component in direction of velocity
        if (triangleData.cosTheta< 0f)
        {
            return Vector3.zero;
        }

        Vector3 airResistance = -0.5f * rho * triangleData.velocity.magnitude * triangleData.velocity * triangleData.area * C_d;

        return EnsureValidForce(airResistance);
    }

    private static Vector3 EnsureValidForce(Vector3 force)
    {
        if (float.IsNaN(force.x + force.y + force.z))
        {
            Debug.Log("invalid force! " + force);
            return Vector3.zero;
        }
        return force;
    }

}
