using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

public class RaceGenerator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int RandomSeed = 23117;
    [SerializeField] private uint TrackPoints = 10;
    [SerializeField, Range(0f, 1f)] private float SlopeChance = 0.2f;
    [SerializeField] private float HeightRange = 100f;
    [SerializeField] private float TrackSectionLength = 50f;
    [SerializeField] private uint MaxGenerationIterations = 10_000;
    [SerializeField] private uint MaxGeneratedBuildings = 1000;
    
    [Header("References")]
    [SerializeField] private NavMeshSurface NavMesh;
    [SerializeField] private GameObject BuildingPrefab;
    [SerializeField] private Material SharedMat;

    private void Start() => GenerateNewTrack();

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
        if (Mathf.Abs(cross) < Mathf.Epsilon) // Tangents are parallel
            return null;

        Vector2 dir = end - start;
        float t = (dir.x * endDir.y - dir.y * endDir.x) / cross;
        float s = (startDir.x * dir.y - startDir.y * dir.x) / cross;
        if (t is > 15f and < 50f && s is > 15f and < 50f) // Stops the splines from curving back, or being too long
            return start + startDir * t;
        
        return null;
    }

    private void ClearPreviousTrackGameObjects()
    {
        // Collects all of the current children game objects
        List<GameObject> children = (from Transform child in transform select child.gameObject).ToList();

        // Calls the correct function depending on the current environment
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
        // Cleanly sets up the scene for the track
        ClearPreviousTrackGameObjects();
        Random.InitState(RandomSeed);

        List<Spline> splines = new();
        bool createdLoopedTrack = false;
        int iterations = 0;

        // Tries upto x amount of times to create a rounded track, stops to avoid infinite loops if there
        // is an issue in the track generation as it would cause the game to crash
        while (!createdLoopedTrack && iterations < MaxGenerationIterations)
        {
            splines.Clear();
            iterations++;
            
            // Current information about the track generation to use between different sections
            Vector2 start = Vector2.zero;
            Vector2 dir = Vector2.up;
            float height = 0f;

            // Generates UPTO x amount of track points, can generate a minimum of x/2 points
            // Not guaranteed to output a looped track
            for (int i = 0; i < TrackPoints; i++)
            {
                // Either generates a curve or slope
                if (Random.value > SlopeChance)
                {
                    Vector2 end = Vector2.zero;
                    Vector2? control = null;

                    // Tries different end points until it finds one that has a suitable curve
                    while (!control.HasValue)
                    {
                        end = start + Random.insideUnitCircle.normalized * TrackSectionLength;
                        control = TryGetControlPoint(start, end, dir);
                    }

                    // Generates the spline object and adds it to the collection
                    splines.Add(new Spline
                    {
                        StartPoint = new Vector3(start.x, height, start.y),
                        ControlPoint = new Vector3(control.Value.x, height, control.Value.y),
                        EndPoint = new Vector3(end.x, height, end.y)
                    });

                    // Stores persistent information about the track generation
                    Vector3 dir3D = splines[^1].GetDirectionAtPoint(1f);
                    dir = new Vector2(dir3D.x, dir3D.z);
                    start = end;

                    // After the track is halfway through the maximum length it tries to generate a smooth path back to the start
                    if (i > TrackPoints / 2f)
                    {
                        Vector2 gate = new(0f, -80f);
                        Vector2 slopeControl1 = new(0f, -65f);
                        Vector2 slopeMid = new(0f, -50f);
                        Vector2 slopeControl2 = new(0f, -35f);
                        Vector2 startGrid = new(0f, -20f);

                        Vector2? finalControlPoint = TryGetControlPoint(start, dir.normalized, gate, Vector2.up);
                        if (!finalControlPoint.HasValue)
                            continue;

                        // Curve to the gate
                        splines.Add(new Spline
                        {
                            StartPoint = new Vector3(start.x, height, start.y),
                            ControlPoint = new Vector3(finalControlPoint.Value.x, height, finalControlPoint.Value.y),
                            EndPoint = new Vector3(gate.x, height, gate.y)
                        });

                        // Gate to midway through slope
                        splines.Add(new Spline
                        {
                            StartPoint = new Vector3(gate.x, height, gate.y),
                            ControlPoint = new Vector3(slopeControl1.x, height, slopeControl1.y),
                            EndPoint = new Vector3(slopeMid.x, height / 2f, slopeMid.y)
                        });
                        
                        // Midway through slope to start grid
                        splines.Add(new Spline
                        {
                            StartPoint = new Vector3(slopeMid.x, height / 2f, slopeMid.y),
                            ControlPoint = new Vector3(slopeControl2.x, 0f, slopeControl2.y),
                            EndPoint = new Vector3(startGrid.x, 0f, startGrid.y)
                        });
                        
                        // Start grid to start
                        splines.Add(new Spline
                        {
                            StartPoint = new Vector3(startGrid.x, 0f, startGrid.y),
                            ControlPoint = Vector3.zero,
                            EndPoint = Vector3.zero
                        });

                        // The track is now a loop so we exit out of the loop to progress and create it in the world
                        createdLoopedTrack = true;
                        break;
                    }
                }
                
                else
                {
                    // New height for the track to be on
                    float newHeight = Random.value * HeightRange;
                    
                    // Slopes are split into 2 splines so we need two control points
                    Vector2 control1 = start + dir.normalized * 15f;
                    Vector2 midpoint = start + dir.normalized * 30f;
                    Vector2 control2 = start + dir.normalized * 45f;
                    Vector2 end = start + dir.normalized * 60f;

                    // Adds the splines to the collection
                    splines.Add(new Spline
                    {
                        StartPoint = new Vector3(start.x, height, start.y),
                        ControlPoint = new Vector3(control1.x, height, control1.y),
                        EndPoint = new Vector3(midpoint.x, (height + newHeight) / 2f, midpoint.y)
                    });
                    
                    splines.Add(new Spline
                    {
                        StartPoint = new Vector3(midpoint.x, (height + newHeight) / 2f, midpoint.y),
                        ControlPoint = new Vector3(control2.x, newHeight, control2.y),
                        EndPoint = new Vector3(end.x, newHeight, end.y)
                    });

                    // Stores persistent information
                    height = newHeight;
                    start = end;
                }
            }
        }

        // Early exit if the track does not generate properly
        if (!createdLoopedTrack)
        {
            Debug.LogError("Could not generate track, try a different seed or check generation code.");
            return;
        }

        // Trackers for the bounding box of all the spline points
        float minX = float.MaxValue;
        float minZ = float.MaxValue;
        float maxX = float.MinValue;
        float maxZ = float.MinValue;

        TrackSpline previousSpline = null;
        TrackSpline first = null;

        // Loops through each of the track segment splines and creates game objects for them
        foreach (Spline spline in splines)
        {
            minX = Mathf.Min(minX, spline.StartPoint.x, spline.ControlPoint.x, spline.EndPoint.x);
            maxX = Mathf.Max(maxX, spline.StartPoint.x, spline.ControlPoint.x, spline.EndPoint.x);

            minZ = Mathf.Min(minZ, spline.StartPoint.z, spline.ControlPoint.z, spline.EndPoint.z);
            maxZ = Mathf.Max(maxZ, spline.StartPoint.z, spline.ControlPoint.z, spline.EndPoint.z);

            GameObject track = new("Track");
            track.transform.SetParent(transform);

            TrackSpline trackSpline = track.AddComponent<TrackSpline>();
            first ??= trackSpline;

            // Generates the actual mesh and colliders after assigning the spline
            trackSpline.Spline = spline;
            trackSpline.GenerateMesh(SharedMat);

            // Makes the previous section know that the current one is after it (used for pathfinding)
            if (!previousSpline.IsUnityNull())
                previousSpline!.Next = trackSpline;
            previousSpline = trackSpline;
        }

        // Makes the last spline know about the first so they all loop together
        previousSpline!.Next = first;

        // Using rejection sampling tries to generate buildings X amount of times
        for (int i = 0; i < MaxGeneratedBuildings; i++)
        {
            // Within a slightly larger area th
            Vector3 pos = new
            (
                Random.Range(minX - 150f, maxX + 150f),
                Random.Range(-50f, HeightRange + 100f), // Height range is constant
                Random.Range(minZ - 150f, maxZ + 150f)
            );

            // Checks that the area is available before spawning
            Vector3 buildingSize = Vector3.one * 15f;
            if (!Physics.BoxCast(new Vector3(pos.x, pos.y + 25f, pos.z), buildingSize, Vector3.down))
            {
                GameObject go = Instantiate(BuildingPrefab, transform, true);
                go.transform.position = pos;
            }
        }

        // Builds the nav mesh so Enemy AI can pathfind on it
        NavMesh?.BuildNavMesh();

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
        
        if (GUILayout.Button("Get new random track"))
        {
            generator.NewRandomSeed();
            generator.GenerateNewTrack();
        }
    }
}

#endif // UNITY_EDITOR
