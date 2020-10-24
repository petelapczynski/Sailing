using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
[RequireComponent(typeof(WaterPatch))]
public class WaterFloat : MonoBehaviour
{
    // public properties
    public float airDrag = 1f;
    public float waterDrag = 10f;
    public bool affectDirection = true;
    public bool attachToSurface = false;

    private List<Transform> floatPoints;

    // used components
    protected Rigidbody rbody;
    //protected Waves waves;
    //protected WaterController waterController;
    protected WaterPatch waterPatch;

    // water line 
    protected float waterLine;
    protected Vector3[] waterLinePoints;

    // help vectors
    protected Vector3 centerOffset;
    protected Vector3 smoothVectorRotation;
    protected Vector3 targetUp;

    public Vector3 center { get { return transform.position + centerOffset; } }


    // Awake runs before start
    void Awake()
    {
        //waves = FindObjectOfType<Waves>();
        //waterController = FindObjectOfType<WaterController>();
        
        rbody = GetComponent<Rigidbody>();
        rbody.useGravity = false;
        waterPatch = GetComponent<WaterPatch>();

        floatPoints = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Float Point")) 
                floatPoints.Add(child);
        }

        // compute center
        waterLinePoints = new Vector3[floatPoints.Count];
        for (int i = 0; i < floatPoints.Count; i++)
        {
            waterLinePoints[i] = floatPoints[i].position;
        }
        centerOffset = GetCenter(waterLinePoints) - transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    void Update()
    {
           
    }

    // Update for physics
    void FixedUpdate()
    {
        // Update water patch
        float time = Application.isPlaying ? Time.time : 0;
        waterPatch._Center = transform.position;
        if (waterPatch.SqrDrift > 0.5f || time - waterPatch.LastUpdateTime >= 0f)
        {
            waterPatch.UpdatePatch(time);
        }

        // default water surface
        float newWaterLine = 0f;
        bool pointUnderWater = false;

        // set waterLinePoints and waterLine
        for (int i = 0; i < floatPoints.Count; i++)
        {
            // height 
            waterLinePoints[i] = floatPoints[i].position;

            // Waves
            //waterLinePoints[i].y = waterController.GetHeight(floatPoints[i].position);
            waterLinePoints[i].y = waterPatch.GetWaterHeight(waterLinePoints[i].x, waterLinePoints[i].z);

            newWaterLine += waterLinePoints[i].y / floatPoints.Count;
            if (waterLinePoints[i].y > floatPoints[i].position.y)
                pointUnderWater = true;
        }

        float waterLineDelta = newWaterLine - waterLine;
        waterLine = newWaterLine;

        // compute up vector
        targetUp = GetNormal(waterLinePoints);

        // gravity
        Vector3 gravity = Physics.gravity;
        rbody.drag = airDrag;
        if (waterLine >= center.y)
        {
            // under water
            rbody.drag = waterDrag;
            if (attachToSurface)
            {
                //attach to water surface
                rbody.position = new Vector3(rbody.position.x, waterLine - centerOffset.y, rbody.position.z);
            }
            else {
                //go up
                //gravity = -Physics.gravity;
                gravity = affectDirection ? targetUp * -Physics.gravity.y : -Physics.gravity;
                transform.Translate(Vector3.up * waterLineDelta * 0.9f);
            }
        }

        rbody.AddForce(gravity * Mathf.Clamp(Mathf.Abs(waterLine - center.y), 0,1));



        // rotation
        if (pointUnderWater) {
            // attach to water surface
            targetUp = Vector3.SmoothDamp(transform.up, targetUp, ref smoothVectorRotation, 0.2f);
            rbody.rotation = Quaternion.FromToRotation(transform.up, targetUp) * rbody.rotation;
        }

    }

    public static Vector3 GetCenter(Vector3[] points)
    {
        Vector3 center = Vector3.zero;
        for (int i = 0; i < points.Length; i++)
        {
            center += points[i] / points.Length;
        }
        return center;
    }

    public static Vector3 GetNormal(Vector3[] points)
    {
        // https://www.ilikebigbits.com/2015_03_04_plane_from_points.html
        if (points.Length < 3)
            return Vector3.up;

        Vector3 center = GetCenter(points);
        
        float xx = 0f, xy = 0f, xz = 0f, yy = 0f, yz = 0f, zz = 0f;

        for (int i = 0; i < points.Length; i++) {
            Vector3 r = points[i] - center;
            xx += r.x * r.x;
            xy += r.x * r.y;
            xz += r.x * r.z;
            yy += r.y * r.y;
            yz += r.y * r.z;
            zz += r.z * r.z;
        }

        float det_x = yy * zz - yz * yz;
        float det_y = xx * zz - xz * xz;
        float det_z = xx * yy - xy * xy;

        if (det_x > det_y && det_x > det_z)
            return new Vector3(det_x, xz * yz - xy * zz, xy * yz - xz * yy).normalized;
        if (det_y > det_z)
            return new Vector3(xz * yz - xy * zz, det_y, xy * xz - yz * xx).normalized;
        else
            return new Vector3(xy * yz - xz * yy, xy * xz - yz * xx, det_z);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (floatPoints == null )
            return;

        for (int i = 0; i < floatPoints.Count; i++)
        {
            if (floatPoints[i] == null)
                continue;

            //if (waterController != null)
            //{
                // draw cube
            //    Gizmos.color = Color.red;
            //    Gizmos.DrawCube(waterLinePoints[i], Vector3.one * 0.3f);

            //}

            // draw sphere
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(floatPoints[i].position, 0.1f);

        }

        // draw center
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(new Vector3(center.x, waterLine, center.z), Vector3.one * 1f);

        }
    }
}
