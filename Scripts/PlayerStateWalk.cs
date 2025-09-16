using Godot;
using System;

public partial class PlayerStateWalk : IPlayerState
{
    public IPlayerState DoState(Player _player, double _delta)
    {
        switch (_player.FacingDirection)
        {
            case Facing.Up: _player.anim.Play("Walk_Up"); break;
            case Facing.Down: _player.anim.Play("Walk_Down"); break;
            case Facing.Left: _player.anim.Play("Walk_Right"); break;
            case Facing.Right: _player.anim.Play("Walk_Right"); break;
        }

        if (_player.Velocity.IsZeroApprox())
        {
            return _player.idle;
        }

        else if (Input.IsActionJustPressed("attack"))
        {
            return _player.attack;
        }

        return _player.walk;
    }

    public void EnterState(Player _player)
    {
        GD.Print("Entered State: WALK.");
    }

    public void ExitState(Player _player)
    {
        GD.Print("Exited State: WALK.");
    }
}
