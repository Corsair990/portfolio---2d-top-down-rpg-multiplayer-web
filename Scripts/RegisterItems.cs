using Godot;
using System.Collections.Generic;

public partial class ItemRegistry : Node
{
    // The public instance that can be accessed from anywhere.
    public static ItemRegistry Instance { get; private set; }

    // Path to the folder containing item resource files.
    private const string ITEM_RESOURCES_PATH = "res://items";

    // Dictionaries for fast lookups in both directions.
    private readonly Dictionary<ushort, ItemData> _id_to_data = new();
    private readonly Dictionary<ItemData, ushort> _data_to_id = new();

    public override void _Ready()
    {
        Instance = this;

        RegisterItems();
    }

    private void RegisterItems()
    {
        GD.Print("ItemRegistry: Starting item registration...");
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
                    var resource = GD.Load<ItemData>($"{ITEM_RESOURCES_PATH}/{fileName}");
                    if (resource != null)
                    {
                        if (currentId == ushort.MaxValue)
                        {
                            GD.PrintErr("ItemRegistry: Ran out of item IDs! Max is 65,535.");
                            return;
                        }

                        _id_to_data.Add(currentId, resource);
                        _data_to_id.Add(resource, currentId);

                        GD.Print($"Registered '{resource.name}' with ID: {currentId}");
                        currentId++;
                    }
                }
                fileName = dir.GetNext();
            }
        }
        else
        {
            GD.PrintErr($"ItemRegistry: Failed to open directory at '{ITEM_RESOURCES_PATH}'.");
        }

        GD.Print($"ItemRegistry: Finished. Registered {_id_to_data.Count} items.");
    }

    public ItemData GetItemData(ushort id)
    {
        _id_to_data.TryGetValue(id, out var data);
        return data;
    }

    public ushort GetItemId(ItemData data)
    {
        _data_to_id.TryGetValue(data, out var id);
        return id;
    }
}