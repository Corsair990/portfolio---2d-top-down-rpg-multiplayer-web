using Godot;
using System;

// We'll sync these enums, so giving them a base type is good practice.
public enum Facing : byte
{
    Up,
    Down,
    Left,
    Right
}

public enum PlayerAnimState : byte
{
    Idle,
    Walk,
    Interact,
    Attack,
    Damaged
}

public partial class Player : CharacterBody2D
{
    [Export] public AnimatedSprite2D anim;
    [Export] public MultiplayerSynchronizer synchronizer;
    [Export] public Camera2D camera;

    [Export] public float walkSpeed = 175;
    [Export] public float runSpeed = 300;

    // These properties will be synced from the server to clients.
    // The C# setter logic will update the local state on other clients.
    private Facing _facing = Facing.Down;
    [Export]
    public Facing FacingDirection
    {
        get => _facing;
        set
        {
            _facing = value;
            UpdateFacingAnimation();
        }
    }

    private PlayerAnimState _currentAnimState = PlayerAnimState.Idle;
    [Export]
    public PlayerAnimState CurrentAnimState
    {
        get => _currentAnimState;
        set
        {
            if (_currentAnimState == value) return;
            _currentAnimState = value;
            // If we are not the authority, we force our state machine to match the server's.
            if (!IsMultiplayerAuthority())
            {
                TransitionToState(StateFromEnum(value));
            }
        }
    }

    // Local variables, not networked.
    private IPlayerState _currentState;

    // State machine instances
    public readonly PlayerStateIdle idle = new();
    public readonly PlayerStateWalk walk = new();
    public readonly PlayerStateInteract interact = new();
    public readonly PlayerStateAttack attack = new();
    public readonly PlayerStateDamaged damaged = new();

    // These public fields are modified by the state machine.
    public bool isInteracting = false;
    public bool isAttacking = false;

    public override void _Ready()
    {
        TransitionToState(idle);

        if (IsMultiplayerAuthority()) 
        {
            camera.Enabled = true;
        }
    }

    public override void _PhysicsProcess(double _delta)
    {
        if (IsMultiplayerAuthority())
        {
            // --- CLIENT-SIDE LOGIC (FOR THE OWNER) ---
            // 1. Get local inputs.
            Vector2 moveInputs = Input.GetVector("move_left", "move_right", "move_up", "move_down");
            bool isSprinting = Input.IsActionPressed("sprint");
            bool isAttackingPressed = Input.IsActionJustPressed("attack");

            // 2. Send inputs to the server for authoritative processing.
            synchronizer.Rpc(nameof(Server_ReceiveInput), moveInputs, isSprinting, isAttackingPressed);

            // 3. Perform client-side prediction for responsiveness.
            ProcessMovementAndState(_delta, moveInputs, isSprinting, isAttackingPressed);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    private void Server_ReceiveInput(Vector2 _moveInputs, bool _isSprinting, bool _isAttackingPressed)
    {
        // --- SERVER-SIDE LOGIC ---
        // Security check: only the owner of this node can command it.
        if (synchronizer.Multiplayer.GetRemoteSenderId() != synchronizer.GetMultiplayerAuthority()) return;

        // The server runs the authoritative simulation using the client's inputs.
        ProcessMovementAndState(GetProcessDeltaTime(), _moveInputs, _isSprinting, _isAttackingPressed);
    }

    // This method contains the core simulation logic, run by both the client (for prediction)
    // and the server (for authority).
    private void ProcessMovementAndState(double _delta, Vector2 _moveInputs, bool _isSprinting, bool _isAttackingPressed)
    {
        // Set the authoritative attacking state only on the server
        if (Multiplayer.IsServer() && _isAttackingPressed)
        {
            this.isAttacking = true;
        }

        SetFacing(_moveInputs);
        Move(_moveInputs, _isSprinting);
        HandleStates(_delta);
    }

    private void Move(Vector2 _moveInputs, bool _isSprinting)
    {
        float moveSpeed = _isSprinting ? runSpeed : walkSpeed;
        Velocity = _moveInputs * moveSpeed;
        MoveAndSlide();
    }

    private void SetFacing(Vector2 _moveInputs)
    {
        if (_moveInputs.Y < 0) FacingDirection = Facing.Up;
        if (_moveInputs.Y > 0) FacingDirection = Facing.Down;
        if (_moveInputs.X < 0) FacingDirection = Facing.Left;
        if (_moveInputs.X > 0) FacingDirection = Facing.Right;
    }

    private void UpdateFacingAnimation()
    {
        if (anim == null) return;
        if (FacingDirection == Facing.Left) anim.FlipH = true;
        else anim.FlipH = false;
    }

    private void HandleStates(double _delta)
    {
        IPlayerState newState = _currentState.DoState(this, _delta);

        if (newState != _currentState)
        {
            TransitionToState(newState);
        }
    }

    public void OnAnimationFinished()
    {
        // Only the server can authoritatively end the attack state.
        if (!Multiplayer.IsServer()) return;

        if (isAttacking)
        {
            isAttacking = false;
        }
    }

    // Centralize state transitions to also update our synced enum property.
    public void TransitionToState(IPlayerState _newState)
    {
        if (_currentState != null)
        {
            _currentState.ExitState(this);
        }
        _currentState = _newState;
        _currentState.EnterState(this);

        // If we are the server, update the synced property so clients are notified.
        if (Multiplayer.IsServer())
        {
            CurrentAnimState = EnumFromState(_newState);
        }
    }

    // Helper methods to convert between state objects and our synced enum.
    private PlayerAnimState EnumFromState(IPlayerState state)
    {
        if (state is PlayerStateWalk) return PlayerAnimState.Walk;
        if (state is PlayerStateInteract) return PlayerAnimState.Interact;
        if (state is PlayerStateAttack) return PlayerAnimState.Attack;
        if (state is PlayerStateDamaged) return PlayerAnimState.Damaged;
        return PlayerAnimState.Idle;
    }

    private IPlayerState StateFromEnum(PlayerAnimState _animState)
    {
        switch (_animState)
        {
            case PlayerAnimState.Walk: return walk;
            case PlayerAnimState.Interact: return interact;
            case PlayerAnimState.Attack: return attack;
            case PlayerAnimState.Damaged: return damaged;
            default: return idle;
        }
    }
}