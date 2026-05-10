using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using RivenFramework;
using UnityEngine;

public class GI_SaveManager : MonoBehaviour
{
    private float sessionStartTime;
    
    [Header("Save Settings")] 
    public string saveSlot;

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
        sessionStartTime = Time.realtimeSinceStartup;
        //GameInstance.Get<GI_TileChunkManager>().OnSaveFileLoaded();
        Debug.Log($"[SaveManager] Loaded save slot '{slot}'");
    }
    
    public void UnloadSaveFile()
    {
        var player = FindObjectOfType<TDPawn_Player>();
        var inv = player.playerInventory;
        var cycle = GameInstance.Get<CycleManager>();
        cycle.SaveCycle(out float cycleTimer, out bool cycleIsNight);
        var (_, _, _, existingPlaytime) = LoadFileInfo(saveSlot);
        SavePlayer(player.transform.position, inv.items, inv.gold, cycleTimer, cycleIsNight, existingPlaytime);
        GameInstance.Get<GI_TileChunkManager>().OnSaveFileUnloaded();
        saveSlot = null;
        Debug.Log("[SaveManager] Save file unloaded");
    }

    public void DeleteSaveFile(string slot)
    {
        string playerFile = Path.Combine(GetSaveRoot(slot));
        File.Delete(playerFile);
    }
    
    public static (string displayName, string playtime, string lastLogin, float rawPlaytime) LoadFileInfo(string slot)
    {
        string playerFile = Path.Combine(GetSaveRoot(slot), "player.json");
        if (!File.Exists(playerFile))
            return ("", "", "", 0f);

        try
        {
            var data = JsonUtility.FromJson<PlayerSaveData>(File.ReadAllText(playerFile));

            int totalSecs = Mathf.FloorToInt(data.totalPlaytimeSeconds);
            string playtime = $"{totalSecs / 3600:D2}:{(totalSecs % 3600) / 60:D2}:{totalSecs % 60:D2}";

            string lastLogin = "";
            if (data.lastLoginTimestamp > 0)
            {
                var dt = new System.DateTime(data.lastLoginTimestamp, System.DateTimeKind.Utc).ToLocalTime();
                lastLogin = dt.ToString("MM/dd/yy");
            }

            return ($"File", $"{playtime}  [{lastLogin}]", lastLogin, data.totalPlaytimeSeconds);
        }
        catch
        {
            return ("", "", "", 0f);
        }
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
    public void SavePlayer(Vector3 position, List<ItemInstance> inventory, int gold = 0, float cycleTimer = 0f, bool cycleIsNight = false, float existingPlaytime = 0f)
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
        
        float sessionTime = Time.realtimeSinceStartup - sessionStartTime;
        var data = new PlayerSaveData
        {
            posX = position.x,
            posY = position.y,
            gold = gold,
            cycleTimer = cycleTimer,
            cycleIsNight = cycleIsNight,
            cycleTimestamp = System.DateTime.UtcNow.Ticks,
            lastLoginTimestamp = System.DateTime.UtcNow.Ticks,
            totalPlaytimeSeconds = existingPlaytime + sessionTime,
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
    
    public (float timer, bool isNight, long timestamp) LoadCycleState()
    {
        if (!File.Exists(PlayerFile)) return (0f, false, 0);
        var data = JsonUtility.FromJson<PlayerSaveData>(File.ReadAllText(PlayerFile));
        return (data.cycleTimer, data.cycleIsNight, data.cycleTimestamp);
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
        public float cycleTimer;
        public bool cycleIsNight;
        public long cycleTimestamp;
        public long lastLoginTimestamp;
        public float totalPlaytimeSeconds;
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