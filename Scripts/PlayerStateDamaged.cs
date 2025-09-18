using Godot;

public partial class PlayerStateDamaged : IPlayerState
{
    public void Enter(CharacterController _controller)
    {
        _controller.Velocity = Vector2.Zero;
        _controller.animationController.UpdateAnimation(PlayerState.Attack);
    }

    public void Process(CharacterController _controller, double _delta)
    {

    }

    public void PhysicsProcess(CharacterController _controller, double _delta, byte _packedInputs)
    {
        
    }

    public void Exit(CharacterController _controller)
    {

    }
}
