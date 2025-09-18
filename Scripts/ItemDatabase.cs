using Godot;
using System.Collections.Generic;

public partial class ItemDatabase : Node
{
    public static ItemDatabase instance { get; private set; }

    private const string ITEM_RESOURCES_PATH = "res://Items";

    private readonly Dictionary<ushort, ItemData> itemDatabase = new();

    public override void _Ready()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            QueueFree();
        }

        RegisterItems();
    }

    private void RegisterItems()
    {
        GD.Print("ItemDatabase: Loading all items...");
        ushort currentId = 0;

        using var dir = DirAccess.Open(ITEM_RESOURCES_PATH);

        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                if (!dir.CurrentIsDir() && fileName.EndsWith(".tres"))
                {
                    var item = GD.Load<ItemData>($"{ITEM_RESOURCES_PATH}/{fileName}");

                    if (item != null)
                    {
                        if (currentId == ushort.MaxValue)
                        {
                            GD.PrintErr("ItemDatabase: Ran out of item IDs! Max is 65,535.");
                            return;
                        }

                        itemDatabase.Add(currentId, item);

                        GD.Print($"Registered '{item.name}' with ID: {currentId}");
                        currentId++;
                    }
                }
                fileName = dir.GetNext();
            }
        }
        else
        {
            GD.PrintErr($"ItemDatabase: Failed to open directory at '{ITEM_RESOURCES_PATH}'.");
        }

        GD.Print($"ItemDatabase: Finished. Registered {itemDatabase.Count} items.");
    }

    public ItemData GetItemData(ushort _id)
    {
        itemDatabase.TryGetValue(_id, out var data);
        return data;
    }
}
