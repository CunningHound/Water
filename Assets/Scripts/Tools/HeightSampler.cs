using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HeightSampler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(GetHeightAt(transform.position));
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(GetHeightAt(transform.position));
    }

    private float GetHeightAt(Vector3 pos)
    {
        Terrain currentTerrain = GetNearestTerrain(pos);
        float height = currentTerrain.SampleHeight(pos);
        height += currentTerrain.transform.position.y;
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
