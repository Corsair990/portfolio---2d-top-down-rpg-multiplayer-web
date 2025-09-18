public interface IPlayerState
{
    // Runs when the state is first entered. Use for setup logic.
    public void Enter(CharacterController _controller);

    // Runs as normal _Process.
    public void Process(CharacterController _controller, double _delta);

    // Runs as normal _PhysicsProcess.
    public void PhysicsProcess(CharacterController _controller, double _delta, byte _packedInputs);

    // Use for cleanup logic.
    public void Exit(CharacterController _controller);
}