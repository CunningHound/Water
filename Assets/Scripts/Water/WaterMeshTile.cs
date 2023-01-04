using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaterMeshTile : MonoBehaviour
{
    public float size;
    public int resolution;

    // Start is called before the first frame update
    void Start()
    {
        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        if(filter != null)
        {
            Debug.Log(gameObject.transform.position);
            Mesh mesh = GenerateMesh(size, resolution);
            filter.mesh = mesh;
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
                point.y = Mathf.Clamp( GetDepthAt(point + gameObject.transform.position) - 1f, -5f, 5f);
                point.y = -point.y/5f;
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

    private float GetDepthAt(Vector3 pos)
    {
        Terrain terrain = GetNearestTerrain(pos);
        float height = terrain.SampleHeight(pos);
        height += terrain.transform.position.y;
        return height;
    }

    private Terrain GetNearestTerrain(Vector3 pos)
    {
        Terrain[] terrains = Terrain.activeTerrains;
        return terrains.OrderBy(x =>
        {
            Vector3 terrainPos = x.transform.position;
            Vector3 terrainSize = x.terrainData.size * 0.5f;
            Vector3 terrainCentre = new Vector3(terrainPos.x + terrainSize.x, terrainPos.y + terrainSize.y, 0);
            return Vector3.Distance(terrainCentre, pos);
        }).First();

    }
}
