using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// mostly following Habrador tutorial
// https://www.habrador.com/tutorials/unity-boat-tutorial/

public class BuoyancyMesh
{
    private Transform objectTransform;
    private Vector3[] vertices;
    private int[] triangles;

    private Rigidbody rb;

    public Vector3[] verticesGlobal;
    private float[] distancesToWater;

    public List<BuoyancyTriangle> underwaterTriangles = new List<BuoyancyTriangle>();
    public List<BuoyancyTriangle> aboveWaterTriangles = new List<BuoyancyTriangle>();

    private MeshCollider underwaterMeshCollider;
    private Mesh underwaterMesh;
    private Mesh aboveWaterMesh;

    //Help class to store triangle data so we can sort the distances
    private class VertexData
    {
        //The distance to water from this vertex
        public float distanceToWater;
        //An index so we can form clockwise triangles
        public int index;
        //The global Vector3 position of the vertex
        public Vector3 globalPos;
    }

    public BuoyancyMesh(GameObject buoyantObject, GameObject underwaterObject, GameObject aboveWaterObject, Rigidbody rb)
    {
        objectTransform = buoyantObject.transform;
        this.rb = rb;

        underwaterMeshCollider = underwaterObject.GetComponent<MeshCollider>();
        underwaterMesh = underwaterObject.GetComponent<MeshFilter>().mesh;
        aboveWaterMesh = aboveWaterObject.GetComponent<MeshFilter>().mesh;

        vertices = buoyantObject.GetComponent<MeshFilter>().mesh.vertices;
        triangles = buoyantObject.GetComponent<MeshFilter>().mesh.triangles;

        verticesGlobal = new Vector3[vertices.Length];
        distancesToWater = new float[vertices.Length];
    }

    public void GenerateMeshes()
    {
        underwaterTriangles.Clear();
        aboveWaterTriangles.Clear();

        // TODO: slamming data

        for(int i=0; i< vertices.Length; i++)
        {
            Vector3 globalPos = objectTransform.TransformPoint(vertices[i]);

            verticesGlobal[i] = globalPos;
            distancesToWater[i] = globalPos.y; // TODO:
        }

        AddTriangles();
    }


    public float CalculateUnderwaterLength()
    {
        return underwaterMesh.bounds.size.z;
    }

    public void AddTriangles()
    {
        List<VertexData> vertexData = new List<VertexData>();

        vertexData.Add(new VertexData());
        vertexData.Add(new VertexData());
        vertexData.Add(new VertexData());

        int i = 0;
        while (i < triangles.Length)
        {
            // three vertices per triangle
            // 'triangles' is actually an int[], the triangles are implicit
            for (int j=0; j<3; j++)
            {
                if(i>=triangles.Length)
                {
                    //Debug.Log("triangles has length " + triangles.Length + " but we're trying to get element " + i);
                }
                vertexData[j].distanceToWater = distancesToWater[triangles[i]]; // TODO: not entirely accurate
                vertexData[j].index = j;
                vertexData[j].globalPos = verticesGlobal[triangles[i]];

                i++;
            }

            // all above water
            if(vertexData[0].distanceToWater >= 0f && vertexData[1].distanceToWater >= 0f && vertexData[2].distanceToWater >= 0f)
            {
                Vector3 p1 = vertexData[0].globalPos;
                Vector3 p2 = vertexData[1].globalPos;
                Vector3 p3 = vertexData[2].globalPos;

                aboveWaterTriangles.Add(new BuoyancyTriangle(p1, p2, p3, rb));

                continue;
            }

            //create fully underwater triangles
            if(vertexData[0].distanceToWater < 0f && vertexData[1].distanceToWater < 0f && vertexData[2].distanceToWater < 0f)
            {
                Vector3 p1 = vertexData[0].globalPos;
                Vector3 p2 = vertexData[1].globalPos;
                Vector3 p3 = vertexData[2].globalPos;

                //Debug.Log("adding fully underwater");
                underwaterTriangles.Add(new BuoyancyTriangle(p1, p2, p3, rb));
            }
            else
            {
                vertexData.Sort((x, y) => x.distanceToWater.CompareTo(y.distanceToWater));
                vertexData.Reverse();
                //Debug.Log("sorted = " + vertexData[0].distanceToWater + "," + vertexData[1].distanceToWater + ", " + vertexData[2].distanceToWater);

                if (vertexData[1].distanceToWater < 0f)
                {
                    // one vertex above water level
                    AddTrianglesOneDryVertex(vertexData);
                }
                else
                {
                    //two vertices above water level
                    AddTrianglesTwoDryVertices(vertexData);
                }
            }
        }
    }

