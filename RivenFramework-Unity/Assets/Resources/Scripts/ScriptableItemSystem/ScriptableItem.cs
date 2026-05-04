using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SI_", menuName = "Neverway/ScriptableItem")]
public class ScriptableItem : ScriptableObject
{
    [Header("Item Details")]
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
    
    [Header("Properties")]
    [Tooltip("What the item is primarily composed of")]
    public ItemMaterial material;
    [Tooltip("The variant name for items that share a form, like sword, dagger, knife")]
    public string subType;
    [Tooltip("How the item works")]
    public List<ItemForm> forms;
    [Tooltip("How the item affects other things")]
    public List<ItemTag> tags;
    
    public virtual void OnItemUsePrimary(TDPawn_Player _owner) => ItemBehaviorResolver.ExecutePrimary(this, _owner);
    public virtual void OnItemUseSecondary(TDPawn_Player _owner) => ItemBehaviorResolver.ExecuteSecondary(this, _owner);
    public virtual void OnItemReleasePrimary(TDPawn_Player _owner) => ItemBehaviorResolver.ExecuteReleasePrimary(this, _owner);
    public virtual void OnItemReleaseSecondary(TDPawn_Player _owner) => ItemBehaviorResolver.ExecuteReleaseSecondary(this, _owner);
}

[Serializable]
public class ScriptableItemLootTable
{
    public float chanceToDrop = 0.5f;
    public ScriptableItem[] items;
}
