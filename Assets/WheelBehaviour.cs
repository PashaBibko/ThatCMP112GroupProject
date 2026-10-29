using UnityEngine;

public class WheelBehaviour : MonoBehaviour
{

    public float RayCastDistance;
    public float RestLength = 0.5f;
    public float SpringStrength = 3000f;
    public float DamperStrength = 500f;


    private float PreviousLength;
    private Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        RaycastHit hit;

        float velocity = (Vector3.Dot(rb.GetPointVelocity(transform.position), transform.up));

        if(Physics.Raycast(transform.position,-transform.up,out hit,RayCastDistance))
        {
            float DistanceToGround = hit.distance;
            float Compression = RestLength - hit.distance;
            float SpringForce = (Compression * SpringStrength) - (velocity * DamperStrength);

            rb.AddForceAtPosition(transform.up * SpringForce, transform.position);


            
        }


    }
}
