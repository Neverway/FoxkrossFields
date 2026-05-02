using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "SItem_Tile", menuName = "Neverway/SItem/Tile")]
public class SItem_Tile : ScriptableItem
{
    [Header("Tiles")]
    [HideInInspector] public Tilemap tilemap;
    
    private void PlaceTile(TDPawn_Player _owner)
    {
        var heldTileData = GameInstance.Get<GI_TileDataManager>().GetTileDataFromID(id);
        tilemap = GameInstance.Get<GI_TileDataManager>().GetTilemapFromLayer((int)heldTileData.tileLayer);

        Vector3Int cell = tilemap.WorldToCell(_owner.physObjectAttachmentPoint.transform.position);
        if (tilemap.GetTile(cell) == null)
        {
            GameInstance.Get<GI_TileChunkManager>().RecalculateLeaves(cell);
            tilemap.SetTile(cell, heldTileData.tile);
            GameInstance.Get<GI_TileChunkManager>().MarkTileDirty(cell, (int)heldTileData.tileLayer, id);

            // Consume one from stack
            _owner.playerInventory.TryRemoveItem(_owner.playerInventory.currentItemIndex);
        }
    }
    
    
    public override void OnItemUsePrimary(TDPawn_Player _owner)
    {
        _owner.playerInventory.DefaultPrimaryAction();
    }

    public override void OnItemUseSecondary(TDPawn_Player _owner)
    {
        PlaceTile(_owner);
    }

    public override void OnItemReleasePrimary(TDPawn_Player _owner)
    {
        _owner.playerInventory.DefaultReleasePrimary();
    }

    public override void OnItemReleaseSecondary(TDPawn_Player _owner)
    {
    }
    
    public override void OnDefaultAction(TDPawn_Player _owner)
    {
        PlaceTile(_owner);
    }
}
