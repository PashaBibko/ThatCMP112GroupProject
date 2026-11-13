using UnityEngine;

public class KartMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Rigidbody mRigidbody;
    public float Speed;
    public float TurnStrength;
    public float RotSmoothing;

    private void Start()
    {
        mRigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        float vInput = Input.GetAxis("Vertical");
        float hInput = Input.GetAxis("Horizontal");
        float currentSpeed = mRigidbody.linearVelocity.magnitude * 75;
        currentSpeed = Mathf.Clamp(currentSpeed,-300,2000);

        if(vInput < 0)
            currentSpeed *= -1;
        else
            currentSpeed = Mathf.Abs(currentSpeed);
        mRigidbody.AddForce(transform.forward * (vInput * (Speed + currentSpeed)));
        KartRotations(hInput, mRigidbody.linearVelocity.magnitude);
    }

    private void KartRotations(float input, float speed)
    {
        //rotates the car towards player input - clamp speed so cars turning speed is limited
        speed = Mathf.Clamp(speed, 0, 8);
        transform.Rotate(0, input * TurnStrength * speed, 0);

        //float xRot = transform.eulerAngles.x;
        //if (xRot > 180)
        //{
        //    xRot -= 360;
        //}
        ////limit the x rotation to stop car from flipping
        //xRot = Mathf.Clamp(xRot, -50f, 50f);

        //transform.rotation = Quaternion.Euler(xRot,transform.eulerAngles.y,0);
        //this is  for if the car is in midair or if it ends flipped up somehow - it rotates it facing straight up;
        Debug.Log("Rotation: " + transform.eulerAngles.x);


        if (!Physics.Raycast(transform.position,-transform.up, 2))
        {
            Quaternion rotationTarget = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            transform.rotation = Quaternion.RotateTowards(transform.rotation,rotationTarget,RotSmoothing);

            
        }

        RaycastHit hit;

        if (Physics.Raycast(transform.position, -transform.up, out hit, 5f))
        {
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                5 * Time.deltaTime
            );
        }

    }
}
