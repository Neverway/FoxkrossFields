using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileObjectData
{
    public string tileID;
    public string prefabTileID;
    public int worldX, worldY;
    public long unloadTimestamp;
    
    public string warpTargetEnvironment;
    public int warpTargetX, warpTargetY;
    
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
