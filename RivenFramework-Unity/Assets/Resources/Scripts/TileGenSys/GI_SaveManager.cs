using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using RivenFramework;
using UnityEngine;

public class GI_SaveManager : MonoBehaviour
{
    [Header("Save Settings")] 
    public string saveSlot = "slot_0";

    private string SaveRoot => Path.Combine(Application.persistentDataPath, "saves", saveSlot);
    private string ChunkDir => Path.Combine(SaveRoot, "chunks");
    private string PlayerFile => Path.Combine(SaveRoot, "player.json");

    private void Awake()
    {
        Directory.CreateDirectory(ChunkDir);
    }

    public void SaveChunk(ChunkData data)
    {
        string path = ChunkPath(data.chunkCoord);
        File.WriteAllText(path, JsonUtility.ToJson(data));
        data.isDirty = false;
    }

    public ChunkData LoadChunk(Vector2Int coord)
    {
        string path = ChunkPath(coord);
        if (!File.Exists(path)) return null;

        try
        {
            var data = JsonUtility.FromJson<ChunkData>(File.ReadAllText(path));
            data.PostLoadValidate();
            data.isDirty = false;
            return data;
        }
        catch
        {
            Debug.LogWarning($"[SaveManager] Detected corrupted chunk at {coord}, regenerating...");
            return null;
        }
    }

    private string ChunkPath(Vector2Int coord)
    {
        return Path.Combine(ChunkDir, $"chunk_{coord.x}_{coord.y}.json");
    }

    public void SavePlayer(Vector3 position, List<ItemInstance> inventory)
    {
        var inventorySaveData = new List<InventoryItemSaveData>();
        foreach (var itemInstance in inventory)
        {
            if (itemInstance == null)
            {
                inventorySaveData.Add(null);
                continue;
            }

            var entry = new InventoryItemSaveData
            {
                id = itemInstance.id,
                stackSize = itemInstance.stackSize
            };
            
            if (GameInstance.Get<GI_ItemDatabase>().GetItem(itemInstance.id) == null)
            {
                entry.isGenerated = true;
                entry.displayName = itemInstance.displayName;
                entry.description = itemInstance.asset.description;
                entry.subType = itemInstance.asset.subType;
                entry.material = itemInstance.asset.material;
                entry.forms = new List<ItemForm>(itemInstance.asset.forms);
                entry.tags = new List<ItemTag>(itemInstance.asset.tags);
            }

            inventorySaveData.Add(entry);
        }
        var data = new PlayerSaveData { posX = position.x, posY = position.y, inventory = inventorySaveData };
        File.WriteAllText(PlayerFile, JsonUtility.ToJson(data));
    }

    public Vector3? LoadPlayerPosition()
    {
        if (!File.Exists(PlayerFile)) return null;
        var data = JsonUtility.FromJson<PlayerSaveData>(File.ReadAllText(PlayerFile));
        return new Vector3(data.posX, data.posY, 0);
    }
    
    public List<ItemInstance> LoadPlayerInventory(int inventorySize)
    {
        var result = new List<ItemInstance>(new ItemInstance[inventorySize]);
        if (!File.Exists(PlayerFile)) return result;

        var data = JsonUtility.FromJson<PlayerSaveData>(File.ReadAllText(PlayerFile));
        if (data.inventory == null) return result;

        var registry = GameInstance.Get<GI_ItemDatabase>();
        for (int i = 0; i < data.inventory.Count && i < inventorySize; i++)
        {
            var entry = data.inventory[i];
            if (entry == null) continue;
            var asset = registry.GetItem(entry.id);
            if (asset != null)
            {
                result[i] = new ItemInstance(asset, entry.stackSize);
            }
            else if (entry.isGenerated)
            {
                var generated = ScriptableObject.CreateInstance<ScriptableItem>();
                generated.id = entry.id;
                generated.displayName = entry.displayName;
                generated.description = entry.description;
                generated.subType = entry.subType;
                generated.material = entry.material;
                generated.forms = entry.forms ?? new List<ItemForm>();
                generated.tags = entry.tags ?? new List<ItemTag>();
                generated.maxStackSize = 1;
                generated.isDiscardable = true;
                generated.icon = registry.GetIcon(entry.material, entry.subType, entry.id);
                result[i] = new ItemInstance(generated, entry.stackSize);
            }
            else
            {
                Debug.LogWarning($"[SaveManager] Could not find item '{entry.id}' in registry - Make sure you added it to the registry you dummy");
            }
        }
        return result;
    }

    public bool HasSave()
    {
        return File.Exists(PlayerFile);
    }

    [Serializable]
    private class PlayerSaveData
    {
        public float posX, posY;
        public List<InventoryItemSaveData> inventory;
    }
    
    [Serializable]
    private class InventoryItemSaveData
    {
        public string id;
        public int stackSize;
        
        // Data for generated items
        public bool isGenerated;
        public string displayName;
        public string description;
        public string subType;
        public ItemMaterial material;
        public List<ItemForm> forms;
        public List<ItemTag> tags;
    }
}
