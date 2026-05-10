using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WB_Shop : MonoBehaviour
{
    public Image[] sellSlotIcons;
    public TMP_Text[] sellSlotStackCount;
    public TMP_Text[] sellSlotAmount;
    
    public Image[] buySlotIcons;
    public TMP_Text[] buySlotAmount;
    
    public Image[] slotFrames;
    
    public WidgetNavigator navigator;
    
    public TDPawn_Player player;
    public Pawn_ItemInventory itemInventory;
    public TMP_Text goldCounter;
    public WB_ItemStats itemStats;
    
    private List<ScriptableItem> buyableItems = new List<ScriptableItem>();
    private int selectedIndex = -1;

    public void Start()
    {
        for (int i = 0; i < navigator.selectableElements.Count; i++)
        {
            int capturedIndex = i;
            navigator.selectableElements[i].OnSelected.AddListener(() => OnSlotSelect(capturedIndex));
            navigator.selectableElements[i].OnInteracted.AddListener(() => OnConfirmSelect());
        }
    }

    public void SetStock(ScriptableItem[] stock)
    {
        buyableItems = new List<ScriptableItem>(stock);
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
            return;
        }

        goldCounter.text = itemInventory.gold.ToString();
        
        for (int i = 0; i < sellSlotIcons.Length; i++)
        {
            var item = itemInventory.items[i];
            if (item != null)
            {
                sellSlotIcons[i].sprite = item.icon;
                sellSlotIcons[i].enabled = true;
                sellSlotStackCount[i].text = item.stackSize.ToString();
                sellSlotStackCount[i].enabled = true;
                sellSlotAmount[i].text = item.sellAmount.ToString();
                sellSlotAmount[i].enabled = true;
            }
            else
            {
                sellSlotIcons[i].enabled = false;
                sellSlotStackCount[i].enabled = false;
                sellSlotAmount[i].enabled = false;
            }
        }
        
        for (int i = 0; i < buySlotIcons.Length; i++)
        {
            if (i < buyableItems.Count && buyableItems[i] != null)
            {
                buySlotIcons[i].sprite = buyableItems[i].icon;
                buySlotIcons[i].enabled = true;
                buySlotAmount[i].text = buyableItems[i].sellAmount.ToString();
                buySlotAmount[i].enabled = true;
            }
            else
            {
                buySlotIcons[i].enabled = false;
                buySlotAmount[i].enabled = false;
            }
        }
    }
    public void OnSlotSelect(int index)
    {
        if (itemInventory == null) return;
        selectedIndex = index;

        bool isSellSlot = index < sellSlotIcons.Length;

        if (isSellSlot)
        {
            var item = itemInventory.items[index];
            if (item != null)
            {
                itemStats.Show();
                itemStats.SetPosition(slotFrames[index].transform.position);
                itemStats.SetTextContent(item.asset.displayName, item.asset.description);
            }
            else
            {
                itemStats.Hide();
            }
        }
        else
        {
            int buyIndex = index - sellSlotIcons.Length;
            if (buyIndex < buyableItems.Count && buyableItems[buyIndex] != null)
            {
                var bItem = buyableItems[buyIndex];
                itemStats.Show();
                itemStats.SetPosition(slotFrames[index].transform.position);
                itemStats.SetTextContent(bItem.displayName, bItem.description);
            }
            else
            {
                itemStats.Hide();
            }
        }
    }
    
    public void OnConfirmSelect()
    {
        if (selectedIndex < 0 || itemInventory == null) return;

        if (selectedIndex < sellSlotIcons.Length)
            SellItem(selectedIndex);
        else
            BuyItem(selectedIndex - sellSlotIcons.Length);
    }

    private void SellItem(int inventoryIndex)
    {
        var item = itemInventory.items[inventoryIndex];
        if (item == null) return;

        itemInventory.gold += item.sellAmount;
        itemInventory.TryRemoveItem(inventoryIndex);
        itemStats.Hide();
    }

    private void BuyItem(int buyIndex)
    {
        if (buyIndex >= buyableItems.Count || buyableItems[buyIndex] == null) return;

        var item = buyableItems[buyIndex];
        int cost = item.sellAmount;

        if (itemInventory.gold < cost)
        {
            Debug.Log("[Shop] Not enough gold");
            return;
        }

        var instance = new ItemInstance(item, 1);
        if (!itemInventory.TryAddItem(instance)) return;

        itemInventory.gold -= cost;
    }

    public void CloseWidget()
    {
        Destroy(gameObject);
    }
}