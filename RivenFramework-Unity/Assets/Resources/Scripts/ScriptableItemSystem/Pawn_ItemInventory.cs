using System;
using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class Pawn_ItemInventory : MonoBehaviour
{
    [Tooltip("")]
    public List<ScriptableItem> items;
    public int currentItemIndex;
    public TDPawn_Player owner;
    public float holdTimer = 0f;

    private void Start()
    {
        owner = GetComponent<TDPawn_Player>();
    }

    /// <summary>
    /// Tries to find an existing stack for an item type that's not full and returns the inventory index if found, otherwise returns -1
    /// </summary>
    private int GetExistingStack(ScriptableItem _item)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (!items[i]) continue;
            if (items[i].id == _item.id && items[i].currentStackSize < items[i].maxStackSize) return i;
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
            if (!items[i]) return i;
        }
        return -1;
    }
    
    /// <summary>
    /// Attempts to add an item to the inventory if it's not full and returns true if successful
    /// </summary>
    public bool TryAddItem(ScriptableItem _item)
    {
        var existingStack = GetExistingStack(_item);
        var nextFreeSlot = GetNextFreeSlot();
        if (existingStack != -1)
        {
            items[existingStack].currentStackSize++;
            return true;
        }
        else if (nextFreeSlot != -1)
        {
            items[nextFreeSlot] = _item;
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
        if (items[_itemIndex])
        {
            if (items[_itemIndex].isDiscardable == false) return false;
            
            // It's a stack
            if (items[_itemIndex].maxStackSize != 0)
            {
                // Remove 1 from stack
                items[_itemIndex].currentStackSize--;
                // If stack is empty, remove item
                if (items[_itemIndex].currentStackSize == 0) items[_itemIndex] = null;
                return true;
            }
            // It's not a stack
            else
            {
                // Remove item
                items[_itemIndex] = null;
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Switch to the next item in the inventory
    /// </summary>
    public void NextItem()
    {
        if (currentItemIndex >= items.Count-1) currentItemIndex = 0;
        else currentItemIndex++;
    }

    /// <summary>
    /// Switch to the previous item in the inventory
    /// </summary>
    public void PreviousItem()
    {
        if (currentItemIndex <= 0)
        {
            currentItemIndex = items.Count-1;
        }
        else
        {
            currentItemIndex--;
        }
    }

    
    
    public void ItemUsePrimary()
    {
        ScriptableItem heldItem = GetCurrentItem();
        if (heldItem == null) { DefaultPrimaryAction(); return; }
        heldItem.OnItemUsePrimary(owner);
    }

    public void ItemUseSecondary()
    {
        ScriptableItem heldItem = GetCurrentItem();
        if (heldItem == null) { DefaultSecondaryAction(); return; }
        heldItem.OnItemUseSecondary(owner);
    }

    public void ItemReleasePrimary()
    {
        ScriptableItem heldItem = GetCurrentItem();
        if (heldItem == null) { DefaultReleasePrimary(); return; }
        heldItem.OnItemReleasePrimary(owner);
    }

    public void ItemReleaseSecondary()
    {
        ScriptableItem heldItem = GetCurrentItem();
        if (heldItem == null) { DefaultReleaseSecondary(); return; }
        heldItem.OnItemReleaseSecondary(owner);
    }

    
    public HashSet<int> unbreakableLayers = new HashSet<int> { 0, 1, 4, 5 };
    public void DefaultPrimaryAction()
    {
        var tileDataManager = owner.tileDataManager;
        Vector3 attachPos = owner.physObjectAttachmentPoint.transform.position;
        

        for (int layer = tileDataManager.GetTilemapCount() - 1; layer >= 0; layer--)
        {
            if (unbreakableLayers.Contains(layer)) continue;
            Tilemap tilemap = tileDataManager.GetTilemapFromLayer(layer);
            if (tilemap == null) continue;

            Vector3Int cell = tilemap.WorldToCell(attachPos);
            TileBase tile = tilemap.GetTile(cell);
            if (tile == null) continue;

            TileData tileData = tileDataManager.GetTileDataFromTileBase(tile);

            float durability = tileData.tileDurability > 0 ? tileData.tileDurability : 1f;
            float breakTime = owner.TDCurrentStats.destroyHoldTime * durability;

            holdTimer += Time.deltaTime;

            if (holdTimer >= breakTime)
            {
                bool removingTrunk = layer == 3;
                tilemap.SetTile(cell, null);
                GameInstance.Get<GI_TileChunkManager>().MarkTileDirty(cell, layer, null);

                if (removingTrunk)
                    GameInstance.Get<GI_TileChunkManager>().RecalculateLeaves(cell);

                if (tileData.drops != null)
                {
                    foreach (var drop in tileData.drops)
                    {
                        var randomChance = Random.Range(0f, 1f);
                        if (drop.chanceToDrop >= randomChance)
                        {
                            foreach (var item in drop.items)
                            {
                                TryAddItem(item);
                            }
                        }
                    }
                }

                holdTimer = 0f;
            }
            return;
        }

        holdTimer = 0f;
    }

    public void DefaultSecondaryAction()
    {
        ScriptableItem heldItem = GetCurrentItem();
        if (heldItem != null) heldItem.OnDefaultAction(owner);
    }
    
    public void DefaultReleasePrimary()
    {
        holdTimer = 0f;
        ScriptableItem heldItem = GetCurrentItem();
        if (heldItem != null) heldItem.OnDefaultReleaseInteract(owner);
    }

    public void DefaultReleaseSecondary()
    {
        ScriptableItem heldItem = GetCurrentItem();
        if (heldItem != null) heldItem.OnDefaultReleaseAction(owner);
    }
    
    
    public ScriptableItem GetCurrentItem()
    {
        if (items == null || items.Count == 0) return null;
        if (currentItemIndex < 0 || currentItemIndex >= items.Count) return null;
        return items[currentItemIndex];
    }

}
