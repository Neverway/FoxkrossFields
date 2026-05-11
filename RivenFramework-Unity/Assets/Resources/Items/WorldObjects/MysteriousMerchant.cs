using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;

public class MysteriousMerchant : MonoBehaviour, IInteractable
{
    public GameObject shopWidget;
    public ScriptableItem[] stock;
    
    public void OnInteract(TDPawn_Player interactor)
    {
        GameInstance.Get<GI_WidgetManager>().AddWidget(shopWidget);
        FindObjectOfType<WB_Shop>().SetStock(stock);
    }
}
