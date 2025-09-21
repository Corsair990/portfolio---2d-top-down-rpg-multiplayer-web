using Godot;
using System;
using System.Collections.Generic;

public partial class CharacterController : CharacterBody2D
{
    [Export] public MultiplayerSynchronizer synchronizer;
    [Export] public StateMachine stateMachine;
    [Export] public AnimationController animationController;
    [Export] public Camera2D camera;
    public Control inventoryUI;

    private Queue<byte> inputQueue = new Queue<byte>();

    public long ownerId;

    [Export] public Vector2 serverPosition;
    [Export] public Vector2 serverVelocity;
    [Export] public float walkSpeed = 175;
    [Export] public float runSpeed = 300;
    [Export] public float smoothingSpeed = 30f;

    bool inventoryActive = false;

    public override void _Process(double _delta)
    {
        if (ownerId == 0) return;

        CollectInput();


        // We are interpolating all client positions here as well as our own.
        // Later I will look into creating an Area of Interest Management and only interpolate clients we can see.
        // For now this is works fine. At scale, not so much.
        // We can interpolate here using the global position because the server only, runs the physics simulation.
        // Our clients are just visual representations. Later we can enable multithreading and run physics safetly on a seperate thread this way.

        if (serverPosition != Vector2.Zero)
        {
            GlobalPosition = GlobalPosition.Lerp(serverPosition, (float)_delta * smoothingSpeed);
        }
    }

    public override void _PhysicsProcess(double _delta)
    {
        // Here we are using simple snapshot interpolation. The server runs all physics.
        // Later we can use multi-threaded physics if we keep it this way.
        // Client side prediction and server reconciliation is overkill for a 2D RPG. A fast-paced FPS is another story.
        // The small delay of about 50ms keeps it simple and the player movement is smooth.

        if (Multiplayer.IsServer()) return;

        if (Multiplayer.GetUniqueId() == ownerId)
        {
            if (inputQueue.Count > 0)
            {
                //GD.Print($"[Client {ownerId}] Processing {inputQueue.Count} inputs for prediction.");
                RpcId(1, nameof(ServerReceiveInputBatch), inputQueue.ToArray());

                inputQueue.Clear();
            }
        }
    }

    private void CollectInput()
    {
        // I decided this would be the most efficient way to send inputs to the server.
        // A queue of bytes wont miss any inputs each frame, as collecting in the physics process would.
        // This should result in about 0.7 bytes of bandwidth/sec per client over something like sending raw input each frame.
        // Sending raw input each frame or physics frame would be about 2.2 bytes bandwidth/sec per client.
        // If you need more than 8 inputs you can use a ushort for up to 16 inputs etc.

        if (Multiplayer.GetUniqueId() != ownerId || Multiplayer.IsServer()) return;

        byte packedInputs = 0;
        if (Input.IsActionPressed("move_up")) packedInputs    |= (1 << 0);
        if (Input.IsActionPressed("move_down")) packedInputs  |= (1 << 1);
        if (Input.IsActionPressed("move_left")) packedInputs  |= (1 << 2);
        if (Input.IsActionPressed("move_right")) packedInputs |= (1 << 3);
        if (Input.IsActionPressed("sprint")) packedInputs     |= (1 << 4);
        if (Input.IsActionJustPressed("attack")) packedInputs |= (1 << 5);

        if (Input.IsActionJustPressed("toggle_inventory"))
        {
            if (inventoryUI != null)
            {
                inventoryActive = !inventoryActive;

                inventoryUI.Visible = inventoryActive;
            }
        }


        //GD.Print($"[Client {ownerId}] Collected input: {packedInputs}");
        //inputQueue.Enqueue(packedInputs);

        if (inputQueue.Count == 0 || inputQueue.Peek() != packedInputs)
        {
            if (inputQueue.Count > 0) inputQueue.Clear();
            inputQueue.Enqueue(packedInputs);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    private void ServerReceiveInputBatch(byte[] inputs)
    {
        if (Multiplayer.GetRemoteSenderId() != ownerId) return;

        //GD.Print($"[Server] Received {inputs.Length} inputs from {ownerId}.");

        foreach (byte packedInput in inputs)
        {
            var physicsDelta = GetPhysicsProcessDeltaTime();
            stateMachine.OnPhysicsProcess(physicsDelta, packedInput);
            MoveAndSlide();
        }

        serverPosition = GlobalPosition;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SetOwner(long _id)
    {
        ownerId = _id;

        //GD.Print($"On peer {Multiplayer.GetUniqueId()}, character {Name}'s owner was set to {ownerId}.");

        if (Multiplayer.GetUniqueId() == ownerId)
        {
            camera.Enabled = true;
            GetNode<ClientEvents>("/root/ClientEvents").EmitSignal(ClientEvents.SignalName.PlayerSpawned, this);
        }
        else
        {
            camera.Enabled = false;
        }
    }
}