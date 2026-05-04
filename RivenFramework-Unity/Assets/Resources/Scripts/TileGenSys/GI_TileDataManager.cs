using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GI_TileDataManager : MonoBehaviour
{
    public TileData[] tileDatabase;
    public Grid tileGrid;
    private Dictionary<string, TileData> tileDataCache;
    private Tilemap[] tilemapCache;

    public void Update()
    {
        if (tileGrid == null) tileGrid = FindObjectOfType<Grid>();
        else if (tilemapCache == null) RebuildTilemapCache();
    }

    private void Awake()
    {
        tileDataCache = new Dictionary<string, TileData>();
        foreach (var tile in tileDatabase)
            tileDataCache[tile.tileID] = tile;
    }

    private void Start()
    {
        if (tileGrid != null) RebuildTilemapCache();
    }

    public void RebuildTilemapCache()
    {
        tilemapCache = tileGrid.gameObject.GetComponentsInChildren<Tilemap>();
    }

    public TileData GetTileDataFromID(string _tileID)
    {
        if (tileDataCache.TryGetValue(_tileID, out var data)) return data;
        return tileDatabase[0];
    }

    public TileData GetTileDataFromTileBase(TileBase _tile)
    {
        foreach (var data in tileDatabase)
        {
            if (data.tile == _tile) return data;
        }
        return tileDatabase[0];
    }
    
    public TileBase GetTileBaseFromID(string _tileID)
    {
        if (tileDataCache.TryGetValue(_tileID, out var data)) return data.tile;
        return tileDatabase[0].tile;
    }

    public Tilemap GetTilemapFromLayer(int _tileLayer)
    {
        if (tilemapCache == null) RebuildTilemapCache();
        if (_tileLayer < 0 || _tileLayer >= tilemapCache.Length) return null;
        return tilemapCache[_tileLayer];
    }
    
    public int GetTilemapCount()
    {
        if (tilemapCache == null) RebuildTilemapCache();
        return tilemapCache?.Length ?? 0;
    }
}

[Serializable]
public struct TileData
{
    public string tileID;
    public TileBase tile;
    public TileLayers tileLayer;
    public float tileDurability;
    public TileMaterialType tileMaterialType;
    public ScriptableItemLootTable[] drops;
    public GameObject associatedPrefab;
}

public enum TileMaterialType
{
    generic,
    soil,
    stone,
    wood,
    metal,
    leaves,
    glass,
    fluid
}

// DONT FORGET TO UPDATE LAYER_COUNT in CHUNK DATA YOU DUMMY
public enum TileLayers
{
    Ground = 0,
    Path = 1,
    Objects = 2,
    ObjectsSolid = 3,
    Fluids = 4,
    Overlay = 5,
    None = -1
}