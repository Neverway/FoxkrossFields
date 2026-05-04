using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileObjectData
{
    public string tileID;
    public int worldX, worldY;
    
    public int altertableTier = 2;

    public float growthTimer = 0f;
    public int growthStage = 0;

    public List<InventoryItemSaveData> chestInventory;

    [Serializable]
    public class InventoryItemSaveData
    {
        public string id;
        public int stackSize;
    }
}
