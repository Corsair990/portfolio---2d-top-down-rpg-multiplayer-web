using Godot;

public partial class World : Node
{
    [Export] private Node playerContainer;
    [Export] private PackedScene playerScene;

    public override void _Ready()
    {
        if (Multiplayer.IsServer())
        {
            // The server listens for this signal directly. No client request is needed.
            Multiplayer.PeerConnected += OnPeerConnected;
            Multiplayer.PeerDisconnected += OnPeerDisconnected;
        }
    }

    private void OnPeerConnected(long _id)
    {
        SpawnPlayer(_id);
    }

    private void OnPeerDisconnected(long _id) 
    {
        
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RequestSpawn(long _id)
    {
        if (!Multiplayer.IsServer()) return;

        SpawnPlayer(_id);
    }

    private void SpawnPlayer(long _id)
    {
        GD.Print($"Server is spawning player for ID: {_id}");

        var playerInstance = playerScene.Instantiate<CharacterController>();
        playerInstance.Name = _id.ToString();

        playerContainer.AddChild(playerInstance);

        playerInstance.Rpc(nameof(CharacterController.SetOwner), _id);
    }

    public override void _ExitTree()
    {
        Multiplayer.PeerConnected -= OnPeerConnected;
        Multiplayer.PeerDisconnected -= OnPeerDisconnected;
    }
}