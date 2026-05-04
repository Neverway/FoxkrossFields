using System.Collections.Generic;
using RivenFramework;
using UnityEngine;

/// <summary>
/// Used by the Altertable, this class handles the fusion, imbuing, and deconstruction of scriptable items
/// It can generate new items based on the operation or find matching items that already exist
/// </summary>
public class AlchemySystem
{
    /// <summary>
    /// && FUSION: merges the forms and tags of two items, inheriting the strongest material
    /// </summary>
    public static ItemInstance Fuse(ItemInstance a, ItemInstance b)
    {
        if (a == null || b == null) return null;
        var result = ScriptableObject.CreateInstance<ScriptableItem>();
        
        // Merge the forms of the items
        var mergedForms = new List<ItemForm>(a.asset.forms);
        foreach (var form in b.asset.forms)
        {
            if (!mergedForms.Contains(form)) mergedForms.Add(form);
        }
        result.forms = mergedForms;
        
        // Merge the tags of the items
        var mergedTags = new List<ItemTag>(a.asset.tags);
        foreach (var tag in b.asset.tags)
        {
            if (!mergedTags.Contains(tag)) 
                mergedTags.Add(tag);
        }
        result.tags = mergedTags;
        
        // Inherit the strongest material
        result.material = (ItemMaterial)Mathf.Max((int)a.asset.material, (int)b.asset.material);
        
        // Generate a name and id for the new item
        result.subType = a.asset.subType;
        result.displayName = GenerateName(result);
        result.id = GenerateID(result);
        result.description = GenerateDescription(result);
        result.maxStackSize = a.asset.maxStackSize;
        result.isDiscardable = true;
        
        // Choose an icon from the dominate item
        var resolvedIcon = GameInstance.Get<GI_ItemDatabase>().GetIcon(result.material, result.subType, result.id);
        result.icon = resolvedIcon != null ? resolvedIcon : a.asset.icon;
        
        return ResolveToKnownItem(result);
    }

    /// <summary>
    /// || IMBUE: merges the dominate tag of item B onto item A
    /// </summary>
    public static ItemInstance Imbue(ItemInstance a, ItemInstance b)
    {
        if (a == null || b == null) return null;
        var result = ScriptableObject.CreateInstance<ScriptableItem>();

        // Inherit the forms and material from a
        result.forms = new List<ItemForm>(a.asset.forms);
        result.material = a.asset.material;

        // Merge the tags of the items
        var mergedTags = new List<ItemTag>(a.asset.tags);
        foreach (var tag in b.asset.tags)
        {
            if (!mergedTags.Contains(tag))
            {
                mergedTags.Add(tag);
                break;
            }
        }
        result.tags = mergedTags;

        // Generate a name and id for the new item
        result.subType = a.asset.subType;
        result.displayName = GenerateName(result);
        result.id = GenerateID(result);
        result.description = GenerateDescription(result);
        result.maxStackSize = a.asset.maxStackSize;
        result.isDiscardable = true;
        
        // Choose an icon from the dominate item
        var resolvedIcon = GameInstance.Get<GI_ItemDatabase>().GetIcon(result.material, result.subType, result.id);
                result.icon = resolvedIcon != null ? resolvedIcon : a.asset.icon;
        
        return ResolveToKnownItem(result);
    }
    
    /// <summary>
    /// ## DECONSTRUCTION: Divides an item into it's base components
    /// Tier 1 - Extract raw material resource
    /// Tier 2 - Extract tags as elemental essences
    /// </summary>
    public static List<ItemInstance> Deconstruct(ItemInstance item, int altertableTier = 2)
    {
        var results = new List<ItemInstance>();
        if (item == null) return results;

        // Tier 1 Extract material
        var materialItem = TryGetMaterialDrop(item.asset.material);
        if (materialItem != null) results.Add(materialItem);

        // Tier 2 Extract essences
        Debug.Log($"[Alchemy] tier {altertableTier}");
        //if (altertableTier >= 1)
        //{
            foreach (var tag in item.asset.tags)
            {
                var essence = TryGetTagEssence(tag);
                Debug.Log($"[Alchemy] Essence result: {(essence == null ? "NULL - missing from database!" : essence.id)}");
                if (essence != null)
                {
                    results.Add(essence);
                }
            }
        //}

        return results;
    }
    
