using Godot;
using System;

public partial class PlayerStateIdle : IPlayerState
{
    public IPlayerState DoState(Player _player, double _delta)
    {
        switch (_player.facing)
        {
            case Facing.Up: _player.anim.Play("Idle_Up"); break;
            case Facing.Down: _player.anim.Play("Idle_Down"); break;
            case Facing.Left: _player.anim.Play("Idle_Right"); break;
            case Facing.Right: _player.anim.Play("Idle_Right"); break;
        }

        if (!_player.Velocity.IsZeroApprox())
        {
            return _player.walk;
        }

        if (Input.IsActionJustPressed("interact") && _player.isInteracting)
        {
            return _player.interact;
        }

        if (Input.IsActionJustPressed("attack"))
        {
            return _player.attack;
        }

        return _player.idle;
    }

    public void EnterState(Player _player)
    {
        //GD.Print("Entered State: IDLE.");
    }

    public void ExitState(Player _player)
    {
        //GD.Print("Exited State: IDLE.");
    }
}
