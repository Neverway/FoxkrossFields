using System;
using RivenFramework;
using TMPro;
using UnityEngine;

public class WorldObject_Plant : MonoBehaviour, IInteractable, ITileObjectReceiver
{
    public ScriptablePlant plantData;
    private int worldX, worldY;
    private float growthTimer;
    private int growthStage;
    public TMP_Text debugStats;

    public void Update()
    {
        if (plantData == null || plantData.stages == null) return;
        if (growthStage >= plantData.stages.Length - 1) return;

        growthTimer += Time.deltaTime;

        if (growthTimer >= plantData.stages[growthStage].duration)
        {
            growthTimer = 0f;
            AdvanceStage();
        }
        
        debugStats.text = $"[{worldX},{worldY}] stage{growthStage}/{plantData.stages.Length-1} timer{growthTimer:F2}";
    }

    private void AdvanceStage()
    {
        growthStage++;
        if (growthStage >= plantData.stages.Length) return;

        string newTileId = plantData.stages[growthStage].tileID;
        var cell = new Vector3Int(worldX, worldY, 0);
        var chunkManager = GameInstance.Get<GI_TileChunkManager>();
        var tileDataManager = GameInstance.Get<GI_TileDataManager>();
        var tileData = tileDataManager.GetTileDataFromID(newTileId);
        var tilemap = tileDataManager.GetTilemapFromLayer((int)tileData.tileLayer);
        
        tilemap.SetTile(cell, tileData.tile);
        chunkManager.MarkTileDirty(cell, (int)tileData.tileLayer, newTileId);
        chunkManager.UpdatePlantTileID(cell, newTileId);
    }

    public void ReceiveData(TileObjectData data)
    {
        worldX = data.worldX;
        worldY = data.worldY;
        growthTimer = data.growthTimer;
        growthStage = data.growthStage;
    }

    public TileObjectData ProvideData()
    {
        TileObjectData objectData = new TileObjectData
        {
            tileID = plantData != null ? plantData.stages[growthStage].tileID : "",
            worldX = worldX,
            worldY = worldY,
            growthTimer = growthTimer,
            growthStage = growthStage
        };
        
        return objectData;
    }

    public void OnInteract(TDPawn_Player interactor)
    {
    }
}