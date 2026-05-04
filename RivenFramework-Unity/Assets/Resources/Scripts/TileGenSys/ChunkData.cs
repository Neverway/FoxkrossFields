using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChunkData
{
    public Vector2Int chunkCoord;
    public int size;
    public LayerData[] layers;
    public List<TileObjectData> tileObjects = new List<TileObjectData>();
    [NonSerialized] public bool isDirty;

    private const int LAYER_COUNT = 6; // DONT FORGET TO UPDATE TileLayers IN TILEDATAMANAGER YOU DUMMY
    public const string EMPTY_TILE = "__empty__";

    public void PostLoadValidate()
    {
        if (layers == null) layers = new LayerData[LAYER_COUNT];
        for (int i = 0; i < layers.Length; i++)
            if (layers[i] == null) layers[i] = new LayerData(size * size);
    }

    public ChunkData(Vector2Int coord, int chunkSize)
    {
        chunkCoord = coord;
        size = chunkSize;
        layers = new LayerData[LAYER_COUNT];
        for (int i = 0; i < LAYER_COUNT; i++)
            layers[i] = new LayerData(chunkSize * chunkSize);
    }

    public string GetTile(int layer, int x, int y)
    {
        return layers[layer].tiles[x + y * size];
    }

    public void SetTile(int layer, int x, int y, string tileID)
    {
        layers[layer].tiles[x + y * size] = tileID;
    }

    public TileObjectData GetTileObject(int worldX, int worldY)
    {
        return tileObjects.Find(t => t.worldX == worldX && t.worldY == worldY);
    }

    public void SetTileObject(TileObjectData data)
    {
        var existing = GetTileObject(data.worldX, data.worldY);
        if (existing != null) tileObjects.Remove(existing);
        tileObjects.Add(data);
        isDirty = true;
    }

    public void RemoveTileObject(int worldX, int worldY)
    {
        tileObjects.RemoveAll(t => t.worldX == worldX && t.worldY == worldY);
        isDirty = true;
    }
}

[Serializable]
public class LayerData
{
    public string[] tiles;

    public LayerData(int count)
    {
        tiles = new string[count];
    }
}