using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Opening")]
    public float openAngle = 90f;
    public float openSmoothTime = 0.35f;

    [Header("Hinge")]
    public bool hingeOnPositiveSide = false;

    private Transform hingePivot;
    private Quaternion initialPivotRotation;

    private bool isOpen = false;
    private float currentAngle = 0f;
    private float angularVelocity = 0f;

    void Start()
    {
        CreateHingePivot();
    }

    void CreateHingePivot()
    {
        List<Vector3> points = new List<Vector3>();

        MeshFilter[] meshes =
        GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter mesh in meshes)
        {
            if (mesh.sharedMesh == null)
                continue;

            Bounds bounds = mesh.sharedMesh.bounds;

            // Get 8 bounding box corners in world space
            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 localPoint =
                        bounds.center +
                        Vector3.Scale(
                        bounds.extents,
                        new Vector3(x, y, z)
                        );

                        points.Add(
                        mesh.transform.TransformPoint(localPoint)
                        );
                    }
                }
            }
        }

        if (points.Count == 0)
        {
            Debug.LogError(
            "Door has no mesh: " + gameObject.name
            );
            enabled = false;
            return;
        }

        // Calculate horizontal center
        Vector3 center = Vector3.zero;

        foreach (Vector3 point in points)
            center += point;

        center /= points.Count;

        // Find horizontal direction of door width
        float xx = 0f;
        float zz = 0f;
        float xz = 0f;

        foreach (Vector3 point in points)
        {
            float dx = point.x - center.x;
            float dz = point.z - center.z;

            xx += dx * dx;
            zz += dz * dz;
            xz += dx * dz;
        }

        float angle = 0.5f *
        Mathf.Atan2(2f * xz, xx - zz);

        Vector3 widthDirection = new Vector3(
        Mathf.Cos(angle),
        0f,
        Mathf.Sin(angle)
        );

        Vector3 depthDirection = new Vector3(
        -widthDirection.z,
        0f,
        widthDirection.x
        );

        float minWidth = float.MaxValue;
        float maxWidth = float.MinValue;

        float minDepth = float.MaxValue;
        float maxDepth = float.MinValue;

        foreach (Vector3 point in points)
        {
            float width =
            Vector3.Dot(point, widthDirection);

            float depth =
            Vector3.Dot(point, depthDirection);

            minWidth = Mathf.Min(minWidth, width);
            maxWidth = Mathf.Max(maxWidth, width);

            minDepth = Mathf.Min(minDepth, depth);
            maxDepth = Mathf.Max(maxDepth, depth);
        }

        float hingeWidth = hingeOnPositiveSide
        ? maxWidth
        : minWidth;

        float hingeDepth =
        (minDepth + maxDepth) * 0.5f;

        Vector3 hingePosition =
        widthDirection * hingeWidth +
        depthDirection * hingeDepth;

        hingePosition.y = center.y;

        // Create pivot automatically
        GameObject pivotObject = new GameObject(
        gameObject.name + "_AutoHinge"
        );

        Transform originalParent = transform.parent;

        hingePivot = pivotObject.transform;

        hingePivot.position = hingePosition;
        hingePivot.rotation = Quaternion.identity;

        hingePivot.SetParent(originalParent, true);

        initialPivotRotation = hingePivot.rotation;

        // Reparent without changing visible position
        transform.SetParent(hingePivot, true);
    }

    void Update()
    {
        if (hingePivot == null)
            return;

        float targetAngle = isOpen ? openAngle : 0f;

        currentAngle = Mathf.SmoothDampAngle(
        currentAngle,
        targetAngle,
        ref angularVelocity,
        openSmoothTime
        );

        hingePivot.rotation =
        Quaternion.AngleAxis(
        currentAngle,
        Vector3.up
        ) * initialPivotRotation;
    }

    public void Interact()
    {
        if (hingePivot == null)
            return;

        isOpen = !isOpen;
    }
}