    // -----------------------------------------------------------------------
    // Helper functions
    // -----------------------------------------------------------------------
    
    /// <summary>
    /// Get the adjective for whatever the item form is (used when generating a name for an item)
    /// </summary>
    private static string FormAdjective(ItemForm form)
    {
        switch (form)
        {
            case ItemForm.Hoe: return "Tilling";
            case ItemForm.Pickaxe: return "Mining";
            case ItemForm.Hatchet: return "Chopping";
            case ItemForm.Blade: return "Bladed";
            case ItemForm.Polearm: return "Piercing";
            case ItemForm.Bludgeon: return "Bludgeoning";
            default: return form.ToString();
        }
    }
    
    /// <summary>
    /// Get the noun for whatever the item form is (used when generating a name for an item)
    /// </summary>
    private static string FormNoun(ItemForm form)
    {
        switch (form)
        {
            case ItemForm.Blade: return "blade";
            case ItemForm.Polearm: return "polearm";
            case ItemForm.Bludgeon: return "bludgeon";
            case ItemForm.Hoe: return "hoe";
            case ItemForm.Pickaxe: return "pickaxe";
            case ItemForm.Hatchet: return "hatchet";
            default: return form.ToString().ToLower();
        }
    }
    
    /// <summary>
    /// Create a name for new items based on its material, form, and tags
    /// </summary>
    private static string GenerateName(ScriptableItem item)
    {
        string materialPrefix = item.material != ItemMaterial.None ? item.material.ToString() : "";
        var visibleForms = item.forms.FindAll(f => f != ItemForm.None);
        string formName;
        string tagSuffix = "";
        
        if (visibleForms.Count == 0)
        {
            // No forms
            formName = "Shard";
        }
        else if (visibleForms.Count == 1)
        {
            // One form
            formName = !string.IsNullOrEmpty(item.subType) ? System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.subType) : visibleForms[0].ToString();
        }
        else
        {
            // Multiple forms
            // Add all the non-primary forms as adjectives
            string adjectives = "";
            for (int i = 1; i < visibleForms.Count; i++)
            {
                adjectives += FormAdjective(visibleForms[i]) + " ";
            }
            // Add the primary form as a noun
            string noun;
            if (!string.IsNullOrEmpty(item.subType))
            {
                noun = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.subType);
            }
            else
            {
                noun = visibleForms[0].ToString();
            }
            // Set the final form name
            formName = adjectives + noun;
        }

        // Tags
        foreach (var tag in item.tags)
        {
            switch (tag)
            {
                case ItemTag.Earth: tagSuffix += "Earth "; break;
                case ItemTag.Fire: tagSuffix += "Fire "; break;
                case ItemTag.Water: tagSuffix += "Water "; break;
                case ItemTag.Air: tagSuffix += "Air "; break;
                case ItemTag.Sharp: tagSuffix += "Sharp "; break;
                case ItemTag.Blunt: tagSuffix += "Blunt "; break;
                case ItemTag.Poison: tagSuffix += "Poison "; break;
                case ItemTag.Bouncy: tagSuffix += "Bouncy "; break;
            }
        }

        //return $"{tagSuffix}{materialPrefix} {formName}".Trim();
        return $"{materialPrefix} {formName}".Trim();
    }
    
    /// <summary>
    /// Create an id for new items based on its material, form, and tags
    /// </summary>
    private static string GenerateID(ScriptableItem item)
    {
        string material;
        var visibleForms = item.forms.FindAll(f => f != ItemForm.None);
        string formPart;
        string tags;
        
        // Material
        if (item.material != ItemMaterial.None) material = item.material.ToString().ToLower() + "_";
        else material = "";

        // Forms
        if (visibleForms.Count == 0)
        {
            // No form
            formPart = "shard";
        }
        else if (visibleForms.Count == 1)
        {
            // One form
            if (!string.IsNullOrEmpty(item.subType)) formPart = item.subType;
            else formPart = FormNoun(visibleForms[0]);
        }
        else
        {
            // Multiple forms
            formPart = string.Join("_", visibleForms.ConvertAll(f => FormNoun(f)));
        }

        // Tags
        if (item.tags.Count > 0) tags = "_" + string.Join("_", item.tags.ConvertAll(t => t.ToString().ToLower()));
        else tags = "";

        return $"{material}{formPart}";
    }
    
    /// <summary>
    /// Create a description for new items based on its form and tags
    /// </summary>
    private static string GenerateDescription(ScriptableItem item)
    {
        var parts = new List<string>();
        foreach (var form in item.forms)
        {
            switch (form)
            {
                case ItemForm.Blade: parts.Add("Slashes enemies"); break;
                case ItemForm.Hoe: parts.Add("Tills soil"); break;
                case ItemForm.Pickaxe: parts.Add("Mines stone and ore"); break;
                case ItemForm.Hatchet: parts.Add("Chops wood"); break;
                case ItemForm.Polearm: parts.Add("Pierces enemies"); break;
                case ItemForm.Bludgeon: parts.Add("Bludgeons enemies"); break;
            }
        }
        foreach (var tag in item.tags)
        {
            switch (tag)
            {
                case ItemTag.Earth: parts.Add("slows"); break;
                case ItemTag.Fire: parts.Add("ignites"); break;
                case ItemTag.Water: parts.Add("soaks"); break;
                case ItemTag.Air: parts.Add("blows"); break;
                case ItemTag.Poison: parts.Add("poisons"); break;
                case ItemTag.Sharp: parts.Add("pierces"); break;
                case ItemTag.Blunt: parts.Add("staggers"); break;
                case ItemTag.Bouncy: parts.Add("bounces"); break;
            }
        }
        return parts.Count > 0 ? string.Join(" and ", parts) + "." : "A mysterious alchemical creation.";
    }
    
    /// <summary>
    /// Get an existing item from a generated item if a match is found in the item registry
    /// </summary>
    private static ItemInstance ResolveToKnownItem(ScriptableItem generated)
    {
        var existing = GameInstance.Get<GI_ItemDatabase>().GetItem(generated.id);
        if (existing != null) return new ItemInstance(existing, 1);

        return new ItemInstance(generated, 1);
    }
    
    /// <summary>
    /// Gets the material item based on an item's material (used when deconstructing)
    /// </summary>
    private static ItemInstance TryGetMaterialDrop(ItemMaterial material)
    {
        string id = material switch
        {
            ItemMaterial.Bone => "bone",
            ItemMaterial.Wood => "wood",
            ItemMaterial.Flint => "flint",
            ItemMaterial.Copper => "copper_ingot",
            ItemMaterial.Bronze => "bronze_ingot",
            ItemMaterial.Iron => "iron_ingot",
            ItemMaterial.Steel => "steel_ingot",
            _ => null
        };
        if (id == null)
        {
            Debug.Log($"Failed to get material {material}");
            return null;
        }
        var asset = GameInstance.Get<GI_ItemDatabase>().GetItem(id);
        return asset != null ? new ItemInstance(asset, 1) : null;
    }
    
    /// <summary>
    /// Gets the essence item based on an item's tags (used when deconstructing)
    /// </summary>
    private static ItemInstance TryGetTagEssence(ItemTag tag)
    {
        string id = tag switch
        {
            ItemTag.Earth => "essence_earth",
            ItemTag.Water => "essence_water",
            ItemTag.Fire => "essence_fire",
            ItemTag.Air => "essence_air",
            ItemTag.Poison => "essence_poison",
            ItemTag.Sharp => "essence_sharp",
            ItemTag.Blunt => "essence_blunt",
            ItemTag.Bouncy => "essence_bouncy",
            _ => null
        };
        if (id == null)
        {
            Debug.Log($"Failed to get essence {tag}");
            return null;
        }
        var asset = GameInstance.Get<GI_ItemDatabase>().GetItem(id);
        return asset != null ? new ItemInstance(asset, 1) : null;
    }
}
