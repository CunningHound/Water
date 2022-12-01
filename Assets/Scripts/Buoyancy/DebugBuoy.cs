using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugBuoy : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 pos = transform.position;
        pos.y = WaveManager.GetInstance().GetWaterHeightAt(pos);
        transform.position = pos;
    }
}
