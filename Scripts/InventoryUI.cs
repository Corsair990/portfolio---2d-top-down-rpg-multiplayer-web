using Godot;

public partial class InventoryUI : Control
{
    [Export] public Inventory inventory;
    [Export] private GridContainer gridContainer;
    [Export] private PackedScene slotScene;

    public override void _Ready()
    {
        GetNode<ClientEvents>("/root/ClientEvents").PlayerSpawned += OnPlayerSpawned;
    }

    private void OnPlayerSpawned(CharacterController _controller)
    {
        inventory = _controller.GetNode<Inventory>("Inventory");

        if (inventory == null)
        {
            GD.Print($"Inventory is null.");
        }

        _controller.inventoryUI = this;

        GD.Print($"Inventory UI connected to {_controller.ownerId}.");

        SetupSlots();
    }

    private void OnInventorySlotUpdated(int _slotIndex, ItemData _item, int _quantity)
    {
        var slotUI = gridContainer.GetChild<InventorySlotUI>(_slotIndex);

        slotUI.Update(_item, _quantity);
    }

    public void SetupSlots()
    {
        inventory.SlotUpdated += OnInventorySlotUpdated;

        for (int i = 0; i < inventory.inventorySize; i++)
        {
            var slot = slotScene.Instantiate<InventorySlotUI>();
            gridContainer.AddChild(slot);
        }
    }

    public override void _ExitTree()
    {
        inventory.SlotUpdated -= OnInventorySlotUpdated;
    }
}