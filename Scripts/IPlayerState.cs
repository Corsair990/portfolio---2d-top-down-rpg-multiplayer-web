using Godot;
using System;

public interface IPlayerState
{
    // Runs when the state is first entered. Use for setup logic like starting an animation.
    public void EnterState(Player _player);

    // Runs every physics frame. Contains the core logic for the state and checks for transitions.
    // It should return the new state if a transition happens, otherwise return itself.
    public IPlayerState DoState(Player _player, double _delta);

    // Runs when the state is exited. Use for cleanup logic.
    public void ExitState(Player _player);
}