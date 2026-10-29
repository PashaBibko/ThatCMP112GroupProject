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

        
        //foreach(GameObject gm in Wheels)
        //{
        //    gm.GetComponent<WheelCollider>().motorTorque = VInput;
        //}
        

        Vector3 MassCenter = new Vector3(rb.centerOfMass.x, rb.centerOfMass.y - TiltStrength, rb.centerOfMass.z);
        rb.AddForceAtPosition(transform.forward * VInput * Speed, MassCenter);
    }
}
