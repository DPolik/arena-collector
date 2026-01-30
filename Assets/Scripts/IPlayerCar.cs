using System;
using UnityEngine;

public interface IPlayerCar
{
    public void RegisterForTrafficCollisionEvent(Action<IPlayerCar, TrafficCarController> onTrafficCollision);
    public void RegisterForPickupCollisionEvent(Action<IPlayerCar, Pickup> onPickupCollision);
    public void Destroy();
    public void ChangeActivation(bool activate);
    public CarColor CarColor { get; set; }
    public SteeringAgent GetSteeringAgent();
}
