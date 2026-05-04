using RivenFramework;
using UnityEngine;

public class WorldObject_Altertable : MonoBehaviour, IInteractable, ITileObjectReceiver
{
    private int tier = 2;
    private int worldX, worldY;
    public GameObject altertableWidget;

    public void ReceiveData(TileObjectData data)
    {
        tier = data.altertableTier > 0 ? data.altertableTier : tier;
        worldX = data.worldX;
        worldY = data.worldY;
    }

    public TileObjectData ProvideData()
    {
        return new TileObjectData
        {
            tileID = "altertable",
            worldX = worldX,
            worldY = worldY,
            altertableTier = tier
        };
    }

    public void OnInteract(TDPawn_Player interactor)
    {
        var session = new AltertableData { tier = tier };
        GameInstance.Get<GI_WidgetManager>().AddWidget(altertableWidget);
        FindObjectOfType<WB_Altertable>().SetSession(session);
    }
}