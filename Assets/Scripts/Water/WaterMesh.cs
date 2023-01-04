using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaterMesh : MonoBehaviour
{
    public GameObject focusObject;

    // Range for each detail level is like a (taxicab) radius: extends out to that distance (in x, z) from camera

    [SerializeField]
    private float nearResolution = 0.25f;
    [SerializeField]
    private float nearRange = 50;

    [SerializeField]
    private float midResolution = 0.5f;
    [SerializeField]
    private float midRange = 250;
    public bool enableMidRange = true;

    [SerializeField]
    private float farResolution = 1f;
    [SerializeField]
    private float farRange = 1000;
    public bool enableFarRange = true;

    [SerializeField]
    private float distantResolution = 2f;
    [SerializeField]
    private float distantRange = 2000;
    public bool enableDistantRange = true;

    public Material waterMaterial;

    private Mesh waterMesh;

    // Start is called before the first frame update
    void Start()
    {
        GameObject wm = new GameObject();
        wm.name = "water mesh";
        wm.transform.SetParent(transform);
        MeshFilter meshFilter = wm.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = wm.AddComponent<MeshRenderer>();
        meshFilter.mesh = GenerateMesh();
        waterMesh = meshFilter.mesh;
        meshRenderer.material = waterMaterial;
    }

    void FixedUpdate()
    {
        transform.position = new Vector3(focusObject.transform.position.x, 0, focusObject.transform.position.z);
    }

    private Mesh GenerateMesh()
    {
        Mesh nearMesh = GenerateSubmesh(nearRange*2f, nearRange*2f, nearResolution, -nearRange, -nearRange);
        List<Mesh> meshList = new List<Mesh> { nearMesh };

        if (enableMidRange)
        {
            Mesh midMesh = GenerateMeshRing(midRange, midRange, nearRange, nearRange, midResolution);
            meshList.Add(midMesh);

            if (enableFarRange)
            {
                Mesh farMesh = GenerateMeshRing(farRange, farRange, midRange, midRange, farResolution);
                meshList.Add(farMesh);

                if (enableDistantRange)
                {
                    Mesh distantMesh = GenerateMeshRing(distantRange, distantRange, farRange, farRange, distantResolution);
                    meshList.Add(distantMesh);
                }
            }
        }
        return CombineMeshes(meshList);
    }

    private Mesh CombineMeshes(List<Mesh> meshes)
    {
        CombineInstance[] combine = new CombineInstance[meshes.Count];
        for (int i = 0; i < meshes.Count; i++)
        {
            combine[i].mesh = meshes[i];
            combine[i].transform = transform.localToWorldMatrix;
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.CombineMeshes(combine);
        return mesh;
    }

    private Mesh GenerateMeshRing(float widthExternal, float heightExternal, float widthInternal, float heightInternal, float resolution = 1f)
    {
        CombineInstance[] combine = new CombineInstance[4];
        List<Mesh> meshList = new List<Mesh>();
        if (widthInternal > 0)
        {
            Debug.Log(widthInternal);
            //top centre
            Mesh top = GenerateSubmesh(widthInternal * 2f, heightExternal - heightInternal, resolution, -widthInternal, heightInternal);
            meshList.Add(top);

            //bottom centre
            Mesh bottom = GenerateSubmesh(widthInternal * 2f, heightExternal - heightInternal, resolution, -widthInternal, -heightExternal);
            meshList.Add(bottom);
        }

        //left strip
        Mesh left = GenerateSubmesh(widthExternal - widthInternal, heightExternal * 2, resolution, -widthExternal, -heightExternal);
        meshList.Add(left);

        //right strip
        Mesh right = GenerateSubmesh(widthExternal - widthInternal, heightExternal * 2, resolution, widthInternal, -heightExternal);
        meshList.Add(right);

        return CombineMeshes(meshList);

    }

    public Mesh GenerateSubmesh(float width, float height, float resolution = 1f, float widthOffset = 0f, float heightOffset = 0f)
    {
        Mesh mesh = new Mesh();
        int vertexCount = 0;
        int triangleIndex = 0;

        int widthPoints = (int)(width / resolution);
        int heightPoints = (int)(height / resolution);
        Vector3[] vertices = new Vector3[(widthPoints + 1) * (heightPoints + 1)];
        int[] triangles = new int[widthPoints * heightPoints * 2 * 3];
        Vector2[] uvs = new Vector2[(widthPoints + 1) * (heightPoints + 1)];
        if ((widthPoints + 1) * (heightPoints + 1) >= 256 * 256)
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        for (int i = 0; i < widthPoints + 1; i++)
        {
            float fractionOfWidth = (float)i / (widthPoints);
            for (int j = 0; j < heightPoints + 1; j++)
            {
                int k = i * (heightPoints + 1) + j;
                float fractionOfHeight = (float)j / (heightPoints);
                Vector3 point = new Vector3( (fractionOfWidth*width) + widthOffset, 0, (fractionOfHeight * height) + heightOffset);
                point.y = Mathf.Clamp( GetDepthAt(point + focusObject.transform.position) - 1f, -5f, 5f);
                point.y = -point.y/5f;
                //Debug.Log("setting y = " + point.y);
                vertices[vertexCount++] = point;
                if (i < widthPoints && j < heightPoints)
                {
                    Vector2 uv = new Vector2((float)i / (float)(widthPoints-1), (float)j / (float)(heightPoints-1));
                    uvs[i + j * widthPoints] = uv;
                    triangles[triangleIndex++] = k;
                    triangles[triangleIndex++] = k + 1;
                    triangles[triangleIndex++] = k + heightPoints + 1;

                    triangles[triangleIndex++] = k + 1;
                    triangles[triangleIndex++] = k + heightPoints + 2;
                    triangles[triangleIndex++] = k + heightPoints + 1;
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
        Terrain currentTerrain = GetNearestTerrain(pos);
        float height = currentTerrain.SampleHeight(pos);
        height += currentTerrain.transform.position.y;
        //Debug.Log("height at " + pos + " = " + height);
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

