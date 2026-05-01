using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GI_TileDataManager : MonoBehaviour
{
    public TileData[] tileDatabase;
    public Grid tileGrid;
    private Dictionary<string, TileBase> tileCache;
    private Tilemap[] tilemapCache;

    public void Update()
    {
        if (tileGrid == null) tileGrid = FindObjectOfType<Grid>();
        else if (tilemapCache == null) RebuildTilemapCache();
    }

    private void Awake()
    {
        tileCache = new Dictionary<string, TileBase>();
        foreach (var tile in tileDatabase)
        {
            tileCache[tile.tileID] = tile.tile;
        }
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
        foreach (var tile in tileDatabase)
        {
            if (tile.tileID == _tileID)
            {
                return tile;
            }
        }

        return tileDatabase[0];
    }
    
    public TileBase GetTileBaseFromID(string _tileID)
    {
        foreach (var tile in tileDatabase)
        {
            if (tile.tileID == _tileID)
            {
                return tile.tile;
            }
        }
        return tileDatabase[0].tile;
    }

    public Tilemap GetTilemapFromLayer(int _tileLayer)
    {
        if (tilemapCache == null) RebuildTilemapCache();
        if (_tileLayer < 0 || _tileLayer >= tilemapCache.Length) return null;
        return tilemapCache[_tileLayer];
    }
}

[Serializable]
public struct TileData
{
    public string tileID;
    public TileBase tile;
    public int tileLayer;
    public float tileDurability;
    public TileMaterialType tileMaterialType;
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
