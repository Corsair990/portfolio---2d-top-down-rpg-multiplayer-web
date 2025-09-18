using Godot;
using System;
using System.Collections.Generic;

public enum PlayerState
{
    Idle,
    Walk,
    Interact,
    Attack,
    Damaged
}

public partial class StateMachine : Node
{
    [Export] private CharacterController controller;

    private Dictionary<PlayerState, IPlayerState> states = new();
    public IPlayerState currentState { get; private set; }

    public override void _Ready()
    {
        if (controller == null) controller = GetParent<CharacterController>();

        states.Add(PlayerState.Idle, new PlayerStateIdle());
        states.Add(PlayerState.Walk, new PlayerStateWalk());
        states.Add(PlayerState.Interact, new PlayerStateInteract());
        states.Add(PlayerState.Attack, new PlayerStateAttack());
        states.Add(PlayerState.Damaged, new PlayerStateDamaged());

        currentState = states[PlayerState.Idle];
        currentState.Enter(controller);
    }

    public override void _Process(double _delta)
    {
        currentState.Process(controller, _delta);
    }

    public void OnPhysicsProcess(double _delta, byte _packedInputBytes)
    {
        currentState.PhysicsProcess(controller, _delta, _packedInputBytes);
    }

    public void TransitionTo(PlayerState newStateKey)
    {
        if (!states.ContainsKey(newStateKey) || states[newStateKey] == currentState)
        {
            return;
        }

        //GD.Print($"[Peer {controller.ownerId}] Transitioning from {currentState.GetType().Name} to {newStateKey}");

        currentState.Exit(controller);

        currentState = states[newStateKey];

        currentState.Enter(controller);
    }
}