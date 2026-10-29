using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR


public class RaceGenerator : MonoBehaviour
{
    [SerializeField] private List<Vector3> TrackPoints = new();
    [SerializeField] private int RandomSeed = 23117;

    [Serializable]
    private struct PolarCoord
    {
        public float Angle;
        public float Distance;
        public float Elevation;
    }

    public void GenerateNewTrack()
    {
        const int TRACK_POINTS = 5;
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

        List<Vector3> unsortedPoints = new();
        for (int i = 0; i < TRACK_POINTS; i++)
        {
            Vector2 point = Random.insideUnitCircle * 20f;
            unsortedPoints.Add(new Vector3(point.x, 0f, point.y));
        }

        List<PolarCoord> polarCoords = new();
        foreach (Vector3 point in unsortedPoints)
        {
            float angle = Mathf.Atan2(point.z, point.x);
            float elevation = point.y;
            float distance = Mathf.Clamp(new Vector2(point.x, point.z).magnitude, 5f, 20f);

            polarCoords.Add(new PolarCoord
            {
                Angle = angle,
                Distance = distance,
                Elevation = elevation
            });
        }

        IOrderedEnumerable<PolarCoord> sortedPolar = polarCoords.OrderBy(coord => coord.Angle);
        
        foreach (PolarCoord coord in sortedPolar)
            TrackPoints.Add(new Vector3(
                Mathf.Cos(coord.Angle) * coord.Distance,
                coord.Elevation,
                Mathf.Sin(coord.Angle) * coord.Distance)
            );
        
        //
        for (int i = 0; i < TrackPoints.Count - 1; i++)
        {
            GameObject go = new("TrackSection-" + i);
            go.transform.SetParent(transform);
            
            Vector3 start = TrackPoints[i];
            Vector3 end = TrackPoints[i + 1];

            TrackSpline spline = go.AddComponent<TrackSpline>();
            spline.StartPoint = start;
            spline.EndPoint = end;
            spline.ControlPoint = (start + end) / 2f; // temporary
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

    private void OnDrawGizmos()
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
