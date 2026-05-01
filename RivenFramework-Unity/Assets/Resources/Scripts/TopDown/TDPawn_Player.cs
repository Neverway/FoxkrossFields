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
    
    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/
    private GI_WidgetManager widgetManager;
    private new TDPawnActions action = new TDPawnActions();
    private InputActions.TopDownActions inputActions;
    [SerializeField] private GameObject DeathScreenWidget, InventoryWidget;
    [SerializeField] private Pawn_Inventory playerInventory;
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
        isPaused = widgetManager.GetExistingWidget("WB_Pause");
        
        // Pause Game
        if (inputActions.Menu.WasPressedThisFrame())
        {
            widgetManager.ToggleWidget("WB_Pause");
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
        
        
        if (isPaused || isDead) return;
        UpdateMovement();
        UpdateAttachmentPoint();
        //UpdateRotation();
        
        // Kill bind
        if (Input.GetKeyDown(KeyCode.Delete)) Kill();
        
        // Interact 
        if (inputActions.Interact.WasPressedThisFrame())
        {
            if (physObjectAttachmentPoint.attachedObject)
            {
                action.DropPhysProp(this);
            }
            else
            {
                action.Interact(this, interactionPrefab, viewPoint.transform);
            }
        }
        
        // Switch item
        if (inputActions.LeftAction.WasPressedThisFrame()) action.ItemSwapNext(this);
        if (inputActions.RightAction.WasPressedThisFrame()) action.ItemSwapPrevious(this);
        
        // Use Item
        if (!playerInventory)
        {
            throw new Exception("playerInventory reference has not been set in the inspector! The inventory should be on one of the child objects under the player prefab, please manually assign it!");
        }
        if (inputActions.Interact.WasPressedThisFrame())
        {
            // Throw held object, or Item Use Action 0
            if (physObjectAttachmentPoint.attachedObject)
            {
                action.ThrowPhysProp(this);
            }
            else
            {
                action.ItemUseAction(playerInventory, 0);
            }
        }
        if (inputActions.Action.WasPressedThisFrame()) action.ItemUseAction(playerInventory, 1);
        if (inputActions.Interact.WasReleasedThisFrame()) action.ItemUseAction(playerInventory, 0, "release");
        if (inputActions.Action.WasReleasedThisFrame()) action.ItemUseAction(playerInventory, 1, "release");
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
        physObjectAttachmentPoint.transform.localPosition = new Vector3(faceDirection.x, faceDirection.y, 0);
    }
    
    private void ApplyMovement()
    {
        action.Move(this, moveDirection, currentMoveSpeed);
    }

    /*private void UpdateRotation()
    {
        if (applicationSettings == null) applicationSettings = GameInstance.Get<ApplicationSettings>();
        
        // Get the look speed
        float horizontalLookSpeed = applicationSettings.currentSettingsData.horizontalLookSpeed;
        float verticalLookSpeed = applicationSettings.currentSettingsData.verticalLookSpeed;
        
        // Separate multipliers for mouse and joystick
        float mouseMultiplier = applicationSettings.currentSettingsData.mouseLookSensitivity;
        float joystickMultiplier = applicationSettings.currentSettingsData.joystickLookSensitivity;

        // Determine the input method (mouse or joystick)
        bool isUsingMouse = false;
        if (inputActions.LookAxis.IsInProgress())
        {
            if (inputActions.LookAxis.activeControl.device.name == "Mouse")
            {
                isUsingMouse = true;
            }
        }

        // Apply the appropriate multiplier
        var multiplier = isUsingMouse ? mouseMultiplier : joystickMultiplier;
        
        // Store the rotation values
        lookRotation.x -= inputActions.LookAxis.ReadValue<Vector2>().y * (10 * verticalLookSpeed) * (multiplier/10);
        lookRotation.y += inputActions.LookAxis.ReadValue<Vector2>().x * (10 * horizontalLookSpeed) * (multiplier/10);
        lookRotation.x = Mathf.Clamp(lookRotation.x, -90f, 90f);
    }*/
    /*private void ApplyRotation()
    {
        action.FaceTowardsDirection(this, viewPoint, lookRotation);
    }*/


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
