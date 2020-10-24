using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class WaterBoat : MonoBehaviour
{
    // properties
    public Transform motor;
    public float steerPower = 500f;
    public float motorPower = 5f;
    public float maxSpeed = 15f;
    public float drag = 0.1f;

    // components
    public ParticleSystem particles;

    private Rigidbody rbody;
    private Quaternion startRotation;

    void Awake()
    {
        rbody = GetComponent<Rigidbody>();
        startRotation = motor.localRotation;
    }


    // Update for physics events
    private void FixedUpdate()
    {

        int steer = 0;
        // steer direction [-1, 0, 1]
        if (Input.GetKey(KeyCode.A))
            steer = 1;
        if (Input.GetKey(KeyCode.D))
            steer = -1;


        // compute vectors
        Vector3 forward = Vector3.Scale(new Vector3(1, 0, 1), transform.forward);

        // forward power
        if (Input.GetKey(KeyCode.W))
            ApplyForceToReachVelocity(rbody, forward * maxSpeed, motorPower);
        // backward power
        if (Input.GetKey(KeyCode.S))
            ApplyForceToReachVelocity(rbody, forward * - maxSpeed, motorPower);
        // rotation force
        if (steer != 0 && Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
            rbody.AddForceAtPosition(steer * transform.right * steerPower / 100f, motor.position);


        // motor animation // particle system
        motor.SetPositionAndRotation(motor.position, transform.rotation * startRotation * Quaternion.Euler(0, 30f * steer, 0));
        if (particles != null)
        {
            if (Input.GetKey(KeyCode.W))
            {
                particles.Play();
            }
            else 
            {
                particles.Stop();
            }
        }

        //moving forward
        bool movingForward = Vector3.Cross(transform.forward, rbody.velocity).y < 0;

        //move in directions
        rbody.velocity = Quaternion.AngleAxis(Vector3.SignedAngle(rbody.velocity, (movingForward ? 1f : 0f) * transform.forward, Vector3.up) * drag, Vector3.up) * rbody.velocity;

    }

    public static void ApplyForceToReachVelocity(Rigidbody rigidbody, Vector3 velocity, float force = 1, ForceMode mode = ForceMode.Force)
    {
        if (force == 0 || velocity.magnitude == 0)
            return;

        velocity = velocity + velocity.normalized * 0.2f * rigidbody.drag;

        //force = 1 => need 1 s to reach velocity (if mass is 1) => force can be max 1 / Time.fixedDeltaTime
        force = Mathf.Clamp(force, -rigidbody.mass / Time.fixedDeltaTime, rigidbody.mass / Time.fixedDeltaTime);

        //dot product is a projection from rhs to lhs with a length of result / lhs.magnitude https://www.youtube.com/watch?v=h0NJK4mEIJU
        if (rigidbody.velocity.magnitude == 0)
        {
            rigidbody.AddForce(velocity * force, mode);
        }
        else
        {
            var velocityProjectedToTarget = (velocity.normalized * Vector3.Dot(velocity, rigidbody.velocity) / velocity.magnitude);
            rigidbody.AddForce((velocity - velocityProjectedToTarget) * force, mode);
        }
    }
}
