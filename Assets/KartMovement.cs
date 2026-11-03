using System.Collections;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph;
using UnityEngine;

public class KartMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Rigidbody rb;
    public float Speed;
    public float TurnStrength;
    public float RotSmoothing;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float VInput = Input.GetAxis("Vertical");
        float HInput = Input.GetAxis("Horizontal");
        float Currentspeed = rb.linearVelocity.magnitude * 75;

        Currentspeed = Mathf.Clamp(Currentspeed,-300,1600);

        if(VInput < 0)
        {
            Currentspeed = Currentspeed * -1;
        }
        else
        {
            Currentspeed = Mathf.Abs(Currentspeed);
        }
        rb.AddForce(transform.forward * VInput * (Speed + Currentspeed));
        KartRotations(HInput, rb.linearVelocity.magnitude);
        

    }



    void KartRotations(float input, float speed)
    {

        //rotates the car towards player input - clamp speed so cars turining speed is limited
        speed = Mathf.Clamp(speed, 0, 8);
        transform.Rotate(0, input * TurnStrength * speed, 0);

        float xRot = transform.eulerAngles.x;
        if (xRot > 180)
        {
            xRot -= 360;
        }
        //limit the x rotation to stop car from flipping
        xRot = Mathf.Clamp(xRot, -50f, 50f);

        

        transform.rotation = Quaternion.Euler(xRot,transform.eulerAngles.y,0);
        //this is  for if the car is in midair or if it ends up flipped up somehow -- it rotates it facing straight up;
        if (!Physics.Raycast(transform.position,-transform.up,2))
        {
            Quaternion rotationTarget = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            transform.rotation = Quaternion.RotateTowards(transform.rotation,rotationTarget,RotSmoothing);
        }
        
    }

}
