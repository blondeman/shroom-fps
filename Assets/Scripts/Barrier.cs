using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds an extruded MeshCollider from child GameObjects used as polygon points on the XZ plane.
/// Add child objects, position them in XZ, and the collider will extrude upward by 'height'.
/// Call RebuildCollider() or toggle the component to refresh after moving children.
/// </summary>
[RequireComponent(typeof(MeshCollider))]
[ExecuteAlways]
public class PolygonExtrudeCollider : MonoBehaviour
{
    [Header("Extrusion")]
    [Tooltip("How far upward (Y) the collider extends from this object's Y position.")]
    public float height = 3f;

    [Tooltip("If true, rebuilds the collider every frame in the editor (useful while placing points).")]
    public bool livePreview = true;

    private MeshCollider meshCollider;

    private void OnEnable()
    {
        RebuildCollider();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (livePreview)
            RebuildCollider();
#endif
    }

    /// <summary>
    /// Gathers child transforms as XZ polygon points and rebuilds the MeshCollider.
    /// </summary>
    public void RebuildCollider()
    {
        if (meshCollider == null)
            meshCollider = GetComponent<MeshCollider>();

        // Collect child positions in local XZ space
        List<Vector3> points = new List<Vector3>();
        foreach (Transform child in transform)
        {
            Vector3 local = transform.InverseTransformPoint(child.position);
            points.Add(new Vector3(local.x, 0f, local.z));
        }

        if (points.Count < 3)
        {
            meshCollider.sharedMesh = null;
            return;
        }

        meshCollider.sharedMesh = BuildExtrudedMesh(points);
    }

    private Mesh BuildExtrudedMesh(List<Vector3> points)
    {
        int n = points.Count;

        // Two rings: bottom (y=0) and top (y=height), in local space
        Vector3[] verts = new Vector3[n * 2];
        for (int i = 0; i < n; i++)
        {
            verts[i]     = new Vector3(points[i].x, 0f,     points[i].z);
            verts[i + n] = new Vector3(points[i].x, height, points[i].z);
        }

        List<int> tris = new List<int>();

        // --- Side walls (double-sided) ---
        for (int i = 0; i < n; i++)
        {
            int next = (i + 1) % n;
            int b0 = i,      b1 = next;
            int t0 = i + n,  t1 = next + n;

            tris.Add(b0); tris.Add(t0); tris.Add(b1);
            tris.Add(b1); tris.Add(t0); tris.Add(t1);

            tris.Add(b1); tris.Add(t0); tris.Add(b0);
            tris.Add(t1); tris.Add(t0); tris.Add(b1);
        }


        Mesh mesh = new Mesh();
        mesh.name = "PolygonExtrudedCollider";
        mesh.vertices = verts;
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private void OnDrawGizmos()
    {
        if (!livePreview) return;

        List<Vector3> worldPoints = new List<Vector3>();
        foreach (Transform child in transform)
            worldPoints.Add(child.position);

        if (worldPoints.Count < 2) return;

        Gizmos.color = new Color(0.2f, 0.9f, 0.4f, 0.9f);
        for (int i = 0; i < worldPoints.Count; i++)
        {
            Vector3 a = worldPoints[i];
            Vector3 b = worldPoints[(i + 1) % worldPoints.Count];

            Gizmos.DrawLine(a, b);                                          // bottom edge
            Gizmos.DrawLine(a + Vector3.up * height, b + Vector3.up * height); // top edge
            Gizmos.DrawLine(a, a + Vector3.up * height);                    // vertical pillar
        }

        Gizmos.color = Color.yellow;
        foreach (Vector3 p in worldPoints)
            Gizmos.DrawSphere(p, 0.05f);
    }
}