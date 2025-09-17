using Godot;
using System;

// We'll sync these enums using bitmasking. We can cut down on network data and cover all our facing / states in a short.
[Flags]
public enum Facing : byte
{
    None = 0,
    Up = 1 << 0,
    Down = 1 << 1,
    Left = 1 << 2,
    Right = 1 << 3
}

[Flags]
public enum PlayerAnimState : byte
{
    None = 0,
    Idle = 1 << 0,
    Walk = 1 << 1,
    Interact = 1 << 2,
    Attack = 1 << 3,
    Damaged = 1 << 4
}

public partial class Player : CharacterBody2D
{
    [Export] public AnimatedSprite2D anim;
    [Export] public MultiplayerSynchronizer synchronizer;
    [Export] public Camera2D camera;

    [Export] public float walkSpeed = 175;
    [Export] public float runSpeed = 300;

    // The single networked property to sync both facing and anim state.
    private short playerState;
    private Vector2 serverPosition;

    [Export]
    public short PlayerState
    {
        get => playerState;
        set
        {
            if (playerState == value) return;
            playerState = value;

            if (!IsMultiplayerAuthority())
            {
                // On clients unpack the received state and update local variables.
                UnpackState(playerState);
                TransitionToState(StateFromEnum(currentAnimState));
            }
        }
    }

    public Facing facing { get; private set; }
    public PlayerAnimState currentAnimState { get; private set; }
    private IPlayerState currentState;

    public readonly PlayerStateIdle idle = new();
    public readonly PlayerStateWalk walk = new();
    public readonly PlayerStateInteract interact = new();
    public readonly PlayerStateAttack attack = new();
    public readonly PlayerStateDamaged damaged = new();

    public bool isInteracting = false;
    public bool isAttacking = false;

    public override void _EnterTree()
    {
        int id = int.Parse(Name);
        SetMultiplayerAuthority(id);
        camera.Enabled = IsMultiplayerAuthority();
        TransitionToState(idle);
    }

    public override void _PhysicsProcess(double _delta)
    {
        if (IsMultiplayerAuthority() && !Multiplayer.IsServer())
        {
            Vector2 moveInputs = Input.GetVector("move_left", "move_right", "move_up", "move_down");
            bool isSprinting = Input.IsActionPressed("sprint");
            bool isAttackingPressed = Input.IsActionJustPressed("attack");

            RpcId(1, nameof(ServerReceiveInput), moveInputs, isSprinting, isAttackingPressed);

            ProcessMovementAndState(_delta, moveInputs, isSprinting, isAttackingPressed);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    private void ServerReceiveInput(Vector2 _moveInputs, bool _isSprinting, bool _isAttackingPressed)
    {
        if (!Multiplayer.IsServer()) return;

        if (Multiplayer.GetRemoteSenderId() != GetMultiplayerAuthority()) return;

        ProcessMovementAndState(GetProcessDeltaTime(), _moveInputs, _isSprinting, _isAttackingPressed);
    }

    private void ProcessMovementAndState(double _delta, Vector2 _moveInputs, bool _isSprinting, bool _isAttackingPressed)
    {
        if (Multiplayer.IsServer() && _isAttackingPressed)
        {
            this.isAttacking = true;
        }

        SetFacing(_moveInputs);
        Move(_moveInputs, _isSprinting);
        HandleStates(_delta);

        if (Multiplayer.IsServer())
        {
            serverPosition = Position;
        }

        else if (IsMultiplayerAuthority())
        {
            float distance = Position.DistanceTo(serverPosition);
            
            if (distance > 0.1f)
            {
                Position = Position.Lerp(serverPosition, 0.5f);
            }
        }
    }

    private void Move(Vector2 _moveInputs, bool _isSprinting)
    {
        float moveSpeed = _isSprinting ? runSpeed : walkSpeed;
        Velocity = _moveInputs * moveSpeed;
        MoveAndSlide();
    }

    private void SetFacing(Vector2 _moveInputs)
    {
        if (_moveInputs.Y < 0) facing = Facing.Up;
        if (_moveInputs.Y > 0) facing = Facing.Down;
        if (_moveInputs.X < 0) facing = Facing.Left;
        if (_moveInputs.X > 0) facing = Facing.Right;

        UpdateServerState();
    }

    private void HandleStates(double _delta)
    {
        IPlayerState newState = currentState.DoState(this, _delta);

        if (newState != currentState)
        {
            TransitionToState(newState);
        }
    }

    public void TransitionToState(IPlayerState _newState)
    {
        if (currentState != null)
        {
            currentState.ExitState(this);
        }
        currentState = _newState;
        currentState.EnterState(this);

        currentAnimState = EnumFromState(_newState);

        UpdateFacingAnimation();

        UpdateServerState();
    }

    private PlayerAnimState EnumFromState(IPlayerState _state)
    {
        if (_state is PlayerStateWalk) return PlayerAnimState.Walk;
        if (_state is PlayerStateInteract) return PlayerAnimState.Interact;
        if (_state is PlayerStateAttack) return PlayerAnimState.Attack;
        if (_state is PlayerStateDamaged) return PlayerAnimState.Damaged;
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

    public short PackState(Facing _facing, PlayerAnimState _animState)
    {
        // Shift the facing flags into the high bits of the short
        short packedFacing = (short)((short)_facing << 8);

        // Combine the shifted facing with the animation state
        short packedState = (short)(packedFacing | (short)_animState);

        return packedState;
    }

    public void UnpackState(short _packedState)
    {
        // Unpack the animation state by masking the lower 8 bits
        currentAnimState = (PlayerAnimState)(_packedState & 0xFF);

        // Unpack the facing by shifting right and masking
        facing = (Facing)((_packedState >> 8) & 0xFF);
    }

    private void UpdateFacingAnimation()
    {
        if (anim == null) return;
        if (facing == Facing.Left) anim.FlipH = true;
        else anim.FlipH = false;
    }

    private void UpdateServerState()
    {
        if (Multiplayer.IsServer())
        {
            PlayerState = PackState(facing, currentAnimState);
        }
    }

    public void OnAnimationFinished()
    {
        if (!Multiplayer.IsServer()) return;

        if (isAttacking)
        {
            isAttacking = false;
        }
    }
}