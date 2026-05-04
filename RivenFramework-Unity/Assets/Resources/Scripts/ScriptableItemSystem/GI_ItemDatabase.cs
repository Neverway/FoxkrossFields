using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores a list of all non-generated scriptable items in the game for easy lookups
/// </summary>
public class GI_ItemDatabase : MonoBehaviour
{
    [Tooltip("A list of every item in the game, manually add items here when you create them (DOM'T FORGET THIS STEP DUMMY)")]
    public List<ScriptableItem> items;
    [Tooltip("A quick lookup table for getting scriptable items based on their id")]
    private Dictionary<string, ScriptableItem> lookup;

    private void Awake()
    {
        // Setup quick lookup table
        lookup = new Dictionary<string, ScriptableItem>();
        foreach (var item in items)
        {
            if (item != null) lookup[item.id] = item;
        }
    }
    
    /// <summary>
    /// Get an existing non-generated item based on an id
    /// </summary>
    public ScriptableItem GetItem(string id)
    {
        lookup.TryGetValue(id, out var item);
        return item;
    }
    
    /// <summary>
    /// Get the icon for an item based on its material, subtype, or by id if the others are invalid
    /// </summary>
    public Sprite GetIcon(ItemMaterial material, string subType, string id)
    {
        // Get a matching icon based on material and form subtype
        foreach (var entry in items)
            if (entry.material == material && entry.subType == subType) return entry.icon;
        // Get a matching icon based on id
        foreach (var entry in items)
            if (entry.id == id) return entry.icon;
        // Fallback to current icon
        return null;
    }
}
