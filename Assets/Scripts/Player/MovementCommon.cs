using System;
using UnityEngine;

interface IPlayerMovement
{
    public void Initialize(MovementComponents components);

    public void OnUpdate();

    public void OnFixedUpdate();
}

[Serializable]
public class MovementComponents
{
    public MoveInput Input;
    public Rigidbody Rigidbody;
    public Transform Orientation;
    public PlayerController PlayerController;
}

public enum MovementType
{
    Ground,
    Air,
}