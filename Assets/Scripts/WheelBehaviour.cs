using UnityEngine;

public class WheelBehaviour : MonoBehaviour
{
    public float RayCastDistance;
    public float RestLength = 0.5f;
    public float SpringStrength;
    public float DamperStrength;
    private Rigidbody mRigidbody;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        mRigidbody = GetComponentInParent<Rigidbody>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        float velocity = Vector3.Dot(mRigidbody.GetPointVelocity(transform.position), transform.up);
        
        //Raycast down towards ground
        if(Physics.Raycast(transform.position,-transform.up,out RaycastHit hit,RayCastDistance))
        {
            //compression is the distance between how far the ray goes in the ground and where the ray likes to rest;
            float compression = RestLength - hit.distance;
            float springForce = (compression * SpringStrength) - velocity * DamperStrength;
            mRigidbody.AddForceAtPosition(transform.up * springForce, transform.position);

            //wheel grip

            //translate world velocity into local - xyz
            Vector3 localVelocity = transform.InverseTransformDirection(mRigidbody.linearVelocity);
            
            //make z velocity smaller - tire grip
            localVelocity.x *= 0.9f;
            mRigidbody.linearVelocity = transform.TransformDirection(localVelocity);

            Debug.DrawRay(transform.position, -transform.up, Color.red);
        }
    }
}
