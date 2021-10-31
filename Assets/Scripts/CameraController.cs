using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    float zoomRate = -2.0f;
    float horizontalPanRate = -25.0f;
    float verticalPanRate = -25.0f;

    Vector3 pos; 

    // Update is called once per frame
    void Update()
    {
        pos = transform.position;

        // Mouse zoom
        pos.y += Input.mouseScrollDelta.y * zoomRate;

        // Arrow keys / WASD panning
        pos.x += Input.GetAxis("Horizontal") * Time.deltaTime * horizontalPanRate;
        pos.z += Input.GetAxis("Vertical") * Time.deltaTime * verticalPanRate;

        // Maximum and minimum values
        pos.y = Mathf.Min(pos.y, 120.0f);
        pos.y = Mathf.Max(pos.y, 10.0f);

        pos.x = Mathf.Min(pos.x, 40.0f);
        pos.x = Mathf.Max(pos.x, -40.0f);

        pos.z = Mathf.Min(pos.z, 35.0f);
        pos.z = Mathf.Max(pos.z, -35.0f);

        transform.position = pos;
    }
}
