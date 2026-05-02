using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ScriptableItem : ScriptableObject
{
    [Tooltip("The string ID to refer to this item")]
    public string id;
    [Tooltip("The name of the item as it appears in the inventory")]
    public string displayName;
    [Tooltip("The sprite that represents this item")]
    public Sprite icon;
    [Tooltip("The game object that is created when using the item")]
    public GameObject associatedGameObject;
    [Tooltip("How many of this item can be held in the same inventory slot")]
    public int maxStackSize;
    [Tooltip("How many of this item are in this slot")]
    public int currentStackSize;
    [Tooltip("The details about this item")]
    [TextArea] public string description;
    [Tooltip("If false, this is considered a key item and cannot be dropped")]
    public bool isDiscardable = true;

    public virtual void OnDefaultInteract(TDPawn_Player _owner) { }
    public virtual void OnDefaultAction(TDPawn_Player _owner) { }
    public virtual void OnDefaultReleaseInteract(TDPawn_Player _owner) { }
    public virtual void OnDefaultReleaseAction(TDPawn_Player _owner) { }
    
    public abstract void OnItemUsePrimary(TDPawn_Player _owner);
    public abstract void OnItemUseSecondary(TDPawn_Player _owner);
    public abstract void OnItemReleasePrimary(TDPawn_Player _owner);
    public abstract void OnItemReleaseSecondary(TDPawn_Player _owner);
}

[Serializable]
public class ScriptableItemLootTable
{
    public float chanceToDrop = 0.5f;
    public ScriptableItem[] items;
}
