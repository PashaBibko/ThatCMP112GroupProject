using UnityEngine;

public class SmokeParticleScript : MonoBehaviour
{

    public Rigidbody rb;
    public ParticleSystem SmokeParticles;
    public GameObject Kart;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float velocity = rb.linearVelocity.magnitude;
        Debug.Log(velocity);
        var em = SmokeParticles.emission;
        if (velocity < 1f)
        {
            em.enabled = false;
        }
        else
        {
            em.enabled = true;
            em.rateOverTime = velocity;
            
        }

        float Xrot = Kart.transform.rotation.eulerAngles.x;

        SmokeParticles.gameObject.transform.localRotation = Quaternion.Euler(Xrot, 0, 0);

    }
}