    private void AddTrianglesOneDryVertex(List<VertexData> vertexData)
    {
        // h, m, l: highest, medium, lowest vertex. always in that order because we sorted before passing here
        Vector3 h = vertexData[0].globalPos;
        //Debug.Log("sorted = " + vertexData[0].distanceToWater + "," + vertexData[1].distanceToWater + ", " + vertexData[2].distanceToWater);

        int index_m = vertexData[0].index - 1;
        if(index_m < 0)
        {
            index_m = 2;
        }

        float height_h = vertexData[0].distanceToWater;
        float height_m = 0f;
        float height_l = 0f;

        Vector3 m = Vector3.zero;
        Vector3 l = Vector3.zero;

        if(vertexData[1].index == index_m)
        {
            m = vertexData[1].globalPos;
            l = vertexData[2].globalPos;

            height_m = vertexData[1].distanceToWater;
            height_l = vertexData[2].distanceToWater;
        }
        else
        {
            m = vertexData[2].globalPos;
            l = vertexData[1].globalPos;

            height_m = vertexData[2].distanceToWater;
            height_l = vertexData[1].distanceToWater;
        }

        // cut triangle
        // TODO: improve/understand notation
        Vector3 mh = h - m;
        float t_m = -height_m / (height_h - height_m);
        Vector3 mi_m = t_m * mh;
        Vector3 i_m = mi_m + m;


        Vector3 lh = h - l;
        float t_l = -height_l / (height_h - height_l);
        Vector3 li_l = t_l * lh;
        Vector3 i_l = li_l + l;
        //Debug.Log("Adding one dry (1)");
        //Debug.Log("h_h = " + height_h + ", h_m = " + height_m + ", h_l = " + height_l);
        //Debug.Log("tm = " + t_m + ", i_m = " + i_m + ", i_l = " + i_l);

        underwaterTriangles.Add(new BuoyancyTriangle(m, i_m, i_l, rb));
        //Debug.Log("h_h = " + height_h + ", h_l = " + height_l);
        //Debug.Log("tl = " + t_l + ", li_l = " + li_l + ", i_l = " + i_l);
        //Debug.Log("Adding one dry (2)");
        underwaterTriangles.Add(new BuoyancyTriangle(m, i_l, l, rb));
    }

    private void AddTrianglesTwoDryVertices(List<VertexData> vertexData)
    {
        Vector3 l = vertexData[2].globalPos;

        // find index of h
        int index_h = vertexData[2].index + 1;
        if( index_h > 2 )
        {
            index_h = 0;
        }

        float height_l = vertexData[2].distanceToWater;
        float height_m = 0f;
        float height_h = 0f;

        Vector3 m = Vector3.zero;
        Vector3 h = Vector3.zero;

        if (vertexData[1].index == index_h)
        {
            h = vertexData[1].globalPos;
            m = vertexData[0].globalPos;

            height_h = vertexData[1].distanceToWater;
            height_m = vertexData[0].distanceToWater;
        }
        else
        {
            h = vertexData[0].globalPos;
            m = vertexData[1].globalPos;

            height_h = vertexData[0].distanceToWater;
            height_m = vertexData[1].distanceToWater;
        }

        Vector3 lm = m - l;
        float t_m = -height_l / (height_m - height_l);
        Vector3 lj_m = t_m * lm;
        Vector3 j_m = lj_m + l;

        Vector3 lh = h - l;
        float t_h = -height_l / (height_h - height_l);
        Vector3 lj_h = t_h * lh;
        Vector3 j_h = lj_h + l;

        //Debug.Log("adding two dry");
        //Debug.Log(vertexData[0].globalPos + ", " + vertexData[1].globalPos + ", " + vertexData[2].globalPos);
        underwaterTriangles.Add(new BuoyancyTriangle(l, j_h, j_m, rb));
    }

    public void DisplayMesh(Mesh mesh, string name, List<BuoyancyTriangle> triangles)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> meshTriangles = new List<int>();

        for (int i = 0; i < triangles.Count; i++)
        {
            Vector3 p1 = objectTransform.InverseTransformPoint(triangles[i].p1);
            Vector3 p2 = objectTransform.InverseTransformPoint(triangles[i].p2);
            Vector3 p3 = objectTransform.InverseTransformPoint(triangles[i].p3);

            vertices.Add(p1);
            meshTriangles.Add(vertices.Count - 1);

            vertices.Add(p2);
            meshTriangles.Add(vertices.Count - 1);

            vertices.Add(p3);
            meshTriangles.Add(vertices.Count - 1);
        }

        mesh.Clear();

        mesh.name = name;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = meshTriangles.ToArray();

        mesh.RecalculateBounds();
    }

}
