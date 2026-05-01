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
    [NonSerialized] public bool isDirty;

    private const int LAYER_COUNT = 4;
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