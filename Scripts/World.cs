using Godot;

public partial class World : Node
{
    [Export] private Node _playerContainer;
    [Export] private PackedScene _playerScene;

    public override void _EnterTree()
    {
        Multiplayer.ConnectedToServer += OnConnectedToServer;
    }

    private void OnConnectedToServer()
    {
        RpcId(1, nameof(RequestSpawn), Multiplayer.GetUniqueId());
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RequestSpawn(long _id)
    {
        if (!Multiplayer.IsServer()) return;

        SpawnPlayer(_id);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SpawnPlayer(long _id)
    {
        if (_playerContainer.GetNodeOrNull(_id.ToString()) != null) return;

        GD.Print($"Peer {Multiplayer.GetUniqueId()} is creating player for ID: {_id}");

        var playerInstance = _playerScene.Instantiate<Player>();
        playerInstance.Name = _id.ToString();

        _playerContainer.AddChild(playerInstance);
    }

    public override void _ExitTree()
    {
        Multiplayer.ConnectedToServer -= OnConnectedToServer;
    }
}