using Godot;
using System;
using System.Collections.Generic;

public partial class CharacterController : CharacterBody2D
{
    [Export] public MultiplayerSynchronizer synchronizer;
    [Export] public StateMachine stateMachine;
    [Export] public AnimationController animationController;
    [Export] public Camera2D camera;

    private Queue<byte> inputQueue = new Queue<byte>();

    public long ownerId;

    [Export] public float walkSpeed = 175;
    [Export] public float runSpeed = 300;

    public override void _Process(double _delta)
    {
        if (ownerId == 0) return;
        CollectInput();
    }

    public override void _PhysicsProcess(double _delta)
    {
        if (Multiplayer.IsServer()) return;

        if (Multiplayer.GetUniqueId() == ownerId)
        {
            if (inputQueue.Count > 0)
            {
                //GD.Print($"[Client {ownerId}] Processing {inputQueue.Count} inputs for prediction.");
                RpcId(1, nameof(ServerReceiveInputBatch), inputQueue.ToArray());

                foreach (byte packedInput in inputQueue)
                {
                    stateMachine.OnPhysicsProcess(_delta, packedInput);
                }

                inputQueue.Clear();
            }

            if (Velocity.Length() > 0)
            {
                //GD.Print($"[Peer {Multiplayer.GetUniqueId()}] Applying velocity {Velocity} to node {Name}");
            }

            MoveAndSlide();
        }
    }

    private void CollectInput()
    {
        // I decided this would be the most efficient way to send inputs to the server.
        // A queue of bytes wont miss any inputs each frame, as collecting in the physics process would.
        // This should result in about ~620 bytes of bandwith/sec over something like sending raw input each frame.
        // Sending raw input each frame or physics frame would be about ~2,280 bytes bandwith/sec.
        // If you need more than 8 inputs you can use a ushort for up to 16 inputs and so on etc.

        if (Multiplayer.GetUniqueId() != ownerId || Multiplayer.IsServer()) return;

        byte packedInputs = 0;
        if (Input.IsActionPressed("move_up")) packedInputs    |= (1 << 0);
        if (Input.IsActionPressed("move_down")) packedInputs  |= (1 << 1);
        if (Input.IsActionPressed("move_left")) packedInputs  |= (1 << 2);
        if (Input.IsActionPressed("move_right")) packedInputs |= (1 << 3);
        if (Input.IsActionPressed("sprint")) packedInputs     |= (1 << 4);
        if (Input.IsActionJustPressed("attack")) packedInputs |= (1 << 5);

        //GD.Print($"[Client {ownerId}] Collected input: {packedInputs}");
        inputQueue.Enqueue(packedInputs);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    private void ServerReceiveInputBatch(byte[] inputs)
    {
        if (Multiplayer.GetRemoteSenderId() != ownerId) return;

        //GD.Print($"[Server] Received {inputs.Length} inputs from {ownerId}.");
        foreach (byte packedInput in inputs)
        {
            stateMachine.OnPhysicsProcess(GetProcessDeltaTime(), packedInput);
        }

        MoveAndSlide();
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SetOwner(long _id)
    {
        ownerId = _id;

        //GD.Print($"On peer {Multiplayer.GetUniqueId()}, character {Name}'s owner was set to {ownerId}.");

        if (Multiplayer.GetUniqueId() == ownerId)
        {
            camera.Enabled = true;
        }
        else
        {
            camera.Enabled = false;
        }
    }
}