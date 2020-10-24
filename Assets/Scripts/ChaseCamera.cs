using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseCamera : MonoBehaviour
{
    public Camera cam;
    public Transform target;
    public float viewDistanceOffset = 6f;
    public float viewHeightOffset = 2f;
    public float camDistanceOffset = -12f;
    public float camHeightOffset = 6f;
    public float minCamHeight = 5f;
    public float smoothRotation = 10f;
    public float smoothMove = 1f;

    private Vector3 targetLookPosition;
    private Vector3 camVelocity;

    // Awake is called on startup
    void Awake()
    {
        if (cam == null)
            cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        // camera rotation
        Quaternion originalRotation = cam.transform.rotation;
        targetLookPosition = target.position + target.forward * viewDistanceOffset + target.up * viewHeightOffset;       
        cam.transform.LookAt(targetLookPosition);
        Quaternion newRotation = cam.transform.rotation; 
        cam.transform.rotation = originalRotation;
        cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, newRotation, smoothRotation * Time.deltaTime);

        // camera position
        Vector3 camPosition = target.position + target.forward * camDistanceOffset + target.up * camHeightOffset;
        if (camPosition.y < minCamHeight) {
            camPosition.y = minCamHeight;        
        }
        cam.transform.position = Vector3.Lerp(cam.transform.position, camPosition, smoothMove * Time.deltaTime);

    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            // Yellow Circle at Target
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(target.transform.position, 0.2f);
            // Red Circle at Adjusted Offset Target
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetLookPosition, 0.2f);
        }

    }
}
