using Godot;
using System;

[GlobalClass]
public partial class ItemData : Resource
{
    [Export] public ushort itemID;
    [Export] public string name;
    [Export] public Texture2D texture;
}