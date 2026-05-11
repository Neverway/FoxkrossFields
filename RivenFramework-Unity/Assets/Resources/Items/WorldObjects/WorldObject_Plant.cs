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

    private void AdvanceStage(int targetStage = -1)
    {
        if (targetStage < 0) growthStage++;
        else growthStage = targetStage;
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

    public void ReceiveData(TileObjectData data, float elapsedSeconds = 0f)
    {
        worldX = data.worldX;
        worldY = data.worldY;
        growthTimer = data.growthTimer;
        growthStage = data.growthStage;
        if (elapsedSeconds > 0f) FastForwardGrowth(elapsedSeconds);
    }

    public TileObjectData ProvideData()
    {
        TileObjectData objectData = new TileObjectData
        {
            tileID = plantData != null ? plantData.stages[growthStage].tileID : "",
            prefabTileID = plantData != null ? plantData.stages[0].tileID : "",
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
    
    private void FastForwardGrowth(float seconds)
    {
        if (plantData == null) return;
        float remaining = seconds;
        while (growthStage < plantData.stages.Length - 1 && remaining > 0f)
        {
            float timeLeftInStage = plantData.stages[growthStage].duration - growthTimer;
            if (remaining >= timeLeftInStage)
            {
                remaining -= timeLeftInStage;
                growthTimer = 0f;
                growthStage++;
            }
            else
            {
                growthTimer += remaining;
                remaining = 0f;
            }
        }
        if (growthStage > 0) AdvanceStage(growthStage);
    }
}