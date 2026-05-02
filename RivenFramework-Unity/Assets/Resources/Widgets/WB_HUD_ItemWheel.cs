using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WB_HUD_ItemWheel : MonoBehaviour
{
    public TDPawn_Player player;
    public Pawn_ItemInventory itemInventory;
    public GameObject itemRotator;
    public Image[] slotIcons;

    public void Update()
    {
        if (player == null)
        {
            player = FindObjectOfType<TDPawn_Player>(); 
            return;
        }

        if (itemInventory == null)
        {
            itemInventory = player.GetComponent<Pawn_ItemInventory>();
            return;
        }
        
        for (int i = 0; i < itemInventory.items.Count; i++)
        {
            if (itemInventory.items[i])
            {
                slotIcons[i].sprite = itemInventory.items[i].icon;
                slotIcons[i].enabled = true;
            }
            else
            {
                slotIcons[i].enabled = false;
            }
        }
        
        // The 36 here is the amount of degrees of rotation until we'd be over the next icon
        itemRotator.transform.localRotation = Quaternion.Euler(0,0,36*itemInventory.currentItemIndex);
    }
}
