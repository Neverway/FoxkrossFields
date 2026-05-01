using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    public void SavePlayer(Vector3 position)
    {
        var data = new PlayerSaveData { posX = position.x, posY = position.y };
        File.WriteAllText(PlayerFile, JsonUtility.ToJson(data));
    }

    public Vector3? LoadPlayer()
    {
        if (!File.Exists(PlayerFile)) return null;
        var data = JsonUtility.FromJson<PlayerSaveData>(File.ReadAllText(PlayerFile));
        return new Vector3(data.posX, data.posY, 0);
    }

    public bool HasSave()
    {
        return File.Exists(PlayerFile);
    }

    private class PlayerSaveData
    {
        public float posX, posY;
    }
}
