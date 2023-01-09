using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControl : MonoBehaviour
{
    public float movementSpeed;
    public float verticalSpeed;
    public float rotationSpeed;
    private Vector3 movement = new Vector3(0,0,0);
    private float verticalMovement = 0;
    private Vector2 lookChange = new Vector2(0,0);

    public float minimumTerrainClearance;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(movement.magnitude > 0)
        {
            Vector3 cameraForward = transform.forward;
            cameraForward.y = 0;
            cameraForward = cameraForward.normalized;
            Vector3 cameraRight = transform.right;
            cameraRight.y = 0;
            cameraRight = cameraRight.normalized;
            Vector3 worldSpaceMovement = movement.z * cameraForward + movement.x * cameraRight;
            transform.position += worldSpaceMovement * movementSpeed * Time.deltaTime;
        }
        
        if(lookChange.magnitude > 0)
        {
            transform.Rotate(new Vector3(0, lookChange.x, 0) * rotationSpeed * Time.deltaTime, Space.World);
        }

        if(verticalMovement != 0)
        {
            transform.position += new Vector3(0, verticalMovement * verticalSpeed * Time.deltaTime, 0);
        }

        float altitude = GetAltitude();
        if(altitude < minimumTerrainClearance)
        {
            transform.position += new Vector3(0, minimumTerrainClearance - altitude, 0);
        }

    }

    public void OnMove(InputValue input)
    {
        Vector2 rawInput = input.Get<Vector2>();
        movement = new Vector3(rawInput.x, 0, rawInput.y);
    }

    public void OnHeight(InputValue input)
    {
        verticalMovement = input.Get<float>();
    }

    public void OnLook(InputValue input)
    {
        lookChange = input.Get<Vector2>();
        lookChange.y = 0;
    }

    public float GetAltitude()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain != null)
        {
            float altitudeFromTerrain = transform.position.y - terrain.SampleHeight(transform.position) - terrain.transform.position.y;
            return Mathf.Min(altitudeFromTerrain, transform.position.y);
        }
        return 10000f; // arbitrary large value
    }
}
