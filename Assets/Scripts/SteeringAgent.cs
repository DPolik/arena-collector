using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SteeringAgent : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 6f;
    public float maxForce = 25f;
    public float rotSpeed = 15f;

    [Header("Arrival")]
    public float slowRadius = 1.5f;
    public float stopDistance = 0.2f;

    private Rigidbody rb;
    private Vector3 desiredVelocity;
    private Vector3 steeringForce;
    private Vector3 target;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
    }
    
    public void SetTarget(Vector3 target)
    {
        this.target = target;
    }

    void FixedUpdate()
    {
        if (target == default)
            return;

        steeringForce = Arrive(target);

        if (steeringForce == Vector3.zero)
            return;
        
        steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);

        RotateTowardsVelocity();

        var dot = Vector3.Dot(transform.forward, desiredVelocity.normalized);
        var forceFactor = Mathf.Max(0, dot); // will be 1 when facing target, 0 when perpendicular or facing away
    
        rb.AddForce(steeringForce * forceFactor, ForceMode.Acceleration);
        ClampVelocity();
    }

    private Vector3 Arrive(Vector3 target)
    {
        var toTarget = target - rb.position;
        toTarget.y = 0f;

        var distance = toTarget.magnitude;

        if (distance < stopDistance)
        {
            this.target = default;
            rb.linearVelocity = Vector3.zero;
            desiredVelocity = Vector3.zero;
            return Vector3.zero;
        }

        var speedFactor = Mathf.Clamp01(distance / slowRadius);
        desiredVelocity = toTarget.normalized * (maxSpeed * speedFactor);

        return desiredVelocity - rb.linearVelocity;
    }

    private void ClampVelocity()
    {
        var vel = rb.linearVelocity;
        vel.y = 0f;

        if (vel.magnitude > maxSpeed)
            rb.linearVelocity = vel.normalized * maxSpeed;
    }

    private void RotateTowardsVelocity()
    {
        if (desiredVelocity.sqrMagnitude < 0.1f)
            return;

        var rot = Quaternion.LookRotation(desiredVelocity.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotSpeed * Time.fixedDeltaTime);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + desiredVelocity);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + steeringForce);
    }

    public void Stop()
    {
        target = default;
        rb.linearVelocity = Vector3.zero;
        desiredVelocity = Vector3.zero;
        steeringForce = Vector3.zero;
    }
}
