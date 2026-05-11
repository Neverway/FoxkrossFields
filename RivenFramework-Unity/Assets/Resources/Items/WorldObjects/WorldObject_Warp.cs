using System;
using RivenFramework;
using TMPro;
using UnityEngine;

public class WorldObject_Warp : MonoBehaviour, IInteractable, ITileObjectReceiver
{
    private string targetEnvironment;
    private int targetX, targetY;
    private int worldX, worldY;
    public TMP_Text debugStats;

    private void Update()
    {
        
        debugStats.text = $"[{worldX},{worldY}] env{targetEnvironment} [{targetX},{targetY}]";
    }

    public void ReceiveData(TileObjectData data, float elapsedSeconds = 0f)
    {
        worldX = data.worldX;
        worldY = data.worldY;
        targetEnvironment = data.warpTargetEnvironment;
        targetX = data.warpTargetX;
        targetY = data.warpTargetY;
    }

    public TileObjectData ProvideData()
    {
        return new TileObjectData
        {
            tileID = "cave_entrance_warp",
            prefabTileID = "cave_entrance_warp",
            worldX = worldX,
            worldY = worldY,
            warpTargetEnvironment = targetEnvironment,
            warpTargetX = targetX,
            warpTargetY = targetY
        };
    }

    public void OnInteract(TDPawn_Player interactor)
    {
        GameInstance.Get<GI_EnvironmentManager>().WarpTo(
            targetEnvironment,
            new Vector2Int(targetX, targetY),
            interactor);
    }
}