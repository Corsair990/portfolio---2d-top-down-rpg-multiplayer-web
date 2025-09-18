using Godot;

public partial class PlayerStateIdle : IPlayerState
{
    public void Enter(CharacterController _controller)
    {
        _controller.Velocity = Vector2.Zero;

        _controller.animationController.UpdateAnimation(PlayerState.Idle);
    }

    public void Process(CharacterController _controller, double _delta)
    { 
        
    }

    public void PhysicsProcess(CharacterController _controller, double _delta, byte _packedInput)
    {
        _controller.Velocity = Vector2.Zero;

        _controller.animationController.UpdateAnimation(PlayerState.Idle);

        byte moveInputs = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3);

        if ((_packedInput & moveInputs) != 0) // Quick movement check.
        {
            _controller.stateMachine.TransitionTo(PlayerState.Walk);
        }

        if ((_packedInput & (1 << 5)) != 0) // Attack check.
        {
            _controller.stateMachine.TransitionTo(PlayerState.Attack);
        }
    }

    public void Exit(CharacterController _controller)
    {

    }
}