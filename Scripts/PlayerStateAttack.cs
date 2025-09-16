using Godot;
using System;
using System.Collections;

public partial class PlayerStateAttack : IPlayerState
{
    public IPlayerState DoState(Player _player, double _delta)
    {
        switch (_player.FacingDirection)
        {
            case Facing.Up: _player.anim.Play("Attack_Sword_Up"); break;
            case Facing.Down: _player.anim.Play("Attack_Sword_Down"); break;
            case Facing.Left: _player.anim.Play("Attack_Sword_Right"); break;
            case Facing.Right: _player.anim.Play("Attack_Sword_Right"); break;
        }

        if (!_player.isAttacking)
        {
            if (!_player.Velocity.IsZeroApprox())
            {
                return _player.walk;
            }
            else
                return _player.idle;
        }

        return _player.attack;
    }

    public void EnterState(Player _player)
    {
        GD.Print("Entered State: ATTACK.");
    }

    public void ExitState(Player _player)
    {
        GD.Print("Exited State: ATTACK.");
    }
}
