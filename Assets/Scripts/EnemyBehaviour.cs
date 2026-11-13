using Unity.VisualScripting;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    private Rigidbody mBody;
    
    private TrackSpline mCurrentTrack;
    private float mLerp;

    private void Start()
    {
        mBody = GetComponent<Rigidbody>();
    }
}
