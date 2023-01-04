using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaterMeshTile : MonoBehaviour
{
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
        Debug.Log("setting depths!");
        Vector3[] vertices = mesh.vertices;
        Terrain terrain = Terrain.activeTerrain;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i];
            v.y = Mathf.Clamp(GetDepthAt(terrain, v + gameObject.transform.position) - 1f, -5f, 0f);
            v.y /= -5f;
            vertices[i] = v;
        }
        mesh.vertices = vertices;
    }

    private float GetDepthAt(Terrain terrain, Vector3 pos)
    {
        float height = terrain.SampleHeight(pos);
        height += terrain.transform.position.y;
        return height;
    }

}
