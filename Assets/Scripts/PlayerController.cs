using System;
using UnityEngine;

[RequireComponent(typeof(SteeringAgent))]
public class PlayerController : MonoBehaviour, IPlayerCar
{
    [SerializeField] private LayerMask groundMask;
    
    private Action<IPlayerCar, Pickup> onPickupCollision;
    private Action<IPlayerCar, TrafficCarController> onTrafficCollision;
    private SteeringAgent agent;

    public CarColor CarColor { get; set; }

    void Awake()
    {
        agent = GetComponent<SteeringAgent>();
    }

    void Update()
    {
        if (!Input.GetMouseButtonUp(0))
            return;

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
        {
            agent.SetTarget(hit.point);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pickup"))
        {
            var pickup = other.gameObject.GetComponentInParent<Pickup>();
            onPickupCollision?.Invoke(this, pickup);
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