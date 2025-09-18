using Godot;

public partial class InventorySlotUI : PanelContainer
{
    [Export] private TextureRect itemTexture;
    [Export] private Label quantityLabel;
    public void Update(ItemData _item, int _quantity)
    {
        if (_item == null || _quantity <= 0)
        {
            itemTexture.Visible = false;
            quantityLabel.Visible = false;
        }
        else
        {
            itemTexture.Texture = _item.icon;
            itemTexture.Visible = true;

            if (_quantity > 1)
            {
                quantityLabel.Text = _quantity.ToString();
                quantityLabel.Visible = true;
            }
            else
            {
                quantityLabel.Visible = false;
            }
        }
    }
}