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
    
    private static string GetSaveRoot(string slot) => Path.Combine(Application.persistentDataPath, "saves", slot);

    private void Awake()
    {
        Directory.CreateDirectory(ChunkDir);
    }

    // ---------------------------------------------
    // SAVE SLOTS
    // ---------------------------------------------
    /// <summary>
    /// Creates a blank save directory for the given slot
    /// Does nothing if the slot already exists
    /// </summary>
    public static void CreateNewSaveFile(string slot)
    {
        string chunkDir = Path.Combine(GetSaveRoot(slot), "chunks");
        Directory.CreateDirectory(chunkDir);
        Debug.Log($"[SaveManager] Created save slot '{slot}' at {GetSaveRoot(slot)}");
    }

    /// <summary>
    /// Returns true if the given slot has a player save file
    /// </summary>
    public static bool HasSaveFile(string slot)
    {
        string playerFile = Path.Combine(GetSaveRoot(slot), "player.json");
        return File.Exists(playerFile);
    }

    /// <summary>
    /// Switches the active save slot to the given slot and recreates
    /// the chunk directory so subsequent reads/writes target it
    /// </summary>
    public void LoadSaveFile(string slot)
    {
        saveSlot = slot;
        Directory.CreateDirectory(ChunkDir);
        //GameInstance.Get<GI_TileChunkManager>().OnSaveFileLoaded();
        Debug.Log($"[SaveManager] Loaded save slot '{slot}'");
    }
    
    public void UnloadSaveFile()
    {
        var inv = FindObjectOfType<Pawn_ItemInventory>();
        SavePlayer(FindObjectOfType<TDPawn_Player>().transform.position, inv.items, inv.gold);
        GameInstance.Get<GI_TileChunkManager>().OnSaveFileUnloaded();
        saveSlot = null;
        Debug.Log("[SaveManager] Save file unloaded");
    }

    
    // ---------------------------------------------
    // CHUNK DATA
    // ---------------------------------------------
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

    private string ChunkPath(Vector2Int coord) => Path.Combine(ChunkDir, $"chunk_{coord.x}_{coord.y}.json");

    // ---------------------------------------------
    // PLAYER DATA
    // ---------------------------------------------
    public void SavePlayer(Vector3 position, List<ItemInstance> inventory, int gold = 0)
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

        var data = new PlayerSaveData
        {
            posX = position.x,
            posY = position.y,
            gold = gold,
            inventory = inventorySaveData
        };
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
    
    public int LoadPlayerGold()
    {
        if (!File.Exists(PlayerFile)) return 0;
        var data = JsonUtility.FromJson<PlayerSaveData>(File.ReadAllText(PlayerFile));
        return data.gold;
    }

    public bool HasPlayerSave() => File.Exists(PlayerFile);

    // ---------------------------------------------
    // DATA CLASSES
    // ---------------------------------------------
    [Serializable]
    private class PlayerSaveData
    {
        public float posX, posY;
        public int gold;
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