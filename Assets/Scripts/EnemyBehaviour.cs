using System.Collections;
using Unity.VisualScripting;
using UnityEngine.AI;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    private NavMeshAgent mAgent;
    private Rigidbody mBody;
    
    private TrackSpline mCurrentTrack;
    private float mLerp;

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

        mAgent.speed = 25f;
        
        // Makes the enemy travel around the track
        while (true)
        {
            if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit))
                if (hit.collider.TryGetComponent(out TrackSpline spline))
                    mAgent.SetDestination(spline.Next.Next.Next.Next.Next.Spline.EndPoint);

            yield return new WaitForSeconds(1f + Random.value); // Adds random to stop CPU dying once every second
        }
        
        // ReSharper disable once IteratorNeverReturns
    }
}
