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
        if(filter != null && filter.mesh != null)
        {
            SetDepths(filter.mesh);
            Debug.Log(gameObject.transform.position);
        }
    }

    private void SetDepths(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i];
            v.y = Mathf.Clamp(GetDepthAt(v + gameObject.transform.position) - 1f, -5f, 5f);
            v.y /= -5f;
            vertices[i] = v;
        }
        mesh.vertices = vertices;
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
