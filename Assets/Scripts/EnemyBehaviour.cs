using System.Collections;
using Unity.VisualScripting;
using UnityEngine.AI;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    [SerializeField] private Transform KartModel;

    private Collider mCollider;
    
    private NavMeshAgent mAgent;
    private TrackSpline mCurrentTrack;
    private float mLerp;

    private Vector3 mLastPosition;

    private void Awake()
    {
        mCollider = GetComponent<Collider>();
        mCollider.enabled = false;
    }
    
    private IEnumerator Start()
    {
        // Waits until the nav mesh has been baked to create the agent
        while (mAgent.IsUnityNull())
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                mAgent = gameObject.AddComponent<NavMeshAgent>();
            }
            
            else
                yield return new WaitForFixedUpdate();
        }

        mAgent.speed = 30f;
        mAgent.autoBraking = false;
        mAgent.angularSpeed = 300f;
        
        // Makes the enemy travel around the track
        while (true)
        {
            // Stops multiple paths being queued at the same time
            while (mAgent.pathPending)
                yield return new WaitForFixedUpdate();
            
            Debug.Log("Calculating new target point");
            if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit))
            {
                mCollider.enabled = true;
                if (hit.collider.TryGetComponent(out TrackSpline spline))
                {
                    mAgent.SetDestination(spline.Next.Spline.EndPoint + Random.insideUnitSphere * 2.5f);
                    yield return new WaitForFixedUpdate(); // Stops possible infinite loops
                }
            }

            while (mAgent.remainingDistance > mAgent.stoppingDistance + 15f)
                yield return new WaitForFixedUpdate();
        }
        
        // ReSharper disable once IteratorNeverReturns
    }

    private void Update()
    {
        if (!mAgent.IsUnityNull())
            KartModel.LookAt(transform.position + mAgent.velocity * 10f);
    }
}
