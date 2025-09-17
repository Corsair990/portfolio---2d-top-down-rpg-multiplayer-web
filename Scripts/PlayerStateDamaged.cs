using Godot;
using System;

public partial class PlayerStateDamaged : IPlayerState
{
    public IPlayerState DoState(Player _player, double _delta)
    {
        switch (_player.facing)
        {
            case Facing.Up: _player.anim.Play("Damaged_Up"); break;
            case Facing.Down: _player.anim.Play("Damaged_Down"); break;
            case Facing.Left: _player.anim.Play("Damaged_Right"); break;
            case Facing.Right: _player.anim.Play("Damaged_Right"); break;
        }

        if (_player.Velocity.IsZeroApprox())
        {
            return _player.idle;
        }
        else if (!_player.Velocity.IsZeroApprox())
        {
            return _player.walk;
        }
        else
            return _player.damaged;
    }

    public void EnterState(Player _player)
    {
        //GD.Print("Entered State: DAMAGED.");
    }

    public void ExitState(Player _player)
    {
        //GD.Print("Exited State: DAMAGED.");
    }
}
