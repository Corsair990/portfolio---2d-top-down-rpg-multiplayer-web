using Godot;
using System;

public partial class PlayerStateInteract : IPlayerState
{
    public IPlayerState DoState(Player _player, double _delta)
    {
        switch (_player.facing)
        {
            case Facing.Up: _player.anim.Play("Interact_Up"); break;
            case Facing.Down: _player.anim.Play("Interact_Down"); break;
            case Facing.Left: _player.anim.Play("Interact_Right"); break;
            case Facing.Right: _player.anim.Play("Interact_Right"); break;
        }

        return _player.interact;
    }

    public void EnterState(Player _player)
    {
        //GD.Print("Entered State: INTERACT.");
    }

    public void ExitState(Player _player)
    {
        //GD.Print("Exited State: INTERACT.");
    }
}
