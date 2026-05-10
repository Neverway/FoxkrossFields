//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M.
// 
// Contributors: 
//  Connorses, Errynei, Soulex
//
//====================================================================================================================//

using System;
using Neverway.Framework;
using RivenFramework;
using Unity.VisualScripting;
using UnityEngine;

public class TDPawn_Player : TDPawn
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/
    
    
    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/

    
    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/
    private Vector3 moveDirection;
    private Vector2 lookRotation;
    private float currentMoveSpeed;
    private Vector2 faceDirection;
    private Grid grid;
    
    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/
    private GI_WidgetManager widgetManager;
    private new TDPawnActions action = new TDPawnActions();
    private InputActions.TopDownActions inputActions;
    [SerializeField] private GameObject DeathScreenWidget, InventoryWidget;
    [SerializeField] public Pawn_ItemInventory playerInventory;
    private ApplicationSettings applicationSettings;
    [SerializeField] private Animator animator;
    
    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/
    private void UpdatePauseMenu()
    {
        if (!widgetManager)
        {
            widgetManager = GameInstance.Get<GI_WidgetManager>();
            if (!widgetManager) return;
        }
        isPaused = widgetManager.GetExistingWidget("WB_Pause") || widgetManager.GetExistingWidget("WB_Altertable") || widgetManager.GetExistingWidget(InventoryWidget.name) || widgetManager.GetExistingWidget("WB_Shop");
        
        // Pause Game
        if (inputActions.Menu.WasPressedThisFrame())
        {
            widgetManager.ToggleWidget("WB_Pause");
        }
        if (inputActions.Select.WasPressedThisFrame())
        {
            var widget = widgetManager.GetExistingWidget(InventoryWidget.name); if (widget) Destroy(widget);
            else widgetManager.AddWidget(InventoryWidget);
            
            widget = widgetManager.GetExistingWidget("WB_Altertable"); if (widget) Destroy(widget);
            widget = widgetManager.GetExistingWidget("WB_Shop"); if (widget) Destroy(widget);
        }
        
        // Lock mouse when unpaused, unlock when paused
        if (isPaused)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public new void Awake()
    {
        base.Awake();
        
        // Subscribe to events
        OnPawnDeath += OnDeath;
        
        // Setup inputs
        inputActions = new InputActions().TopDown;
        inputActions.Enable();
        
        // Enable the view camera
        action.EnableViewCamera(this, true);

        currentMoveSpeed = TDCurrentStats.movementSpeed;
    }

    public void Update()
    {
        // Pausing
        UpdatePauseMenu();
        
        
        // Switch item
        if (inputActions.LeftAction.WasPressedThisFrame()) playerInventory.PreviousItem();
        if (inputActions.RightAction.WasPressedThisFrame()) playerInventory.NextItem();
        
        
        if (isPaused || isDead) return;
        UpdateMovement();
        UpdateAttachmentPoint();
        //UpdateRotation();
        
        // Kill bind
        if (Input.GetKeyDown(KeyCode.Delete)) Kill();
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            var saveManager = GameInstance.Get<GI_SaveManager>();
            saveManager.SavePlayer(gameObject.transform.position, playerInventory.items);
            GameInstance.Get<GI_TileChunkManager>().SaveAllDirty();
        }
        
        // Interact 
        if (inputActions.Interact.WasPressedThisFrame())
        {
            if (physObjectAttachmentPoint.attachedObject)
            {
                //action.DropPhysProp(this);
            }
            else
            {
                //action.Interact(this, interactionPrefab, viewPoint.transform);
            }
        }
        
        // Use Item
        if (!playerInventory)
        {
            throw new Exception("playerInventory reference has not been set in the inspector! The inventory should be on one of the child objects under the player prefab, please manually assign it!");
        }
        if (inputActions.Interact.IsPressed())
        {
            // Throw held object, or Item Use Action 0
            if (physObjectAttachmentPoint.attachedObject)
            {
                action.ThrowPhysProp(this);
            }
            else
            {
                playerInventory.ItemUsePrimary();
                //action.ItemUseAction(playerInventory, 0);
            }
        }

        if (inputActions.Action.IsPressed())
        {
            playerInventory.ItemUseSecondary();
        }

        if (inputActions.Interact.WasReleasedThisFrame())
        {
            playerInventory.ItemReleasePrimary();
        }

        if (inputActions.Action.WasReleasedThisFrame())
        {
            playerInventory.ItemReleaseSecondary();
        }
    }

    public void FixedUpdate()
    {
        if (isPaused || isDead) return;
        ApplyMovement();
        //ApplyRotation();
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/

    
    private void UpdateMovement()
    {
        moveDirection = new Vector3(inputActions.Move.ReadValue<Vector2>().x, inputActions.Move.ReadValue<Vector2>().y, 0);
        
        float spd = currentMoveSpeed;
        if (spd > TDCurrentStats.movementSpeed) spd = Mathf.Pow((1.5f * spd), 0.5f) + 0.4f;
        currentMoveSpeed = Mathf.Lerp(spd, currentMoveSpeed, 1f - Mathf.InverseLerp(TDCurrentStats.movementSpeed, TDCurrentStats.maxMovementSpeed, currentMoveSpeed));
        
        animator.SetFloat("walkX", moveDirection.x);
        animator.SetFloat("walkY", moveDirection.y);
        animator.SetBool("walking", moveDirection.x != 0 || moveDirection.y != 0);
        if (animator.GetBool("walking"))
        {
            faceDirection = new Vector2(moveDirection.x, moveDirection.y);
            animator.SetFloat("idleX", moveDirection.x);
            animator.SetFloat("idleY", moveDirection.y);
        }
    }

    private void UpdateAttachmentPoint()
    {
        physObjectAttachmentPoint.transform.localPosition = new Vector3(faceDirection.x*1f, faceDirection.y*1f, 0);
        if (grid.IsUnityNull()) grid = GameInstance.Get<GI_TileDataManager>().tileGrid;
        Vector3Int cell = grid.WorldToCell(physObjectAttachmentPoint.transform.position);
        physObjectAttachmentPoint.transform.position = grid.GetCellCenterLocal(cell);
    }
    
    private void ApplyMovement()
    {
        action.Move(this, moveDirection, currentMoveSpeed);
    }


    private void OnDeath(DamageInfo _damageInfo)
    {
        // Drop held props
        if (physObjectAttachmentPoint)
        {
            if (physObjectAttachmentPoint.attachedObject)
            {
                if (physObjectAttachmentPoint.attachedObject.TryGetComponent(out Object_PhysPickup physPickup))
                {
                    physPickup.ToggleHeld();
                }
            }
        }

        // Remove the HUD
        Destroy(widgetManager.GetExistingWidget("WB_HUD"));
        // Add the respawn HUD
        widgetManager.AddWidget(DeathScreenWidget);

        // Play the death animation
        if (TryGetComponent(out Animator animator)) animator.Play("Death");
    }

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/
    public bool IsCrouched()
    {
        return action.isCrouching;
    }


    #endregion
}
