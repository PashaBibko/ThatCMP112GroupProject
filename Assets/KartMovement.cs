using Unity.VisualScripting;
using UnityEditor.ShaderGraph;
using UnityEngine;

public class KartMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Rigidbody rb;
    public WheelCollider FrontWheelL;
    public GameObject[] Wheels;
    public float Speed;
    public float TiltStrength;
    public float TurnStrength;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float VInput = Input.GetAxis("Vertical");
        float HInput = Input.GetAxis("Horizontal");
        float speed = rb.linearVelocity.magnitude;

        speed = Mathf.Clamp(speed, 0, 8);

        Vector3 MassCenter = new Vector3(rb.centerOfMass.x, rb.centerOfMass.y - TiltStrength, rb.centerOfMass.z);
        rb.AddForceAtPosition(transform.forward * VInput * Speed, MassCenter);

        KartRotations(HInput, speed);

        
    }



    void KartRotations(float input, float speed)
    {
        transform.Rotate(0, input * TurnStrength * speed, 0);

        float xRot = transform.eulerAngles.x;
        float zRot = transform.eulerAngles.z;
        if (xRot > 180)
        {
            xRot -= 360;
        }

        if (zRot > 180)
        {
            zRot -= 360;
        }

        xRot = Mathf.Clamp(xRot, -5f, 5f);
        
        transform.rotation = Quaternion.Euler(
            xRot,
            transform.eulerAngles.y,
            0 // right now z rot seems to "lock on occasion so its easier right now to just ignore it.
        );
    }
}
