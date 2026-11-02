using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

public class RaceGenerator : MonoBehaviour
{
    [SerializeField] private int RandomSeed = 23117;
    [SerializeField] private Material SharedMat;

    [Serializable]
    private struct PolarCoord
    {
        public float Angle;
        public float Distance;
    }

    private static Vector2? TryGetControlPoint(Vector2 start, Vector2 end, Vector2 dir)
    {
        Vector2 normal = end - start;
        Vector2 mid = (start + end) * 0.5f;
        
        float dot = Vector2.Dot(dir, normal);
        if (Mathf.Abs(dot) < Mathf.Epsilon) // Bisector is parallel
            return null;

        float t = Vector2.Dot(mid - start, normal) / dot;
        if (t is > 15f and < 50f) // Stops the splines from curving back, or being too long
            return start + dir * t;

        return null;
    }

    private static Vector2? TryGetControlPoint(Vector2 start, Vector2 startDir, Vector2 end, Vector2 endDir)
    {
        float cross = startDir.x * endDir.y - startDir.y * endDir.x;
        if (Mathf.Abs(cross) < 1e-4f) // Tangents are parallel
            return null;

        Vector2 d = end - start;
        float t = (d.x * endDir.y - d.y * endDir.x) / cross;
        float s = (startDir.x * d.y - startDir.y * d.x) / cross;
        if (t is > 15f and < 50f && s is > 15f and < 50f) // Stops the splines from curving back, or being too long
            return start + startDir * t;
        
        return null;
    }

    private void ClearPreviousTrackGameObjects()
    {
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
    }

    public void GenerateNewTrack()
    {
        const int TRACK_POINTS = 10;
        
        ClearPreviousTrackGameObjects();
        Random.InitState(RandomSeed);

        List<Spline> splines = new();
        
        bool createdLoopedTrack = false;
        int iterations = 0;

        while (!createdLoopedTrack && iterations < 1000)
        {
            splines.Clear();
            iterations++;
            
            Vector2 start = Vector2.zero;
            Vector2 dir = Vector2.up;

            for (int i = 0; i < TRACK_POINTS; i++)
            {
                Vector2 end = Vector2.zero;
                Vector2? control = null;

                while (!control.HasValue)
                {
                    end = start + Random.insideUnitCircle.normalized * 50f;
                    control = TryGetControlPoint(start, end, dir);
                }
                
                splines.Add(new Spline
                {
                    StartPoint = new Vector3(start.x, 0f, start.y),
                    ControlPoint = new Vector3(control.Value.x, 0f, control.Value.y),
                    EndPoint = new Vector3(end.x, 0f, end.y)
                });

                Vector3 dir3D = splines[^1].GetDirectionAtPoint(1f);
                dir = new Vector2(dir3D.x, dir3D.z);
                start = end;

                //
                if (i > TRACK_POINTS / 2f)
                {
                    Vector2 gate = new(0f, -50f);

                    Vector2? finalControlPoint = TryGetControlPoint(start, dir.normalized, gate, Vector2.up);
                    if (!finalControlPoint.HasValue)
                        continue;

                    Spline endSplineA = new()
                    {
                        StartPoint = new Vector3(start.x, 0f, start.y),
                        ControlPoint = new Vector3(finalControlPoint.Value.x, 0f, finalControlPoint.Value.y),
                        EndPoint = new Vector3(gate.x, 0f, gate.y)
                    };

                    Spline endSplineB = new()
                    {
                        StartPoint = new Vector3(gate.x, 0f, gate.y),
                        ControlPoint = new Vector3(0f, 0f, -12.5f),
                        EndPoint = Vector3.zero
                    };

                    createdLoopedTrack = true;
                    splines.Add(endSplineA);
                    splines.Add(endSplineB);
                    break;
                }
            }
        }

        Debug.Log($"Generation took {iterations} iterations");
        if (createdLoopedTrack)
        {
            foreach (Spline spline in splines)
            {
                GameObject track = new("Track");
                track.transform.SetParent(transform);

                TrackSpline trackSpline = track.AddComponent<TrackSpline>();
                trackSpline.Spline = spline;
                trackSpline.GenerateMesh(SharedMat);
            }
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
