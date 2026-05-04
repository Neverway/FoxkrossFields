using System;
using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GI_TileChunkManager : MonoBehaviour
{
    [Header("Settings")] 
    public int chunkSize = 16;
    public int viewDistance = 3;
    public float updateInterval = 0.25f;

    private Dictionary<Vector2Int, ChunkData> loadedChunks = new();
    private Vector2Int lastPlayerChunk = new(int.MaxValue, 0);
    private GI_TileWorldGenerator generator;
    private GI_SaveManager saveManager;
    private GI_TileDataManager tileDataManager;
    private Transform playerTransform;
    private Dictionary<Vector2Int, GameObject> liveObjects = new();

    private Queue<Vector2Int> chunkLoadQueue = new();
    private bool isLoadingChunk = false;

    private void Start()
    {
        generator = GameInstance.Get<GI_TileWorldGenerator>();
        saveManager = GameInstance.Get<GI_SaveManager>();
        tileDataManager = GameInstance.Get<GI_TileDataManager>();

        var player = FindObjectOfType<TDPawn_Player>();
        if (player) playerTransform = player.transform;

        StartCoroutine(ChunkUpdateLoop());
    }

    private IEnumerator ChunkUpdateLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);
            if (!playerTransform)
            {
                var player = FindObjectOfType<TDPawn_Player>();
                if (player) playerTransform = player.transform;
                yield return null;
            }

            Vector2Int playerChunk = WorldToChunk(playerTransform.position);
            if (playerChunk == lastPlayerChunk) continue;
            lastPlayerChunk = playerChunk;
            
            UpdateChunks(playerChunk);
        }
    }

    private void UpdateChunks(Vector2Int center)
    {
        var needed = new HashSet<Vector2Int>();

        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int y = -viewDistance; y <= viewDistance; y++)
            {
                needed.Add(new Vector2Int(center.x + x, center.y + y));
            }
        }

        var toUnload = new List<Vector2Int>();
        foreach (var coord in loadedChunks.Keys)
        {
            if (!needed.Contains(coord)) toUnload.Add(coord);
        }

        foreach (var coord in toUnload)
        {
            UnloadChunk(coord);
        }

        foreach (var coord in needed)
        {
            if (!loadedChunks.ContainsKey(coord) && !chunkLoadQueue.Contains(coord)) chunkLoadQueue.Enqueue(coord);
        }

        if (!isLoadingChunk)
        {
            isLoadingChunk = true; 
            StartCoroutine(ProcessLoadQueue());
        }
    }

    private IEnumerator ProcessLoadQueue()
    {
        while (chunkLoadQueue.Count > 0)
        {
            LoadChunk(chunkLoadQueue.Dequeue());
            yield return null;
        }

        isLoadingChunk = false;
    }

    private void LoadChunk(Vector2Int coord)
    {
        ChunkData data = saveManager.LoadChunk(coord) ?? generator.GenerateChunk(coord, chunkSize);

        loadedChunks[coord] = data;
        ApplyChunkToTilemap(data);
    }

    private void UnloadChunk(Vector2Int coord)
    {
        if (!loadedChunks.TryGetValue(coord, out var data)) return;
        SaveLiveObjects(coord);
        if (data.isDirty) saveManager.SaveChunk(data);
        ClearChunkFromTilemap(data);
        DespawnTileObjects(coord);
        loadedChunks.Remove(coord);
    }

    private void ApplyChunkToTilemap(ChunkData data)
    {
        if (data?.layers == null) return;
        int area = data.size * data.size;
        var positions = new Vector3Int[area];
        var tiles = new TileBase[area];
        
        for (int layer = 0; layer < data.layers.Length; layer++)
        {
            var tilemap = tileDataManager.GetTilemapFromLayer(layer);
            if (tilemap == null) continue;

            int count = 0;
            for (int x = 0; x < data.size; x++)
            {
                for (int y = 0; y < data.size; y++)
                {
                    int worldX = data.chunkCoord.x * data.size + x;
                    int worldY = data.chunkCoord.y * data.size + y;
                    positions[count] = new Vector3Int(worldX, worldY, 0);
                    string tileID = data.GetTile(layer, x, y);
                    tiles[count] = string.IsNullOrEmpty(tileID) ? null : tileDataManager.GetTileBaseFromID(tileID);
                    count++;
                }
            }
            tilemap.SetTiles(positions, tiles);
        }
        SpawnTileObjects(data);
    }

    private void ClearChunkFromTilemap(ChunkData data)
    {
        if (data?.layers == null) return;
        int area = data.size * data.size;
        var positions = new Vector3Int[area];
        var nullTiles = new TileBase[area];
        
        for (int layer = 0; layer < data.layers.Length; layer++)
        {
            var tilemap = tileDataManager.GetTilemapFromLayer(layer);
            if (tilemap == null) continue;

            int count = 0;
            for (int x = 0; x < data.size; x++)
            {
                for (int y = 0; y < data.size; y++)
                {
                    int worldX = data.chunkCoord.x * data.size + x;
                    int worldY = data.chunkCoord.y * data.size + y;
                    positions[count++] = new Vector3Int(worldX, worldY, 0);
                }
            }
            tilemap.SetTiles(positions, nullTiles);
        }
    }

    public void MarkTileDirty(Vector3Int worldCell, int layer, string newTileId)
    {
        Vector2Int chunkCoord = WorldCellToChunk(worldCell);
        if (!loadedChunks.TryGetValue(chunkCoord, out var data)) return;

        int localX = worldCell.x - chunkCoord.x * chunkSize;
        int localY = worldCell.y - chunkCoord.y * chunkSize;
        data.SetTile(layer, localX, localY, newTileId);
        data.isDirty = true;
    }

    public Vector2Int WorldToChunk(Vector3 worldPos)
    {
        Vector3 cellSize = tileDataManager.tileGrid.cellSize;
        return new Vector2Int(Mathf.FloorToInt(worldPos.x / (chunkSize * cellSize.x)), Mathf.FloorToInt(worldPos.y / (chunkSize * cellSize.y)));
    }

    public Vector2Int WorldCellToChunk(Vector3Int cell)
    {
        return new Vector2Int(Mathf.FloorToInt((float)cell.x / chunkSize), Mathf.FloorToInt((float)cell.y / chunkSize));
    }

    private void OnApplicationQuit()
    {
        SaveAllDirty();
    }

    public void SaveAllDirty()
    {
        foreach (var data in loadedChunks.Values)
        {
            if (data.isDirty) saveManager.SaveChunk(data);
        }
    }

    public void RecalculateLeaves(Vector3Int removedTrunk)
    {
        var tileDataManager = GameInstance.Get<GI_TileDataManager>();
        var generator = GameInstance.Get<GI_TileWorldGenerator>();
        var trunkTilemap = tileDataManager.GetTilemapFromLayer((int)TileLayers.ObjectsSolid);
        var leafTilemap = tileDataManager.GetTilemapFromLayer((int)TileLayers.Overlay);
        if (trunkTilemap == null || leafTilemap == null) return;

        int worldX = removedTrunk.x;
        int worldY = removedTrunk.y;
        
        for (int lx = worldX - 2; lx <= worldX + 2; lx++)
        {
            for (int ly = worldY + 1; ly <= worldY + 3; ly++)
            {
                var leafCell = new Vector3Int(lx, ly, 0);
                bool shouldHaveLeaf = AnyLivingTrunkCoversLeaf(lx, ly, trunkTilemap);

                TileBase currentLeaf = leafTilemap.GetTile(leafCell);
                TileBase desiredLeaf = shouldHaveLeaf ? tileDataManager.GetTileBaseFromID(generator.leafTileID) : null;

                if (currentLeaf == desiredLeaf) continue;

                leafTilemap.SetTile(leafCell, desiredLeaf);
                MarkTileDirty(leafCell, (int)TileLayers.Overlay, shouldHaveLeaf ? generator.leafTileID : null);
            }
        }
    }
    
    private bool AnyLivingTrunkCoversLeaf(int worldX, int worldY, Tilemap trunkTilemap)
    {
        var generator = GameInstance.Get<GI_TileWorldGenerator>();
        TileBase trunkTile = GameInstance.Get<GI_TileDataManager>().GetTileBaseFromID(generator.treeTileID);

        for (int tx = worldX - 1; tx <= worldX + 1; tx++)
        {
            for (int ty = worldY - 3; ty <= worldY - 1; ty++)
            {
                var trunkCell = new Vector3Int(tx, ty, 0);
                var aboveCell = new Vector3Int(tx, ty + 1, 0);
                bool hasTrunk = trunkTilemap.GetTile(trunkCell) == trunkTile;
                bool hasAbove = trunkTilemap.GetTile(aboveCell) == trunkTile;
                if (hasTrunk && hasAbove) return true;
            }
        }
        return false;
    }

    private void SpawnTileObjects(ChunkData data)
    {
        foreach (var objData in data.tileObjects)
        {
            var worldCell = new Vector3Int(objData.worldX, objData.worldY, 0);
            SpawnTileObject(objData, worldCell);
        }
    }
    
    private void SpawnTileObject(TileObjectData objData, Vector3Int worldCell)
    {
        var key = new Vector2Int(worldCell.x, worldCell.y);
        if (liveObjects.ContainsKey(key)) return;

        var tileData = tileDataManager.GetTileDataFromID(objData.tileID);
        if (tileData.associatedPrefab == null) return;

        var worldPos = tileDataManager.tileGrid.CellToWorld(worldCell) + tileDataManager.tileGrid.cellSize * 0.5f;
        var tileObject = Instantiate(tileData.associatedPrefab, worldPos, Quaternion.identity);

        var receiver = tileObject.GetComponent<ITileObjectReceiver>();
        receiver?.ReceiveData(objData);

        liveObjects[key] = tileObject;
    }
    
    private void SaveLiveObjects(Vector2Int chunkCoord)
    {
        if (!loadedChunks.TryGetValue(chunkCoord, out var data)) return;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                int worldX = chunkCoord.x * chunkSize + x;
                int worldY = chunkCoord.y * chunkSize + y;
                var key = new Vector2Int(worldX, worldY);

                if (!liveObjects.TryGetValue(key, out var go)) continue;
                var provider = go.GetComponent<ITileObjectReceiver>();
                if (provider == null) continue;

                var savedData = provider.ProvideData();
                if (savedData != null) data.SetTileObject(savedData);
            }
        }
    }
    
    private void DespawnTileObjects(Vector2Int chunkCoord)
    {
        var toRemove = new List<Vector2Int>();
        foreach (var kvp in liveObjects)
        {
            var key = kvp.Key;
            int cx = Mathf.FloorToInt((float)key.x / chunkSize);
            int cy = Mathf.FloorToInt((float)key.y / chunkSize);
            if (cx == chunkCoord.x && cy == chunkCoord.y)
            {
                Destroy(kvp.Value);
                toRemove.Add(key);
            }
        }
        foreach (var key in toRemove) liveObjects.Remove(key);
    }

    public void TrySpawnTileObject(Vector3Int worldCell, string tileID)
    {
        var tileData = tileDataManager.GetTileDataFromID(tileID);
        if (tileData.associatedPrefab == null) return;

        var objData = new TileObjectData
        {
            tileID = tileID,
            worldX = worldCell.x,
            worldY = worldCell.y
        };

        var chunkCoord = WorldCellToChunk(worldCell);
        if (loadedChunks.TryGetValue(chunkCoord, out var chunk))
            chunk.SetTileObject(objData);

        SpawnTileObject(objData, worldCell);
    }

    public void TryDespawnTileObject(Vector3Int worldCell)
    {
        var key = new Vector2Int(worldCell.x, worldCell.y);
        if (liveObjects.TryGetValue(key, out var tileObject))
        {
            Destroy(tileObject);
            liveObjects.Remove(key);
        }

        var chunkCoord = WorldCellToChunk(worldCell);
        if (loadedChunks.TryGetValue(chunkCoord, out var chunk))
            chunk.RemoveTileObject(worldCell.x, worldCell.y);
    }

}
