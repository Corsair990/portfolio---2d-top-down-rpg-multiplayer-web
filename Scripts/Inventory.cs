using Godot;
using Godot.Collections;

public partial class Inventory : Node
{
    [Signal] public delegate void SlotUpdatedEventHandler(int slotIndex, ItemData item, int quantity);

    [Export] public int inventorySize = 20;
    public InventorySlot[] slots;

    public override void _Ready()
    {
        slots = new InventorySlot[inventorySize];

        for (int i = 0; i < inventorySize; i++) 
        { 
            slots[i] = new InventorySlot();
        }
    }

    public void RequestAddItem(ItemData _item, int _qty)
    {
        if (_item == null || _qty <= 0)
        {
            GD.PrintErr($"[Inventory]: Cannot request to add item. Requested item is null or Qty is <= 0.");
            return;
        }

        Rpc(nameof(ServerAddItem), _item.itemID, _qty);
    }

    private void ServerAddItem(ushort _itemId, int _quantityToAdd)
    {
        long clientId = Multiplayer.GetRemoteSenderId();
        ItemData item = ItemDatabase.instance.GetItemData(_itemId);
        if (item == null || _quantityToAdd <= 0) return;

        // Fill Existing Stacks
        for (int i = 0; i < inventorySize; i++)
        {
            if (_quantityToAdd <= 0) break;

            if (slots[i].Item == item && slots[i].Quantity < item.maxStackSize)
            {
                int spaceLeft = item.maxStackSize - slots[i].Quantity;
                int amountToStack = Mathf.Min(spaceLeft, _quantityToAdd);

                slots[i].Quantity += amountToStack;
                _quantityToAdd -= amountToStack;

                RpcId(clientId, nameof(ClientUpdateSlot), i, _itemId, slots[i].Quantity);
            }
        }

        // Fill Empty Slots
        if (_quantityToAdd > 0)
        {
            for (int i = 0; i < inventorySize; i++)
            {
                if (_quantityToAdd <= 0) break;

                if (slots[i].Item == null)
                {
                    int amountToStack = Mathf.Min(item.maxStackSize, _quantityToAdd);

                    slots[i].Item = item;
                    slots[i].Quantity = amountToStack;
                    _quantityToAdd -= amountToStack;

                    RpcId(clientId, nameof(ClientUpdateSlot), i, _itemId, slots[i].Quantity);
                }
            }
        }

        // If quantityToAdd is still > 0 at the end, the inventory is full.
        // Destroy the items added and leave the rest in the world, or drop at clients position.
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ClientUpdateSlot(int _slotIndex, ushort _itemId, int _quantity)
    {
        if (_slotIndex < 0 || _slotIndex >= inventorySize) return;

        // A quantity of 0 means the slot is now empty
        if (_quantity <= 0)
        {
            slots[_slotIndex].Item = null;
            slots[_slotIndex].Quantity = 0;
        }
        else
        {
            ItemData itemData = ItemDatabase.instance.GetItemData(_itemId);
            if (itemData != null)
            {
                slots[_slotIndex].Item = itemData;
                slots[_slotIndex].Quantity = _quantity;
            }
        }

        EmitSignal(SignalName.SlotUpdated, _slotIndex, slots[_slotIndex].Item, slots[_slotIndex].Quantity);
        GD.Print($"[Client]: Slot {_slotIndex} updated. Item: {slots[_slotIndex].Item.name}, Qty: {_quantity}");
    }

    public void RequestDropItem(int _slotIndex, int _quantity)
    {
        if (_slotIndex < 0 || _slotIndex >= inventorySize || _quantity <= 0) return;

        Rpc(nameof(ServerDropItem), _slotIndex, _quantity);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ServerDropItem(int _slotIndex, int _requestedQty)
    {
        long clientId = Multiplayer.GetRemoteSenderId();

        if (_slotIndex < 0 || _slotIndex >= inventorySize) return;

        InventorySlot slot = slots[_slotIndex];
        if (slot.Item == null || slot.Quantity <= 0) return;

        int amountToDrop = Mathf.Min(slot.Quantity, _requestedQty);

        slot.Quantity -= amountToDrop;

        ushort itemId = slot.Item.itemID;
        int newQuantity = slot.Quantity;

        if (slot.Quantity <= 0)
        {
            slot.Item = null;
            itemId = 0; // ID of 0 to signify an empty slot
            newQuantity = 0;
        }

        GD.Print($"[Server]: Client [{clientId}] dropped [{amountToDrop}] of item {itemId} from slot {_slotIndex}.");
        
        // TODO: Spawn the dropped item into the game world here.


        // We reuse our existing RPC to tell the client the new state of the slot.
        RpcId(clientId, nameof(ClientUpdateSlot), _slotIndex, itemId, newQuantity);
    }
}