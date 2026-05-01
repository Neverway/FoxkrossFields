using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Debug_TileEditTool : MonoBehaviour
{
    [Header("Tiles")]
    public Tilemap tilemap;
    public string selectedTile;

    [Header("Destroy")]
    public float destroyHoldTime = 0.5f;
    private float holdTimer = 0f;
    
    void Update()
    {
        HandleInput();
    }
    
    void HandleInput()
    {
        if (Input.GetKey(KeyCode.X))
        {
            var heldTileData = GameInstance.Get<GI_TileDataManager>().GetTileDataFromID(selectedTile);
            tilemap = GameInstance.Get<GI_TileDataManager>().GetTilemapFromLayer(heldTileData.tileLayer);
            
            Vector3Int cell = tilemap.WorldToCell(transform.position);
            if (tilemap.GetTile(cell) == null)
            {
                GameInstance.Get<GI_TileChunkManager>().RecalculateLeaves(cell);
                tilemap.SetTile(cell, heldTileData.tile);
                GameInstance.Get<GI_TileChunkManager>().MarkTileDirty(cell, heldTileData.tileLayer, selectedTile);
            }
        }

        if (Input.GetKey(KeyCode.Z))
        {
            var heldTileData = GameInstance.Get<GI_TileDataManager>().GetTileDataFromID(selectedTile);
            tilemap = GameInstance.Get<GI_TileDataManager>().GetTilemapFromLayer(heldTileData.tileLayer);
            
            Vector3Int cell = tilemap.WorldToCell(transform.position);
            TileBase tile = tilemap.GetTile(cell);

            if (tile)
            {
                //if (tile.name == heldTileData.tile.name)
                //{
                    holdTimer += Time.deltaTime;
                    if (holdTimer >= destroyHoldTime)
                    {
                        bool removingTrunk = heldTileData.tileLayer == 3;
                        Vector3Int removedCell = cell;
                        
                        tilemap.SetTile(cell, null);
                        GameInstance.Get<GI_TileChunkManager>().MarkTileDirty(cell, heldTileData.tileLayer, null);

                        if (removingTrunk)
                        {
                            GameInstance.Get<GI_TileChunkManager>().RecalculateLeaves(removedCell);
                        }
                        
                        holdTimer = 0f;
                    }
                //}
            }
            
            else holdTimer = 0f;
        }
        else holdTimer = 0f;
    }
}
