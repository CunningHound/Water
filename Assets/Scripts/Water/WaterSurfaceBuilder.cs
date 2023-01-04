using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSurfaceBuilder : MonoBehaviour
{
    public GameObject surfacePrefab;
    public Vector2 origin;
    public float squareDimensions;
    public int squareResolution;
    public Vector2 numberOfSquares;

    void Awake()
    {
        if(surfacePrefab != null)
        {
            MeshFilter meshFilter = surfacePrefab.GetComponent<MeshFilter>();
            meshFilter.mesh = GenerateMesh(squareDimensions, squareResolution);
            for(int i=0; i<numberOfSquares.x; i++)
            {
                for(int j=0; j<numberOfSquares.y; j++)
                {
                    Instantiate(surfacePrefab, new Vector3(origin.x + i*squareDimensions, 0, origin.y + j*squareDimensions), Quaternion.identity);
                }
            }
        }
    }
    private Mesh GenerateMesh(float size, int resolution)
    {
        Mesh mesh = new Mesh();
        int vertexCount = 0;
        int triangleIndex = 0;

        int points = resolution + 1;
        Vector3[] vertices = new Vector3[(points + 1) * (points + 1)];
        int[] triangles = new int[points * points * 2 * 3];
        Vector2[] uvs = new Vector2[(points + 1) * (points + 1)];
        if ((points + 1) * (points + 1) >= 256 * 256)
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        for (int i = 0; i < points + 1; i++)
        {
            float fractionOfWidth = (float)i / (points);
            for (int j = 0; j < points + 1; j++)
            {
                int k = i * (points + 1) + j;
                float fractionOfHeight = (float)j / (points);
                Vector3 point = new Vector3( (fractionOfWidth*size), 0, (fractionOfHeight * size));
                /*point.y = Mathf.Clamp( GetDepthAt(point + gameObject.transform.position) - 1f, -5f, 5f);
                point.y = -point.y/5f;*/
                point.y = 1;
                //Debug.Log("setting y = " + point.y);
                vertices[vertexCount++] = point;
                if (i < points && j < points)
                {
                    Vector2 uv = new Vector2((float)i / (float)(points-1), (float)j / (float)(points-1));
                    uvs[i + j * points] = uv;
                    triangles[triangleIndex++] = k;
                    triangles[triangleIndex++] = k + 1;
                    triangles[triangleIndex++] = k + points + 1;

                    triangles[triangleIndex++] = k + 1;
                    triangles[triangleIndex++] = k + points + 2;
                    triangles[triangleIndex++] = k + points + 1;
                }
            }
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        return mesh;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
