using System;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(SteeringAgent))]
public class OpponentController : MonoBehaviour, IPlayerCar
{
    [Header("Traffic Avoidance")]
    [SerializeField] private float avoidRadius = 2f;
    [SerializeField] private float avoidStrength = 1.5f;
    [SerializeField] private LayerMask trafficMask;
    
    private Action<IPlayerCar, TrafficCarController> onTrafficCollision;
    private Action<IPlayerCar, Pickup> onPickupCollision;
    private SteeringAgent agent;
    private Transform pickupTarget;
    private Vector3 brakePosition;
    
    public CarColor CarColor { get; set; }
    
    void Awake()
    {
        agent = GetComponent<SteeringAgent>();
    }

    private Vector3 GetTargetPosition()
    {
        var target = CalculateTrafficAvoidance();

        if (pickupTarget != null)
        {
            target += pickupTarget.position;
        }
        target.y = 0f;
        return target;
    }

    void Update()
    {
        if (pickupTarget == null) // pick a target only if we don't have one
        {
            if (brakePosition != Vector3.zero) // check if we need to initialize brake position
                brakePosition = transform.position + transform.forward * 0.1f;
            agent.SetTarget(brakePosition);
            PickPickup();
            return;
        }
        
        if (pickupTarget == null) return;
        
        brakePosition = Vector3.zero; // reset brake
        agent.SetTarget(GetTargetPosition());
    }

    private void PickPickup()
    {
        var pickups = GameObject.FindGameObjectsWithTag("Pickup");
        if (pickups.Length == 0)
            return;

        pickupTarget = pickups.OrderBy(p => Vector3.Distance(transform.position, p.transform.position)).First().transform;
    }

    Vector3 CalculateTrafficAvoidance()
    {
        var hits = Physics.OverlapSphere(transform.position, avoidRadius, trafficMask);

        if (hits.Length == 0)
        {
            return Vector3.zero;
        }

        var offset = Vector3.zero;

        foreach (var hit in hits)
        {
            var away = transform.position - hit.transform.position;
            away.y = 0f;

            var weight = 1f - (away.magnitude / avoidRadius);
            offset += away.normalized * weight;
        }

        return offset * avoidStrength;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, avoidRadius);

        if (pickupTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pickupTarget.position, 0.15f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pickup"))
        {
            var pickup = other.gameObject.GetComponentInParent<Pickup>();
            
            onPickupCollision?.Invoke(this, pickup);

            if (other.transform.position == pickupTarget.position)
            {
                agent.Stop();
            }
        }
        else if (other.CompareTag("Traffic"))
        {
            var trafficCar = other.gameObject.GetComponentInParent<TrafficCarController>();
            onTrafficCollision?.Invoke(this, trafficCar);
        }
    }

    public void RegisterForTrafficCollisionEvent(Action<IPlayerCar, TrafficCarController> onTrafficCollision)
    {
        this.onTrafficCollision += onTrafficCollision;
    }

    public void RegisterForPickupCollisionEvent(Action<IPlayerCar, Pickup> onPickupCollision)
    {
        this.onPickupCollision += onPickupCollision;
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    public void ChangeActivation(bool activate)
    {
        agent.enabled = activate;
    }
    
    public SteeringAgent GetSteeringAgent()
    {
        return agent;
    }
}
