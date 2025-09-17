using Godot;

public partial class NetworkManager : Node
{
    private const int serverPort = 7777;
    [Export] private PackedScene levelScene;

    public override void _Ready()
    {
        Multiplayer.PeerConnected += OnPeerConnected;
    }

    public void SetupServer()
    {
        var peer = new ENetMultiplayerPeer();
        peer.CreateServer(serverPort);

        var multiplayer = new SceneMultiplayer();
        multiplayer.MultiplayerPeer = peer;
        GetTree().SetMultiplayer(multiplayer);

        GetTree().ChangeSceneToFile("res://Scenes/world.tscn");
    }

    public void SetupClient()
    {
        var peer = new ENetMultiplayerPeer();
        peer.CreateClient("127.0.0.1", serverPort);

        var multiplayer = new SceneMultiplayer();
        multiplayer.MultiplayerPeer = peer;
        GetTree().SetMultiplayer(multiplayer);

        GetTree().ChangeSceneToFile("res://Scenes/world.tscn");
    }

    private void OnPeerConnected(long _id)
    {
        if (_id == 1) return;
        RpcId(_id, nameof(LoadScene), levelScene.ResourcePath);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void LoadScene(string scenePath)
    {
        GetTree().ChangeSceneToFile(scenePath);
    }

    public override void _ExitTree()
    {
        Multiplayer.PeerConnected -= OnPeerConnected;
    }
}