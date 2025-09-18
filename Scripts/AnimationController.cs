using Godot;
using System;

[Flags]
public enum Facing : byte
{
    Up = 1 << 0,
    Down = 1 << 1,
    Left = 1 << 2,
    Right = 1 << 3
}

public partial class AnimationController : Node
{
    [Export] public AnimatedSprite2D anim;
    [Export] public Facing facing = Facing.Up;

    public override void _Ready()
    {
        if (anim == null) anim = GetNode<AnimatedSprite2D>("Animator");
    }

    public override void _PhysicsProcess(double _delta)
    {
        if (facing == Facing.Left) anim.FlipH = true;
        else anim.FlipH = false;
    }

    public void UpdateAnimation(PlayerState state)
    {
        if (anim == null) return;

        string animName = state.ToString();

        switch (facing)
        {
            case Facing.Up:
                animName += "_Up";
                anim.FlipH = false;
                break;
            case Facing.Down:
                animName += "_Down";
                anim.FlipH = false;
                break;
            case Facing.Left:
                animName += "_Right";
                anim.FlipH = true;
                break;
            case Facing.Right:
                animName += "_Right";
                anim.FlipH = false;
                break;
        }

        if (anim.Animation != animName)
        {
            anim.Play(animName);
        }
    }
}