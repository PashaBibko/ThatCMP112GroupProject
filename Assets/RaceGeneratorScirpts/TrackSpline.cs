using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

[ExecuteAlways]
public class TrackSpline : MonoBehaviour
{
    [field: SerializeField] public Vector3 StartPoint { get; internal set; }
    [field: SerializeField] public Vector3 ControlPoint { get; internal set; }
    [field: SerializeField] public Vector3 EndPoint { get; internal set; }

    private MeshFilter mFilter;

    private void Awake()
    {
        mFilter = gameObject.GetOrAddComponent<MeshFilter>();

        GenerateMesh();
    }

    internal void GenerateMesh()
    {
        const int RENDER_SEGMENTS = 25;
        const int TRACK_WIDTH = 5;

        List<Vector3> verts = new();
        List<int> tris = new();

        for (int i = 0; i < RENDER_SEGMENTS + 1; i++)
        {
            float t = (float)i / RENDER_SEGMENTS;

            Vector3 point = GetPoint(t);
            Vector3 dir = GetDirectionAtPoint(t);
            Vector3 right = Vector3.Cross(dir, Vector3.up).normalized;

            Vector3 lhs = point - right * TRACK_WIDTH;
            Vector3 rhs = point + right * TRACK_WIDTH;

            verts.Add(lhs);
            verts.Add(rhs);
        }

        for (int i = 0; i < RENDER_SEGMENTS; i++)
        {
            int bl = i * 2;
            int br = (i * 2) + 1;
            int tl = (i + 1) * 2;
            int tr = ((i + 1) * 2) + 1;

            tris.Add(bl);
            tris.Add(br);
            tris.Add(tl);

            tris.Add(tl);
            tris.Add(br);
            tris.Add(tr);
        }

        Mesh mesh = new()
        {
            vertices = verts.ToArray(),
            triangles = tris.ToArray(),
        };

        mFilter.mesh = mesh;
    }

    private void OnDrawGizmosSelected()
    {
        Color p = Gizmos.color;

        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawSphere(StartPoint, 2f);
        Gizmos.DrawSphere(EndPoint, 2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(StartPoint, ControlPoint);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(EndPoint, ControlPoint);
        Gizmos.color = Color.purple;
        Gizmos.DrawLine(EndPoint, EndPoint + GetDirectionAtPoint(1f) * 5f);
        
        Gizmos.color = p;
    }

    public Vector3 GetPoint(float t)
    {
        t = Mathf.Clamp01(t);
        Vector3 l0 = Vector3.Lerp(StartPoint, ControlPoint, t);
        Vector3 l1 = Vector3.Lerp(ControlPoint, EndPoint, t);
        return Vector3.Lerp(l0, l1, t);
    }

    public Vector3 GetDirectionAtPoint(float t)
    {
        const float DELTA = 0.01f;
        Vector3 p0 = GetPoint(t - DELTA);
        Vector3 p1 = GetPoint(t + DELTA);
        return (p1 - p0).normalized;
    }

    public float GetLength()
    {
        const int SEGMENTS = 50;

        float length = 0f;
        for (int i = 0; i < SEGMENTS - 1; i++)
        {
            Vector3 start = GetPoint((float)i / SEGMENTS);
            Vector3 end = GetPoint((float)(i + 1) / SEGMENTS);
            length += Vector3.Distance(start, end);
        }

        return length;
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(TrackSpline))]
public class TrackSplineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10f);
        if (GUILayout.Button("Regenerate Mesh"))
            (target as TrackSpline)!.GenerateMesh();
    }
}

#endif // UNITY_EDITOR
