using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class TrafficCarController : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private int midPoints = 2;
    [SerializeField] private float lookAhead = 0.6f;
    [SerializeField] private float offset = 0.3f;

    [Header("Movement")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float steeringForce = 20f;
    [SerializeField] private float drag = 4f;
    [SerializeField] private float rotVelocityThreshold = 0.01f;
    [SerializeField] private float rotSpeed = 10f;

    [Header("Collision Penalty")]
    [SerializeField] private int collisionPenalty = -10000;
    
    private Rigidbody rb;
    private Vector3[] pathPoints;
    private float t;
    private bool startMoving = false;
    
    public int CollisionPenalty => collisionPenalty;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = drag;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
    }

    public void StartPath(Vector3 start, Vector3 end)
    {
        pathPoints = new Vector3[midPoints + 2];
        GeneratePath(start, end);
        rb.position = pathPoints[0];
        startMoving = true;
    }

    void FixedUpdate()
    {
        if (!startMoving || pathPoints == null || pathPoints.Length < 2)
            return;

        t += (speed / 5f) * Time.fixedDeltaTime;

        var distanceToEnd = Vector3.Distance(rb.position, pathPoints[^1]);

        if (distanceToEnd < 0.1f)
        {
            Destroy(gameObject);
            return;
        }
        
        var maxT = pathPoints.Length - 1f;
        var evalT = Mathf.Min(t + lookAhead, maxT);

        var targetPos = EvaluateSpline(evalT);
        var toTarget = targetPos - rb.position;
        toTarget.y = 0f;

        var desiredVelocity = toTarget.normalized * speed;
        var steering = desiredVelocity - rb.linearVelocity;

        rb.AddForce(Vector3.ClampMagnitude(steering, steeringForce), ForceMode.Acceleration);

        RotateTowardsVelocity();
    }
    
    private void GeneratePath(Vector3 startPos, Vector3 targetPos)
    {
        var flowDir = (targetPos - startPos).normalized;
        var sideDir = Vector3.Cross(Vector3.up, flowDir);
        var totalDistance = Vector3.Distance(startPos, targetPos);
        var step = totalDistance / (midPoints + 1);
        pathPoints[0] = startPos;

        for (var i = 1; i <= midPoints; i++)
        {
            var forwardDist = step * i;
            var sideOffset = Random.Range(-offset, offset);
            var point = startPos + flowDir * forwardDist + sideDir * sideOffset;
            point.y = 0f;
            pathPoints[i] = point;
        }

        pathPoints[^1] = targetPos;
    }

    private Vector3 EvaluateSpline(float t)
    {
        var count = pathPoints.Length;
        var i = Mathf.Clamp(Mathf.FloorToInt(t), 0, count - 2);
        var localT = t - i;

        var p0 = pathPoints[Mathf.Clamp(i - 1, 0, count - 1)];
        var p1 = pathPoints[i];
        var p2 = pathPoints[i + 1];
        var p3 = pathPoints[Mathf.Clamp(i + 2, 0, count - 1)];

        return CatmullRom(p0, p1, p2, p3, localT);
    }

    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        var t2 = t * t;
        var t3 = t2 * t;

        return 0.5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
    }
    
    private void RotateTowardsVelocity()
    {
        var vel = rb.linearVelocity;
        vel.y = 0f;

        if (vel.sqrMagnitude < rotVelocityThreshold)
            return;

        var rot = Quaternion.LookRotation(vel.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotSpeed * Time.fixedDeltaTime);
    }
    
    void OnDrawGizmos()
    {
        if (pathPoints == null || pathPoints.Length == 0)
            return;

        Gizmos.color = Color.yellow;

        var prev = pathPoints[0];
        for (var i = 0.1f; i < pathPoints.Length - 1; i += 0.1f)
        {
            var p = EvaluateSpline(i);
            Gizmos.DrawLine(prev, p);
            prev = p;
        }
    }
}
