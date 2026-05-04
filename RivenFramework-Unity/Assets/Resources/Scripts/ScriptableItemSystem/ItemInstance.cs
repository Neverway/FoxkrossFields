using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemInstance
{
    [Tooltip("Reference to the source asset")]
    public ScriptableItem asset;
    public int stackSize;

    public ItemInstance(ScriptableItem asset, int stackSize = 1)
    {
        this.asset = asset;
        this.stackSize = stackSize;
    }

    public string id => asset.id;
    public string displayName => asset.displayName;
    public Sprite icon => asset.icon;
    public int maxStackSize => asset.maxStackSize;
    public bool isDiscardable => asset.isDiscardable;
    
    public void OnItemUsePrimary(TDPawn_Player owner) => asset.OnItemUsePrimary(owner);
    public void OnItemUseSecondary(TDPawn_Player owner) => asset.OnItemUseSecondary(owner);
    public void OnItemReleasePrimary(TDPawn_Player owner) => asset.OnItemReleasePrimary(owner);
    public void OnItemReleaseSecondary(TDPawn_Player owner) => asset.OnItemReleaseSecondary(owner);
}
