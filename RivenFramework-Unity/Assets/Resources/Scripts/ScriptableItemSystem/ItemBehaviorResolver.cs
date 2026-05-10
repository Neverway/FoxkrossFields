using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class ItemBehaviorResolver
{
    /// <summary>
    /// Returns which TileMaterialTypes a given form can break
    /// Null means the form has no special tile-breaking permission
    /// </summary>
    private static HashSet<TileMaterialType> GetBreakableMaterials(ItemForm form)
    {
        return form switch
        {
            ItemForm.Pickaxe => new HashSet<TileMaterialType> { TileMaterialType.stone, TileMaterialType.metal },
            ItemForm.Hatchet => new HashSet<TileMaterialType> { TileMaterialType.wood },
            _ => null
        };
    }

    /// <summary>
    /// Material types that require a specific tool
    /// </summary>
    private static readonly HashSet<TileMaterialType> ToolRestrictedMaterials = new()
    {
        TileMaterialType.stone,
        TileMaterialType.metal,
        TileMaterialType.wood,
    };
    
    
    /// <summary>
    /// Returns a break-time multiplier based on item material
    /// The lower the value, the faster it breaks
    /// </summary>
    private static float GetMaterialSpeedMultiplier(ItemMaterial material)
    {
        return material switch
        {
            ItemMaterial.None   => 1.00f,
            ItemMaterial.Bone   => 0.90f,
            ItemMaterial.Wood   => 0.80f,
            ItemMaterial.Flint  => 0.70f,
            ItemMaterial.Copper => 0.55f,
            ItemMaterial.Bronze => 0.45f,
            ItemMaterial.Iron   => 0.35f,
            ItemMaterial.Steel  => 0.25f,
            _ => 1.00f
        };
    }
    
    // --------------------------------
    // ITEM ACTIONS
    // --------------------------------
    public static void ExecutePrimary(ScriptableItem item, TDPawn_Player owner)
    {
        var breakable = new HashSet<TileMaterialType>();
        foreach (var form in item.forms)
        {
            var permitted = GetBreakableMaterials(form);
            if (permitted != null) breakable.UnionWith(permitted);
        }
        
        if (breakable.Count > 0)
            BreakTileWithTool(item, owner, breakable);
        else
            DefaultPrimaryAction(item, owner);
    }

    public static void ExecuteSecondary(ScriptableItem item, TDPawn_Player owner)
    {
        
        bool handledByForm = false;
        
        if (DefaultSecondaryAction(item, owner)) return;

        foreach (var form in item.forms)
        {
            switch (form)
            {
                case ItemForm.None:
                    break;
                case ItemForm.Tile:
                    PlaceTile(item, owner);
                    handledByForm = true;
                    break;
                case ItemForm.Seed:
                    PlaceSeed(item, owner);
                    handledByForm = true;
                    break;
                case ItemForm.Consumable:
                    break;
                case ItemForm.Hoe:
                    TillTile(item, owner);
                    handledByForm = true;
                    break;
                case ItemForm.Pickaxe:
                    break;
                case ItemForm.Hatchet:
                    break;
                case ItemForm.Blade:
                    break;
                case ItemForm.Polearm:
                    break;
                case ItemForm.Bludgeon:
                    break;
                case ItemForm.Projectile:
                    break;
            }
        }
        
        if (!handledByForm) DefaultPrimaryAction(item, owner);
    }

    public static void ExecuteReleasePrimary(ScriptableItem item, TDPawn_Player owner)
    {
        
    }

    public static void ExecuteReleaseSecondary(ScriptableItem item, TDPawn_Player owner)
    {
        
    }
    
    // --------------------------------
    // FORM BEHAVIOURS
    // --------------------------------
    public static bool DefaultPrimaryAction(ScriptableItem item, TDPawn_Player owner)
    {
        HashSet<int> unbreakableLayers = new HashSet<int> { 0, 1, 4, 5 };
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
            
            if (ToolRestrictedMaterials.Contains(tileData.tileMaterialType))
            {
                owner.playerInventory.holdTimer = 0f;
                return false;
            }

            BreakTile(item, owner, tilemap, tileDataManager, cell, tileData, layer);
            return true;
        }

        owner.playerInventory.holdTimer = 0f;
        return false;
    }
    public static bool DefaultSecondaryAction(ScriptableItem item, TDPawn_Player owner)
    {
        Vector3 attachPos = owner.physObjectAttachmentPoint.transform.position;
        var tileDataManager = owner.tileDataManager;

        for (int layer = tileDataManager.GetTilemapCount() - 1; layer >= 0; layer--)
        {
            var tilemap = tileDataManager.GetTilemapFromLayer(layer);
            if (tilemap == null) continue;

            Vector3Int cell = tilemap.WorldToCell(attachPos);
            if (tilemap.GetTile(cell) == null) continue;

            var liveObject = GameInstance.Get<GI_TileChunkManager>().GetLiveObject(cell);
            if (liveObject != null)
            {
                var interactable = liveObject.GetComponent<IInteractable>();
                if (interactable == null) return false;
                interactable.OnInteract(owner);
                return true;
            }
        }
        
        var hit = Physics2D.OverlapCircleAll(attachPos, 2f);
        if (hit != null)
        {
            foreach (var collider in hit)
            {
                if (collider != null)
                {
                    var sceneInteractable = collider.GetComponent<IInteractable>();
                    if (sceneInteractable != null)
                    {
                        sceneInteractable.OnInteract(owner);
                        return true;
                    }
                }
            }
        }

        return false;
    }
    public static void DefaultPrimaryRelease(ScriptableItem item, TDPawn_Player owner)
    {
        owner.playerInventory.holdTimer = 0f;
    }
    public static void DefaultSecondaryRelease(ScriptableItem item, TDPawn_Player owner)
    {
        
    }

    private static void BreakTileWithTool(ScriptableItem item, TDPawn_Player owner, HashSet<TileMaterialType> permitted)
    {
        HashSet<int> unbreakableLayers = new HashSet<int> { 0, 1, 4, 5 };
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

            // Only break tiles this tool is allowed to break
            if (!permitted.Contains(tileData.tileMaterialType))
            {
                owner.playerInventory.holdTimer = 0f;
                return;
            }

            BreakTile(item, owner, tilemap, tileDataManager, cell, tileData, layer);
            return;
        }

        // No valid tile found for this tool
        owner.playerInventory.holdTimer = 0f;
    }
    
    private static void BreakTile(ScriptableItem item, TDPawn_Player owner, Tilemap tilemap, GI_TileDataManager tileDataManager, Vector3Int cell, TileData tileData, int layer)
    {
        float materialMultiplier = item != null ? GetMaterialSpeedMultiplier(item.material) : 1.0f;
        float durability = tileData.tileDurability > 0 ? tileData.tileDurability : 1f;
        float breakTime = owner.TDCurrentStats.destroyHoldTime * durability * materialMultiplier;

        Vector3 attachPos = owner.physObjectAttachmentPoint.transform.position;
        FXHelper.SpawnTileBreakParticles(attachPos, tilemap.GetTile(cell), tilemap, cell);

        owner.playerInventory.holdTimer += Time.deltaTime;

        if (owner.playerInventory.holdTimer >= breakTime)
        {
            bool removingTrunk = layer == (int)TileLayers.ObjectsSolid;
            tilemap.SetTile(cell, null);
            GameInstance.Get<GI_TileChunkManager>().MarkTileDirty(cell, layer, null);
            GameInstance.Get<GI_TileChunkManager>().TryDespawnTileObject(cell);

            if (removingTrunk)
                GameInstance.Get<GI_TileChunkManager>().RecalculateLeaves(cell);

            if (tileData.drops != null)
            {
                foreach (var drop in tileData.drops)
                {
                    if (drop.chanceToDrop >= Random.Range(0f, 1f))
                    {
                        foreach (var _item in drop.items)
                            owner.playerInventory.TryAddItem(_item);
                    }
                }
            }

            owner.playerInventory.holdTimer = 0f;
        }
    }
    
    
    private static void PlaceTile(ScriptableItem item, TDPawn_Player owner)
    {
        var tileDataManager = GameInstance.Get<GI_TileDataManager>();
        var heldTileData = tileDataManager.GetTileDataFromID(item.id);
        var tilemap = tileDataManager.GetTilemapFromLayer((int)heldTileData.tileLayer);

        Vector3Int cell = tilemap.WorldToCell(owner.physObjectAttachmentPoint.transform.position);
        if (tilemap.GetTile(cell) == null)
        {
            GameInstance.Get<GI_TileChunkManager>().RecalculateLeaves(cell);
            tilemap.SetTile(cell, heldTileData.tile);
            GameInstance.Get<GI_TileChunkManager>().MarkTileDirty(cell, (int)heldTileData.tileLayer, item.id);
            if (heldTileData.associatedPrefab != null) GameInstance.Get<GI_TileChunkManager>().TrySpawnTileObject(cell, item.id);
            owner.playerInventory.TryRemoveItem(owner.playerInventory.currentItemIndex);
        }
    }

    private static void PlaceSeed(ScriptableItem item, TDPawn_Player owner)
    {
        var tileDataManager = GameInstance.Get<GI_TileDataManager>();
        var heldTileData = tileDataManager.GetTileDataFromID(item.id);
        var tilemap = tileDataManager.GetTilemapFromLayer((int)heldTileData.tileLayer);
        var pathTilemap = tileDataManager.GetTilemapFromLayer((int)TileLayers.Path);

        Vector3Int cell = tilemap.WorldToCell(owner.physObjectAttachmentPoint.transform.position);
        var pathTileData = tileDataManager.GetTileDataFromTileBase(pathTilemap.GetTile(cell));

        if (tilemap.GetTile(cell) == null && pathTileData.tileID == "farmland")
        {
            GameInstance.Get<GI_TileChunkManager>().RecalculateLeaves(cell);
            tilemap.SetTile(cell, heldTileData.tile);
            GameInstance.Get<GI_TileChunkManager>().MarkTileDirty(cell, (int)heldTileData.tileLayer, item.id);
            if (heldTileData.associatedPrefab != null) GameInstance.Get<GI_TileChunkManager>().TrySpawnTileObject(cell, item.id);
            owner.playerInventory.TryRemoveItem(owner.playerInventory.currentItemIndex);
        }
    }

    private static void TillTile(ScriptableItem item, TDPawn_Player owner)
    {
        var tileDataManager = GameInstance.Get<GI_TileDataManager>();
        var heldTileData = tileDataManager.GetTileDataFromID("farmland");
        var tilemap = tileDataManager.GetTilemapFromLayer((int)TileLayers.Path);
        var objectTilemap = tileDataManager.GetTilemapFromLayer((int)TileLayers.Objects);
        var objectSolidTilemap = tileDataManager.GetTilemapFromLayer((int)TileLayers.ObjectsSolid);

        Vector3Int cell = tilemap.WorldToCell(owner.physObjectAttachmentPoint.transform.position);
        var pathTileData = tileDataManager.GetTileDataFromTileBase(tilemap.GetTile(cell));
        // Make sure there are no objects in the way

        float durability = pathTileData.tileDurability > 1 ? pathTileData.tileDurability : 1f;
        float breakTime = owner.TDCurrentStats.destroyHoldTime * 1;
        FXHelper.SpawnTileBreakParticles(owner.physObjectAttachmentPoint.transform.position, heldTileData.tile, tilemap, cell);

        owner.playerInventory.holdTimer += Time.deltaTime;

        if (owner.playerInventory.holdTimer >= breakTime)
        {
            if (objectTilemap.GetTile(cell) == null && objectSolidTilemap.GetTile(cell) == null)
            {
                // Choose if farmland, path or cleared
                if (pathTileData.tileID == "")
                {
                    heldTileData = tileDataManager.GetTileDataFromID("farmland");
                }

                if (pathTileData.tileID == "farmland")
                {
                    heldTileData = tileDataManager.GetTileDataFromID("grass_path");
                }

                if (pathTileData.tileID == "grass_path")
                {
                    tilemap.SetTile(cell, null);
                    GameInstance.Get<GI_TileChunkManager>().MarkTileDirty(cell, (int)TileLayers.Path, null);
                    GameInstance.Get<GI_TileChunkManager>().TryDespawnTileObject(cell);
                    owner.playerInventory.holdTimer = 0f;
                    return;
                }


                GameInstance.Get<GI_TileChunkManager>().RecalculateLeaves(cell);
                tilemap.SetTile(cell, heldTileData.tile);
                GameInstance.Get<GI_TileChunkManager>()
                    .MarkTileDirty(cell, (int)heldTileData.tileLayer, heldTileData.tileID);
                if (heldTileData.associatedPrefab != null)
                    GameInstance.Get<GI_TileChunkManager>().TrySpawnTileObject(cell, heldTileData.tileID);
            }
            
            owner.playerInventory.holdTimer = 0f;
        }
    }

    private static void MineTile(ScriptableItem item, TDPawn_Player owner)
    {
        DefaultPrimaryAction(item, owner);
    }

    private static void ChopTile(ScriptableItem item, TDPawn_Player owner)
    {
        DefaultPrimaryAction(item, owner);
    }

    private static void SwingBlade(ScriptableItem item, TDPawn_Player owner)
    {
        
    }
}
