using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR


public class RaceGenerator : MonoBehaviour
{
    [SerializeField] private List<Vector3> TrackPoints = new();
    [SerializeField] private int RandomSeed = 23117;
    [SerializeField] private Material SharedMat;

    [Serializable]
    private struct PolarCoord
    {
        public float Angle;
        public float Distance;
    }

    private Vector2? TryGetControlPoint(Vector2 start, Vector2 end, Vector2 dir)
    {
        Vector2 normal = end - start;
        Vector2 mid = (start + end) * 0.5f;
        
        float dot = Vector2.Dot(dir, normal);
        if (Mathf.Abs(dot) < Mathf.Epsilon) // Bisector is parallel
            return null;

        float t = Vector2.Dot(mid - start, normal) / dot;
        if (t < 0f) // Intersection is behind
            return null;

        return start + dir * t;
    }

    public void GenerateNewTrack()
    {
        Random.InitState(RandomSeed);
        
        const int TRACK_POINTS = 3;
        TrackPoints.Clear();

        List<GameObject> children = new();
        foreach (Transform child in transform)
        {
            children.Add(child.gameObject);
        }
        
        foreach (GameObject child in children)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(child);
                continue;
            }
#endif // UNITY_EDITOR
            Destroy(child);
        }

        List<Vector2> unsortedPoints = new();
        for (int i = 0; i < TRACK_POINTS; i++)
        {
            Vector2 point = Random.insideUnitCircle * 20f;
            unsortedPoints.Add(new Vector2(point.x, point.y));
        }

        List<PolarCoord> polarCoords = new();
        foreach (Vector2 point in unsortedPoints)
        {
            float angle = Mathf.Atan2(point.y, point.x);
            float distance = Mathf.Clamp(new Vector2(point.x, point.y).magnitude, 5f, 20f);

            polarCoords.Add(new PolarCoord
            {
                Angle = angle,
                Distance = distance,
            });
        }

        IOrderedEnumerable<PolarCoord> sortedPolar = polarCoords.OrderBy(coord => coord.Angle);
        
        foreach (PolarCoord coord in sortedPolar)
            TrackPoints.Add(new Vector3(
                Mathf.Cos(coord.Angle) * coord.Distance,
                0f,
                Mathf.Sin(coord.Angle) * coord.Distance)
            );
        
        //
        Vector3? lastEndDirection = null;
        for (int i = 0; i < TrackPoints.Count - 1; i++)
        {
            GameObject go = new("TrackSection-" + i);
            go.transform.SetParent(transform);
            
            go.GetOrAddComponent<MeshRenderer>().sharedMaterial = SharedMat;
            
            Vector3 start = TrackPoints[i];
            Vector3 end = TrackPoints[i + 1];

            TrackSpline spline = go.AddComponent<TrackSpline>();
            spline.StartPoint = start;
            spline.EndPoint = end;

            if (lastEndDirection == null)
            {
                spline.ControlPoint = (start + end) * 0.5f;
                Debug.Log("Used default ControlPoint");
            }
            else
            {
                Vector2 a = new(start.x, start.z);
                Vector2 b = new(end.x, end.z);
                
                Vector2 dir = new(lastEndDirection.Value.x, lastEndDirection.Value.z);
                Vector2 control = TryGetControlPoint(a, b, dir).Value;
                spline.ControlPoint = new Vector3(control.x, 0f, control.y);
            }

            lastEndDirection = spline.GetDirectionAtPoint(1f).normalized;
            spline.GenerateMesh();
        }

#if UNITY_EDITOR
        SceneView.RepaintAll(); // Force redraw gizmo
#endif // UNITY_EDITOR
    }

    public void NewRandomSeed()
    {
        RandomSeed = Random.Range(0, int.MaxValue);
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif // UNITY_EDITOR
    }

    private void OnDrawGizmosSelected()
    {
        Color prevColor =  Gizmos.color;
        Gizmos.color = Color.red;

        Vector3? previous = null;
        foreach (Vector3 point in TrackPoints)
        {
            if (previous != null)
                Gizmos.DrawLine((Vector3)previous, point);
            
            Gizmos.DrawSphere(point, 2f);
            previous = point;
            
            Gizmos.color = new Color(1f, 0f, 1f, 0.5f);
        }

        Gizmos.color = prevColor; 
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(RaceGenerator))]
public class RaceGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RaceGenerator generator = (target as RaceGenerator)!;
        DrawDefaultInspector();
        
        GUILayout.Space(10f);
        if (GUILayout.Button("Generate"))
            generator.GenerateNewTrack();
        if (GUILayout.Button("Get new random seed"))
            generator.NewRandomSeed();
    }
}

#endif // UNITY_EDITOR
