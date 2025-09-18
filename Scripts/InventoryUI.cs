using Godot;

public partial class InventoryUI : Control
{
    [Export] private Inventory inventory;
    [Export] private GridContainer gridContainer;
    [Export] private PackedScene slotScene;

    public override void _Ready()
    {
        for (int i = 0; i < inventory.inventorySize; i++)
        {
            var slot = slotScene.Instantiate<InventorySlotUI>();
            gridContainer.AddChild(slot);
        }

        inventory.SlotUpdated += OnInventorySlotUpdated;
    }

    private void OnInventorySlotUpdated(int _slotIndex, ItemData _item, int _quantity)
    {
        var slotUI = gridContainer.GetChild<InventorySlotUI>(_slotIndex);

        slotUI.Update(_item, _quantity);
    }
}