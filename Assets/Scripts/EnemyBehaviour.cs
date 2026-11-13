using Unity.VisualScripting;
using UnityEngine.AI;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    private NavMeshAgent mAgent;
    private Rigidbody mBody;
    
    private TrackSpline mCurrentTrack;
    private float mLerp;

    private void Update()
    {
        if (mAgent.IsUnityNull())
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                mAgent = gameObject.AddComponent<NavMeshAgent>();
            }
            else
            {
                Debug.LogError($"{name}: no NavMesh within 5 units of {transform.position}", this);
            }
        }

        if (mAgent.IsUnityNull()) return;

        if (!mAgent.SetDestination(new Vector3(-153.7088f, 18.58449f, 105.8145f)))
        {
            Debug.LogError($"{name}: NavMeshAgent.SetDestination failed", this);
        }
    }
}
