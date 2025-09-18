using Godot;
using Godot.Collections;

public partial class Inventory : Node
{
    public Dictionary<uint, uint> inventory = new Dictionary<uint, uint>();

    public void RequestDropItem(ItemData _item)
    {
        ushort itemId = ItemRegistry.Instance.GetItemId(_item);
        Rpc(nameof(Server_DropItem), itemId, Multiplayer.GetUniqueId());
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Server_DropItem(ushort _itemId, long _clientId)
    {
        GD.Print($"Server {Multiplayer.GetUniqueId()}: Received request to drop item {_itemId}.");

        // Validate the request (does the player actually have this item?)
        // Remove the item from the server's version of the inventory.
        // Spawn the item in the world.
        // Tell target client that the inventory has changed (using another RPC).
    }
}