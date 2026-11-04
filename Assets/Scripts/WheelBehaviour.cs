using UnityEngine;

public class WheelBehaviour : MonoBehaviour
{

    public float RayCastDistance;
    public float RestLength = 0.5f;
    public float SpringStrength;
    public float DamperStrength;
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
        //Raycast down towards ground
        if(Physics.Raycast(transform.position,-transform.up,out hit,RayCastDistance))
        {
            float DistanceToGround = hit.distance;

            //compression is the distance between how far the ray goes in the ground and where the ray likes to rest;
            float Compression = RestLength - hit.distance;


            float SpringForce = (Compression * SpringStrength) - (velocity * DamperStrength);

            
            rb.AddForceAtPosition(transform.up * SpringForce, transform.position);

            //wheel grip

            //translate world velocity into local - xyz
            Vector3 localvelocity = transform.InverseTransformDirection(rb.linearVelocity);
            //make z velocity smaller - tire grip
            localvelocity.x *= 0.9f;
            rb.linearVelocity = transform.TransformDirection(localvelocity);

            Debug.DrawRay(transform.position, -transform.up, Color.red);

        }


        

     
    }
}
