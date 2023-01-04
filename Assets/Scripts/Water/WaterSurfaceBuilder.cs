using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSurfaceBuilder : MonoBehaviour
{
    public GameObject surfacePrefab;
    public Vector2 origin;
    public float squareDimensions;
    public Vector2 numberOfSquares;

    void Awake()
    {
        if(surfacePrefab != null)
        {
            for(int i=0; i<numberOfSquares.x; i++)
            {
                for(int j=0; j<numberOfSquares.y; j++)
                {
                    Instantiate(surfacePrefab, new Vector3(origin.x + i*squareDimensions, 0, origin.y + j*squareDimensions), Quaternion.identity);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
