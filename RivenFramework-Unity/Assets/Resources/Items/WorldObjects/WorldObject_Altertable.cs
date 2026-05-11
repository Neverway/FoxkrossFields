using System;
using RivenFramework;
using TMPro;
using UnityEngine;

public class WorldObject_Altertable : MonoBehaviour, IInteractable, ITileObjectReceiver
{
    private int tier = 2;
    private int worldX, worldY;
    public GameObject altertableWidget;
    public TMP_Text debugStats;

    public void Update()
    {
        debugStats.text = $"[{worldX},{worldY}] {tier}";
    }

    public void ReceiveData(TileObjectData data, float elapsedSeconds = 0f)
    {
        tier = data.altertableTier > 0 ? data.altertableTier : tier;
        worldX = data.worldX;
        worldY = data.worldY;
    }

    public TileObjectData ProvideData()
    {
        TileObjectData objectData = new TileObjectData
        {
            tileID = "altertable",
            worldX = worldX,
            worldY = worldY,
            altertableTier = tier
        };
        
        return objectData;
    }

    public void OnInteract(TDPawn_Player interactor)
    {
        var session = new AltertableData { tier = tier };
        GameInstance.Get<GI_WidgetManager>().AddWidget(altertableWidget);
        FindObjectOfType<WB_Altertable>().SetSession(session);
    }
}