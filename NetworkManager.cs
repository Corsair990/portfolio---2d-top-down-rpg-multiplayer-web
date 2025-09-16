using Godot;
using System;
using System.Net;

public partial class NetworkManager : Node
{
    const string SERVER_IP = "127.0.0.1";
    const int SERVER_PORT = 7777;

    [Export] private MultiplayerSpawner _spawner;
    [Export] private PackedScene _playerScene;


    public override void _Ready()
    {
        Multiplayer.PeerConnected += OnClientConnected;
        Multiplayer.PeerDisconnected += OnClientDisconnected;
        Multiplayer.ConnectedToServer += OnConnectedToServer;
        Multiplayer.ConnectionFailed += OnConnectionFailed;
    }

    public void SetupServer()
    {
        GD.Print("Starting server.");

        var peer = new ENetMultiplayerPeer();

        var error = peer.CreateServer(SERVER_PORT);
        
        if (error != Error.Ok)
        {
            GD.PrintErr($"Failed to create server: {error}");
            return;
        }
        Multiplayer.MultiplayerPeer = peer;
        GD.Print("Server created.");

        GetTree().ChangeSceneToFile("res://Scenes/world.tscn");
    }

    public void SetupClient()
    {
        if (OS.HasFeature("web"))
        {
            GD.Print("Running on web, using websockets...");

            var peer = new WebSocketMultiplayerPeer();

            peer.CreateClient($"{SERVER_IP}:{SERVER_PORT}");
        }

        else 
        {
            GD.Print("Running on Desktop/Mobile using Enet.");

            var peer = new ENetMultiplayerPeer();
            var error = peer.CreateClient(SERVER_IP, SERVER_PORT);
            
            if (error != Error.Ok)
            {
                GD.PrintErr($"Failed to create client: {error}");
                return;
            }
            
            Multiplayer.MultiplayerPeer = peer;
        }
    }

    private void OnClientConnected(long _id)
    {
        GD.Print($"Player connected! ID: {_id}");
        
        if (!Multiplayer.IsServer()) return;

        RpcId(_id, nameof(ClientLoadGameScene), "res://Scenes/world.tscn");

        Player playerInstance = _playerScene.Instantiate<Player>();

        playerInstance.Name = _id.ToString();

        _spawner.AddChild(playerInstance);

        playerInstance.SetMultiplayerAuthority((int)_id);
    }

    private void OnConnectionFailed()
    {
        GD.PrintErr("Connection failed.");
    }

    private void OnConnectedToServer()
    {
        GD.Print("Connected to server.");
    }

    private void OnClientDisconnected(long _id)
    {
        GD.Print($"Client {_id} disconnected to server.");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void ClientLoadGameScene(string scenePath)
    {
        GetTree().ChangeSceneToFile(scenePath);
    }

    private void SpawnPlayer(long _id)
    {

        if (!Multiplayer.IsServer()) return;

        Player playerInstance = _playerScene.Instantiate<Player>();


        playerInstance.Name = _id.ToString();


        _spawner.AddChild(playerInstance);


        playerInstance.SetMultiplayerAuthority((int)_id);
    }

}