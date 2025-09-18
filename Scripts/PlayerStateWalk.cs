using Godot;

public partial class PlayerStateWalk : IPlayerState
{
    public void Enter(CharacterController _controller)
    {
        _controller.animationController.UpdateAnimation(PlayerState.Walk);
    }

    public void Process(CharacterController _controller, double _delta) 
    { 
        
    }

    public void PhysicsProcess(CharacterController _controller, double _delta, byte _packedInput)
    {
        byte moveBits = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3);

        if ((_packedInput & moveBits) == 0) // Quick movement check.
        {
            _controller.stateMachine.TransitionTo(PlayerState.Idle);
        }

        if ((_packedInput & (1 << 5)) != 0) // Attack Input check.
        {
            _controller.stateMachine.TransitionTo(PlayerState.Attack);
        }

        // We are moving so unpack input.
        Vector2 moveInput = Vector2.Zero;
        if ((_packedInput & (1 << 0)) != 0) { moveInput.Y -= 1; _controller.animationController.facing = Facing.Up; }   // Up
        if ((_packedInput & (1 << 1)) != 0) { moveInput.Y += 1; _controller.animationController.facing = Facing.Down; } // Down
        if ((_packedInput & (1 << 2)) != 0) { moveInput.X -= 1; _controller.animationController.facing = Facing.Left; } // Left
        if ((_packedInput & (1 << 3)) != 0) { moveInput.X += 1; _controller.animationController.facing = Facing.Right; }// Right

        _controller.animationController.UpdateAnimation(PlayerState.Walk);

        bool isSprinting = (_packedInput & (1 << 4)) != 0; // Sprinting

        float speed = isSprinting ? _controller.runSpeed : _controller.walkSpeed;

        _controller.Velocity = moveInput.Normalized() * speed;
        //GD.Print($"[Peer {_controller.ownerId}] StateWalk set velocity to {_controller.Velocity}");
    }

    public void Exit(CharacterController _controller) 
    {
        
    }
}
