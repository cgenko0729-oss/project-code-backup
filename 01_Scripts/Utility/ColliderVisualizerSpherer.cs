#if UNITY_EDITOR
using UnityEngine;

[ExecuteAlways]                          // draw in Edit Mode and Play Mode
[RequireComponent(typeof(SphereCollider))]

public class ColliderVisualizerSpherer : MonoBehaviour
{
    [Header("Draw Options")]
    public bool onlyWhenSelected = false;
    public bool drawFill = false;
    public Color wireColor = Color.cyan;
    public Color fillColor = new Color(0f, 1f, 1f, 0.08f);

    SphereCollider sphere;

    void OnEnable()    { sphere = GetComponent<SphereCollider>(); }
    void OnValidate()  { sphere = GetComponent<SphereCollider>(); }

    void OnDrawGizmos()
    {
        if (!onlyWhenSelected) Draw();
    }

    void OnDrawGizmosSelected()
    {
        if (onlyWhenSelected) Draw();
    }

    void Draw()
    {
        if (sphere == null) return;

        // Save previous gizmo state
        var prevColor  = Gizmos.color;
        var prevMatrix = Gizmos.matrix;

        // --- How Unity scales SphereColliders ---
        // Radius uses the largest component of lossyScale so it stays a true sphere.
        // Center (offset) is transformed with the object's full (possibly non-uniform) scale.
        // To visualize exactly:
        //   1) Compute world-space center using TransformPoint (applies non-uniform scale to offset).
        //   2) Build a TRS with that world center, the object's rotation, and UNIFORM scale = maxAxis.
        //   3) Draw a sphere of 'sphere.radius' at Vector3.zero in that TRS space.
        var t        = transform;
        var lossy    = t.lossyScale;
        float maxAxis = Mathf.Max(Mathf.Abs(lossy.x), Mathf.Abs(lossy.y), Mathf.Abs(lossy.z));

        // World-space center respecting non-uniform scale for the offset:
        Vector3 worldCenter = t.TransformPoint(sphere.center);

        // Set a matrix that keeps the sphere uniformly scaled by maxAxis at the correct world position
        Gizmos.matrix = Matrix4x4.TRS(worldCenter, t.rotation, Vector3.one * maxAxis);

        if (drawFill)
        {
            Gizmos.color = fillColor;
            Gizmos.DrawSphere(Vector3.zero, sphere.radius);
        }

        Gizmos.color = wireColor;
        Gizmos.DrawWireSphere(Vector3.zero, sphere.radius);

        // Restore gizmo state
        Gizmos.matrix = prevMatrix;
        Gizmos.color  = prevColor;
    }
}
#endif

