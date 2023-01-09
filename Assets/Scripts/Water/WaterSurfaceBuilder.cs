using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSurfaceBuilder : MonoBehaviour
{
    public GameObject LODGroupPrefab;
    public Vector2 origin;
    public float squareDimensions;
    public int[] squareResolutions;
    public Vector2 numberOfSquares;

    public Material waterMaterial;

    void Awake()
    {
        if(LODGroupPrefab != null)
        {
            LODGroup lodGroup = LODGroupPrefab.GetComponent<LODGroup>();
            LOD[] lods = lodGroup.GetLODs();
            for (int i = 0; i < squareResolutions.Length; i++)
            {
                GameObject lod = lods[i].renderers[0].gameObject;
                MeshFilter meshFilter = lod.GetComponent<MeshFilter>();
                meshFilter.mesh = GenerateMesh(squareDimensions, squareResolutions[i]);
                Renderer[] renderers = new Renderer[1];
                renderers[0] = lod.GetComponent<MeshRenderer>();
                lods[i] = new LOD(lods[i].screenRelativeTransitionHeight, renderers);
            }
            lodGroup.SetLODs(lods);

            for (int x = 0; x < numberOfSquares.x; x++)
            {
                for (int y = 0; y < numberOfSquares.y; y++)
                {
                    Instantiate(LODGroupPrefab, new Vector3(origin.x + x * squareDimensions, 0, origin.y + y * squareDimensions), Quaternion.identity);
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
