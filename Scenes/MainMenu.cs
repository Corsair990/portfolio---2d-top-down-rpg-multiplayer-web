using Godot;
using System;

public partial class MainMenu : Node2D
{
    NetworkManager networkManager;

    public override void _Ready()
    {
        networkManager = GetNode<NetworkManager>("/root/NetworkManager");
    }

    public void OnClickHostButton()
    {
        if (networkManager == null) return;

        networkManager.SetupServer();
    }

    public void OnClickJoinButton()
    { 
        if (networkManager ==null) return;

        networkManager.SetupClient();
    }
}
