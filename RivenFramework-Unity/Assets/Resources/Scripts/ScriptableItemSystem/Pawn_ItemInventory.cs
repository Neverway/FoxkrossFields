using System;
using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class Pawn_ItemInventory : MonoBehaviour
{
    public List<ScriptableItem> startingItems;
    [NonSerialized] public List<ItemInstance> items = new List<ItemInstance>();
    public int currentItemIndex;
    public TDPawn_Player owner;
    public float holdTimer = 0f;
    public int inventorySize = 10;
    public int hotbarSize = 10;
    public int gold = 0;

    private void Start()
    {
        owner = GetComponent<TDPawn_Player>();
        for (int i = 0; i < inventorySize; i++) items.Add(null);
        if (!GameInstance.Get<GI_SaveManager>().HasPlayerSave())
        {
            foreach (var item in startingItems)
            {
                if (item != null) TryAddItem(item);
            }
        }
        
    }

    /// <summary>
    /// Tries to find an existing stack for an item type that's not full and returns the inventory index if found, otherwise returns -1
    /// </summary>
    private int GetExistingStack(ScriptableItem _item)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null) continue;
            if (items[i].id == _item.id && items[i].stackSize < items[i].maxStackSize) return i;
        }
        return -1;
    }

    /// <summary>
    /// Tries to find an empty slot and returns the inventory index if found, otherwise returns -1
    /// </summary>
    private int GetNextFreeSlot()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null) return i;
        }
        return -1;
    }
    
    /// <summary>
    /// Attempts to add an item to the inventory if it's not full and returns true if successful
    /// </summary>
    public bool TryAddItem(ScriptableItem _item)
    {
        if (_item == null) return false;
        var existingStack = GetExistingStack(_item);
        var nextFreeSlot = GetNextFreeSlot();
        if (existingStack != -1)
        {
            items[existingStack].stackSize++;
            return true;
        }
        else if (nextFreeSlot != -1)
        {
            items[nextFreeSlot] = new ItemInstance(_item, 1);
            return true;
        }
        
        return false;
    }
    
    public bool TryAddItem(ItemInstance _instance)
    {
        if (_instance == null || _instance.asset == null) return false;
        var existingStack = GetExistingStack(_instance.asset);
        var nextFreeSlot = GetNextFreeSlot();
        if (existingStack != -1)
        {
            items[existingStack].stackSize++;
            return true;
        }
        else if (nextFreeSlot != -1)
        {
            items[nextFreeSlot] = _instance;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Attempts to remove an item of the specified type, returns true if successful
    /// </summary>
    public bool TryRemoveItem(Item _item)
    {
        return false;
    }

    /// <summary>
    /// Attempts to remove an item at the specified index, returns true if successful
    /// </summary>
    public bool TryRemoveItem(int _itemIndex)
    {
        // If there is an item at the index
        if (items[_itemIndex] == null) return false;
        if (items[_itemIndex].isDiscardable == false) return false;
        
        // It's a stack
        if (items[_itemIndex].maxStackSize > 1)
        {
            // Remove 1 from stack
            items[_itemIndex].stackSize--;
            // If stack is empty, remove item
            if (items[_itemIndex].stackSize == 0) items[_itemIndex] = null;
        }
        // It's not a stack
        else
        {
            // Remove item
            items[_itemIndex] = null;
        }
        return true;
    }
    
    public bool TryRemoveItemInstance(ItemInstance _instance)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null) continue;
            if (items[i] != _instance && items[i].id != _instance.id) continue;
            return TryRemoveItem(i);
        }
        return false;
    }
    
    /// <summary>
    /// Switch to the next item in the inventory
    /// </summary>
    public void NextItem()
    {
        if (currentItemIndex >= hotbarSize-1) currentItemIndex = 0;
        else currentItemIndex++;
    }

    /// <summary>
    /// Switch to the previous item in the inventory
    /// </summary>
    public void PreviousItem()
    {
        if (currentItemIndex <= 0)
        {
            currentItemIndex = hotbarSize-1;
        }
        else
        {
            currentItemIndex--;
        }
    }
    
    public void ItemUsePrimary()
    {
        var heldItem = GetCurrentItem();
        if (heldItem == null)
        {
            ItemBehaviorResolver.DefaultPrimaryAction(null, owner);
            return;
        }
        heldItem.OnItemUsePrimary(owner);
    }

    public void ItemUseSecondary()
    {
        var heldItem = GetCurrentItem();
        if (heldItem == null)
        {
            ItemBehaviorResolver.DefaultSecondaryAction(null, owner);
            return;
        }
        heldItem.OnItemUseSecondary(owner);
    }

    public void ItemReleasePrimary()
    {
        var heldItem = GetCurrentItem();
        if (heldItem == null)
        {
            ItemBehaviorResolver.DefaultPrimaryRelease(null, owner);
            return;
        }
        heldItem.OnItemReleasePrimary(owner);
    }

    public void ItemReleaseSecondary()
    {
        var heldItem = GetCurrentItem();
        if (heldItem == null)
        {
            ItemBehaviorResolver.DefaultSecondaryRelease(null, owner);
            return;
        }
        heldItem.OnItemReleaseSecondary(owner);
    }
    
    public ItemInstance GetCurrentItem()
    {
        if (items == null || items.Count == 0) return null;
        if (currentItemIndex < 0 || currentItemIndex >= items.Count) return null;
        return items[currentItemIndex];
    }

}
