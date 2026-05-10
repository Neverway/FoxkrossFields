//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M.
// 
// Contributors: 
//  Connorses, Errynei, Soulex
//
//====================================================================================================================//

using System;
using RivenFramework;
using UnityEngine;

/// <summary>
///  This is a Level Blueprint (LB) script, it is attached to the WorldSettings
///  object in a scene.
///  This LB makes the HUD widget appear on game maps.
/// </summary>
public class LB_World : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    
    
    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/

    
    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/

    
    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    private GI_WidgetManager widgetManager;
    // TODO: This may be better changed from GameObject to reference a parent WB_HUD class
    [Tooltip("A reference to the HUD widget prefab to draw to the UI")]
    [SerializeField] private GameObject HUDWidgetPrefab;

    private TDPawn_Player player;
    private bool hasLoaded;
    
    #endregion
    
    
    #region=======================================( Functions )=======================================================//
    
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
    }

    private void Update()
    {
        if (hasLoaded) return;
        if (!player)
        {
            player = FindObjectOfType<TDPawn_Player>();
            return;
        }

        var saveManager = GameInstance.Get<GI_SaveManager>();
        if (saveManager.HasPlayerSave())
        {
            player.transform.position = (Vector3)saveManager.LoadPlayerPosition();
            player.playerInventory.items = saveManager.LoadPlayerInventory(FindObjectOfType<Pawn_ItemInventory>().inventorySize);
            player.playerInventory.gold = saveManager.LoadPlayerGold();
        }
        widgetManager = FindObjectOfType<GI_WidgetManager>();
        widgetManager.AddWidget(HUDWidgetPrefab);
        GameInstance.Get<GI_TileChunkManager>().OnSaveFileLoaded();

        hasLoaded = true;
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    
    
    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    
    
    #endregion
    
}