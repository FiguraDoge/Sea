using Ditzelgames;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WaterFloat : MonoBehaviour
{

    // public properties
    public float AirDrag = 1;
    public float WaterDrag = 10;
    public bool AffectDirection = true;
    public bool AttachToSurface = false;
    public Transform[] floatPoints;

    protected Rigidbody rb;
    protected Wave Wave;

    protected float waterLine;
    protected Vector3[] waterLinePoints;

    protected Vector3 centerOffset;
    protected Vector3 smoothVectorRotation;
    protected Vector3 targetUp;

    public Vector3 Center
    {
        get
        {
            return transform.position + centerOffset;
        }
    }

    void Awake()
    {
        Wave = FindObjectOfType<Wave>();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        // computer center
        waterLinePoints = new Vector3[floatPoints.Length];
        for (int i = 0; i < floatPoints.Length; i++)
            waterLinePoints[i] = floatPoints[i].position;
        centerOffset = PhysicsHelper.GetCenter(waterLinePoints) - transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        var newWaterLine = 0f;
        var pointUnderWater = false;

        // Set waterLinePoints and waterLine
        for (int i = 0; i < floatPoints.Length; i++)
        {
            // height
            waterLinePoints[i] = floatPoints[i].position;
            waterLinePoints[i].y = Wave.GetHeight(floatPoints[i].position);
            newWaterLine += waterLinePoints[i].y / floatPoints.Length;
            if (waterLinePoints[i].y > floatPoints[i].position.y)
                pointUnderWater = true;
        }

        var waterLineDelta = newWaterLine - waterLine;
        waterLine = newWaterLine;

        // By now, the floating points are floating on the water surface
        // The next step is to add gravity to the boat

        // Gravity
        var gravity = Physics.gravity;
        rb.drag = AirDrag;
        
        // If boat is under water
        if (waterLine > Center.y)
        {
            rb.drag = WaterDrag;
            // Under water
            if (AttachToSurface)
            {
                // Attach to water surface
                rb.position = new Vector3(rb.position.x, waterLine - centerOffset.y, rb.position.z);
            }
            else
            {
                // Go up
                gravity = AffectDirection ? targetUp * -Physics.gravity.y : -Physics.gravity;
                transform.Translate(Vector3.up * waterLineDelta * 0.9f);
            }
        }
        rb.AddForce(gravity * Mathf.Clamp(Mathf.Abs(waterLine - Center.y), 0, 1));

        // Computer up vector
        targetUp = PhysicsHelper.GetNormal(waterLinePoints);

        // Rotate if under water
        if (pointUnderWater)
        {
            // Attach to water surface
            targetUp = Vector3.SmoothDamp(transform.up, targetUp, ref smoothVectorRotation, 0.5f);
            rb.rotation = Quaternion.FromToRotation(transform.up, targetUp) * rb.rotation;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (floatPoints == null)
            return;

        for (int i = 0; i < floatPoints.Length; i++)
        {
            if (floatPoints[i] == null)
                continue;

            if (Wave != null)
            {
                // draw cube
                Gizmos.color = Color.red;
                Gizmos.DrawCube(waterLinePoints[i], Vector3.one * 0.3f);
            }

            // draw sphere
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(floatPoints[i].position, 0.1f);
        }

        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(new Vector3(Center.x, waterLine, Center.z), Vector3.one * 1f);
            Gizmos.DrawRay(new Vector3(Center.x, waterLine, Center.z), targetUp * 1f);
        }
    }
}
