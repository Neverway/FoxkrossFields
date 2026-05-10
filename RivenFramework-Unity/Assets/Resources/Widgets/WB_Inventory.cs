using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WB_Inventory : MonoBehaviour
{
    public TDPawn_Player player;
    public Pawn_ItemInventory itemInventory;
    public TMP_Text goldCounter;
    public Image[] slotIcons;
    public Image[] slotFrames;
    public TMP_Text[] slotStackCount;
    public Color normalFrameColor = Color.white;
    public Color selectedFrameColor = Color.yellow;
    private int pendingSwapIndex = -1;
    public WidgetNavigator navigator;
    public List<WidgetSelectable> selectables;
    public WB_ItemStats itemStats;

    public void Start()
    {
        selectables = navigator.selectableElements;
        for (int i = 0; i < selectables.Count; i++)
        {
            int capturedIndex = i;
            selectables[i].OnInteracted.AddListener(() => OnSlotInteracted(capturedIndex));
            selectables[i].OnSelected.AddListener(() => OnSlotSelect(capturedIndex));
        }
    }

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
            OnSlotSelect(0);
            return;
        }

        goldCounter.text = itemInventory.gold.ToString();
        
        for (int i = 0; i < slotIcons.Length; i++)
        {
            if (itemInventory.items[i] != null)
            {
                slotIcons[i].sprite = itemInventory.items[i].icon;
                slotIcons[i].enabled = true;
                slotStackCount[i].text = itemInventory.items[i].stackSize.ToString();
                slotStackCount[i].enabled = true;
            }
            else
            {
                slotIcons[i].enabled = false;
                slotStackCount[i].enabled = false;
            }
        }
    }

    public void OnSlotSelect(int index)
    {
        if (itemInventory == null) return;
        // If there is an item
        if (itemInventory.items[index] != null)
        {
            // Show the item stats widget
            itemStats.Show();
            // Move the item stats widget to be centered on the position of the item slot
            itemStats.SetPosition(slotFrames[index].transform.position);
            // Set the text content to being the name of the item + the item's generated description
            itemStats.SetTextContent(itemInventory.items[index].asset.displayName, itemInventory.items[index].asset.description);
        }
        // Else
        else
        {
            // Hide the item stats widget
            itemStats.Hide();
        }
    }
    
    public void OnSlotInteracted(int index)
    {
        if (pendingSwapIndex == -1)
        {
            pendingSwapIndex = index;
            slotFrames[index].color = selectedFrameColor;
        }
        else if (pendingSwapIndex == index)
        {
            slotFrames[index].color = normalFrameColor;
            pendingSwapIndex = -1;
        }
        else
        {
            (itemInventory.items[pendingSwapIndex], itemInventory.items[index]) = 
                (itemInventory.items[index], itemInventory.items[pendingSwapIndex]);

            slotFrames[pendingSwapIndex].color = normalFrameColor;
            pendingSwapIndex = -1;
        }
    }
}
