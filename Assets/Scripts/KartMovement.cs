using System;
using UnityEngine;

public class KartMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Rigidbody mRigidbody;
    public float Speed;
    public float TurnStrength;
    public float RotSmoothing;
    
    public Transform ModelTransform;

    private void Start()
    {
        mRigidbody = GetComponent<Rigidbody>();
    }

    private float mRotationalVelocity;
    private float mCurrentModelRot;

    // Update is called once per frame
    private void Update()
    {
        float vInput = Input.GetAxis("Vertical");
        float hInput = Input.GetAxis("Horizontal");

        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 10f))
        {
            Vector3 dir = -Vector3.Cross(hit.normal, transform.right).normalized;
            Debug.DrawRay(transform.position, dir * 15f, Color.deepPink);
            mRigidbody.linearVelocity = dir * (Speed * vInput);

            mCurrentModelRot = Mathf.SmoothDamp
            (
                mCurrentModelRot,
                Vector3.Angle(hit.normal, transform.forward) * -Mathf.Sign(hit.normal.y) + 90f,
                ref mRotationalVelocity,
                0.1f
            );
            ModelTransform.localEulerAngles = new Vector3(mCurrentModelRot, 0f, 0f);
        }
        
        KartRotations(hInput, mRigidbody.linearVelocity.magnitude);
    }

    private void KartRotations(float input, float speed)
    {
        //rotates the car towards player input - clamp speed so cars turning speed is limited
        speed = Mathf.Clamp(speed, 0, 8);
        transform.Rotate(0, input * TurnStrength * speed, 0);

        //this is  for if the car is in midair or if it ends flipped up somehow - it rotates it facing straight up;
        if (!Physics.Raycast(transform.position,-transform.up,2))
        {
            Quaternion rotationTarget = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            transform.rotation = Quaternion.RotateTowards(transform.rotation,rotationTarget,RotSmoothing);
        }
    }
}
