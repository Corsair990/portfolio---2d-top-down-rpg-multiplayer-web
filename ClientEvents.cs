using Godot;
using System;

public partial class ClientEvents : Node
{
    [Signal] public delegate void PlayerSpawnedEventHandler(CharacterController player);
}
