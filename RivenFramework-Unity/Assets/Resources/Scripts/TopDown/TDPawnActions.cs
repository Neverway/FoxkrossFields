//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RivenFramework;

public class TDPawnActions : PawnActions
{
    //=-----------------=
    // Public Variables
    //=-----------------=


    //=-----------------=
    // Private Variables
    //=-----------------=
    private RaycastHit slopeHit;
    public bool isCrouching;
    private GameObject viewCamera;


    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    

    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
    /// <summary>
    /// Make the pawn move, using velocity, in a specified direction
    /// </summary>
    /// <param name="_pawn">A reference to the owning pawn</param>
    /// <param name="_rigidbody">A reference to the owning rigidbody</param>
    /// <param name="_direction">The direction to move in (x-axis is left/right, y-axis is forward/backward, and z-axis is up/down (which is only really used for flying enemies))</param>
    /// <param name="_speed">The speed to move the pawn at (set this to 0 to just use the stats movement speed)</param>
    public void Move(TDPawn _pawn, Vector3 _direction, float _speed=0)
    {
        _pawn.GetComponent<Rigidbody2D>().velocity = _direction * _speed;
    }
    
    /// <summary>
    /// TODO Make the pawn move in a direct path to a specified position
    /// </summary>
    /// <param name="_position"></param>
    public void MoveTo(Vector3 _position)
    {
        
    }
    
    /// <summary>
    /// TODO Make the pawn path-find it's way to a specified position
    /// </summary>
    /// <param name="_position"></param>
    public void MoveToSmart(Vector3 _position)
    {
        
    }
    
    /// <summary>
    /// Make the pawn turn to face a specified amount
    /// </summary>
    /// <param name="_pawn">A reference to the root of the pawn (this is needed to rotate the body to look left and right)</param>
    /// <param name="_viewPoint">A reference to the object that represents the head of the pawn (this is needed to rotate the head to look up and down)</param>
    /// <param name="_direction">The direction to rotate in (x-axis is left/right, y-axis is up/down)</param>
    public void FaceTowardsDirection(TDPawn _pawn, Transform _viewPoint, Vector2 _direction)
    {
        //if(GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn,  _viewPoint, _direction })) return;
        
        _viewPoint.localRotation = Quaternion.Euler(_direction.x, 0, 0); // Rotate the head for up/down
        _pawn.transform.rotation = Quaternion.Euler(0, _direction.y, 0); // Rotate the body for left/right
    }
    
    /// <summary>
    /// Make the pawn face at a specified point
    /// </summary>
    /// <param name="_pawn">A reference to the root of the pawn (this is needed to rotate the body to look left and right)</param>
    /// <param name="_viewPoint">A reference to the object that represents the head of the pawn (this is needed to rotate the head to look up and down)</param>
    /// <param name="_position"></param>
    /// <param name="_speed"></param>
    public void FaceTowardsPosition(TDPawn _pawn, Transform _viewPoint, Vector3 _position, float _speed)
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn, _viewPoint, _position, _speed });
        
        var vectorToTarget = _pawn.transform.position - _position;

        // Rotate the body for left/right
        var bodyLookRotation = Mathf.Atan2(vectorToTarget.x, vectorToTarget.z) * Mathf.Rad2Deg;
        _pawn.transform.rotation = Quaternion.Euler(0, bodyLookRotation+180, 0);
        
        // Rotate the head for up/down
        var headLookRotation = Quaternion.LookRotation(vectorToTarget, _pawn.transform.up).eulerAngles;
        var desiredRotation = new Vector3(-headLookRotation.x, headLookRotation.y + 180, headLookRotation.z);
        _viewPoint.transform.eulerAngles = desiredRotation;
    }
    
    /// <summary>
    /// Make the pawn jump using a force applied to the rigidbody
    /// </summary>
    /// <param name="_pawn">A reference to the pawn to get its jump force & IsOnGround state</param>
    /// <param name="_rigidbody"></param>
    public void Jump(TDPawn _pawn)
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn });
        
        if (IsOnGround(_pawn) is false) return;
        var rigidbody = _pawn.GetComponent<Rigidbody>();
        rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);
        rigidbody.AddForce(Vector3.up * ((TDPawnStats)_pawn.currentStats).jumpForce, ForceMode.Impulse);
    }
    
    /// <summary>
    /// Make the pawn crouch by reducing its capsule collider height (and also trigger Move to change to a crouching movement speed)
    /// </summary>
    /// <param name="_pawn"></param>
    /// <param name="_enable"></param>
    public void Crouch(TDPawn _pawn, bool _enable)
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn, _enable });
        
        if (_enable && isCrouching is false)
        {
            var collider = _pawn.GetComponent<CapsuleCollider>();
            collider.height -= ((TDPawnStats)_pawn.currentStats).crouchDistance;
            collider.center += ((TDPawnStats)_pawn.currentStats).crouchColliderOffset;
            isCrouching = true;
        }
        if (_enable is false && isCrouching && IsHeadClear(_pawn))
        {
            var collider = _pawn.GetComponent<CapsuleCollider>();
            _pawn.transform.position += new Vector3(0, ((TDPawnStats)_pawn.currentStats).crouchDistance, 0);
            collider.height += ((TDPawnStats)_pawn.currentStats).crouchDistance;
            collider.center -= ((TDPawnStats)_pawn.currentStats).crouchColliderOffset;
            isCrouching = false;
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    public void Interact(TDPawn _pawn, GameObject _interactionTrigger, Transform _viewPoint)
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn,  _interactionTrigger, _viewPoint });
        
        var interaction = Object.Instantiate(_interactionTrigger, _viewPoint);
        interaction.transform.GetChild(0).GetComponent<VolumeTriggerInteraction>().owningPawn = _pawn;
        Object.Destroy(interaction,  0.2f);
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="_action"></param>
    public void ItemUseAction(Pawn_Inventory _inventory, int _action = 0, string _mode = "press")
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _inventory, _action, _mode });
        
        var item = _inventory.GetComponentInChildren<Item>(false);
        if (item is null) return;

        switch (_action)
        {
            case 0:
                item.UsePrimary(_mode);
                break;
            case 1:
                item.UseSecondary(_mode);
                break;
            case 2:
                item.UseTertiary(_mode);
                break;
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    public void SwitchItem()
    {
        
    }
    
    public bool IsHeadClear(TDPawn _pawn)
    {
        RaycastHit hit;
        if (Physics.SphereCast(_pawn.transform.position + ((TDPawnStats)_pawn.currentStats).headCheckOffset, ((TDPawnStats)_pawn.currentStats).headCheckRadius, _pawn.transform.up, out hit, ((TDPawnStats)_pawn.currentStats).headCheckDistance, ((TDPawnStats)_pawn.currentStats).groundMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }
        return true;
    }
    
    public bool IsOnGround(TDPawn _pawn)
    {
        // Move the ground check position upwards if the pawn is crouching to account for their change in height
        Vector3 crouchingOffset = new Vector3(0,0,0);
        if (isCrouching) crouchingOffset = new Vector3(0, ((TDPawnStats)_pawn.currentStats).crouchDistance, 0);
        
        return Physics.CheckSphere(_pawn.transform.position - ((TDPawnStats)_pawn.currentStats).groundCheckOffset + crouchingOffset, ((TDPawnStats)_pawn.currentStats).groundCheckRadius, ((TDPawnStats)_pawn.currentStats).groundMask, QueryTriggerInteraction.Ignore);
    }

    public bool IsOnSlope(TDPawn _pawn)
    {
        /*
        This function does not account for crouching offsets. Meaning if a pawn is crouched, the slope detection will likely fail and the pawn will slip off the slope.
        This is a bug, but I'm deciding to keep it in since it's super fun to be able to crouch when falling at a slope to slide down it!
        If this needs to be patched out for any reason, update this function to account for the crouch offset. If you're not sure how to do that, check IsOnGround function above. It correctly accounts for the crouch offset.
        Happy sliding! ~Liz
        //*/
        if (Physics.Raycast(_pawn.transform.position, Vector3.down, out slopeHit, ((TDPawnStats)_pawn.currentStats).slopeCheckDistance, ((TDPawnStats)_pawn.currentStats).groundMask, QueryTriggerInteraction.Ignore))
        {
            return slopeHit.normal != Vector3.up;
        }

        return false;
    }

    public void EnableViewCamera(TDPawn _pawn, bool _setActive)
    {
        if (viewCamera is null)
        {
            // Try to get a view camera
            viewCamera =_pawn.GetComponentInChildren<Camera>(true).gameObject;
            if (viewCamera is null) return;
        }
        
        viewCamera.SetActive(_setActive);
    }

    /// <summary>
    /// Clears and populates the lists of visible pawns
    /// </summary>
    /// <param name="_pawn"></param>
    /// <param name="_distance"></param>
    public void Look(TDPawn _pawn, float _distance)
    {
        // Clear the list of visible pawns
        _pawn.visiblePawns.Clear();
        _pawn.visibleHostiles.Clear();
        _pawn.visibleAllies.Clear();
        foreach (var target in Physics.OverlapSphere(_pawn.transform.position, _distance))
        {
            // Object is pawn
            var targetPawn = target.GetComponent(typeof(TDPawn)) as TDPawn;
            if (targetPawn)
            {
                if (targetPawn.gameObject == _pawn.gameObject) continue;
                // Pawn is not occluded by something
                //if (!Physics.Raycast(_pawn.viewPoint.transform.position, _pawn.transform.position - target.transform.position, 9999, _pawn.currentStats.groundMask))
                //{
                    // Add it to the list of visible pawns
                    _pawn.visiblePawns.Add(targetPawn);
                    // If it's an enemy, add it to the list of visible hostiles
                    if (_pawn.TDCurrentStats.opposedTeams.Contains(((TDPawnStats)targetPawn.currentStats).team))
                    {
                        _pawn.visibleHostiles.Add(targetPawn);
                    }
                    // If it's a friend, add it to the list of visible allies
                    if (((TDPawnStats)_pawn.currentStats).alliedTeams.Contains(((TDPawnStats)targetPawn.currentStats).team))
                    {
                        _pawn.visibleAllies.Add(targetPawn);
                    }
                //}
            }
        }
    }
    
    public void Listen()
    {
        
    }
    
    public void ThrowPhysProp(TDPawn _pawn)
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn });
        
        var attachedObject = _pawn.physObjectAttachmentPoint.attachedObject;
        
        attachedObject.GetComponent<Rigidbody>().AddForce((viewCamera.transform.forward * ((TDPawnStats)_pawn.currentStats).throwForce));
        
        var physPickup = attachedObject.GetComponent<Object_PhysPickup>();
        if (physPickup) physPickup.Drop();
        else attachedObject.GetComponent<Object_PhysPickupAdvanced>().Drop();
    }

    public void DropPhysProp(TDPawn _pawn)
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn });
        
        var attachedObject = _pawn.physObjectAttachmentPoint.attachedObject;
        var physPickup = attachedObject.GetComponent<Object_PhysPickup>();
        if (physPickup) physPickup.Drop();
        else attachedObject.GetComponent<Object_PhysPickupAdvanced>().Drop();
    }

    public TDPawn GetClosest(TDPawn _pawn, List<Pawn> _pawns)
    {
        var closestDistance = 999999f;
        TDPawn closestPawn = null;
        foreach (var target in _pawns)
        {
            var distanceToTarget = Vector3.Distance(_pawn.transform.position, target.transform.position);
            if (distanceToTarget <= closestDistance)
            {
                closestDistance = distanceToTarget;
                closestPawn = ((TDPawn)target);
            }
        }

        return closestPawn;
    }

    public float GetCollectiveAllyCourage(TDPawn _pawn, List<Pawn> _pawns)
    {
        float collectiveAllyCourage = 0;
        foreach (var target in _pawns)
        {
            var distanceToTarget = Vector3.Distance(_pawn.transform.position, target.transform.position);
            if (distanceToTarget <= ((TDPawnStats)_pawn.currentStats).comfortableAllyDistance)
            {
                collectiveAllyCourage += ((TDPawnStats)_pawn.currentStats).courage;
            }
        }
        /*foreach (var VARIABLE in COLLECTION)
        {
            Vector3.Distance(closestAlly.transform.position, _pawn.transform.position) > ((TDS_Stats)_pawn.stats).comfortableAllyDistance
        }*/
        return collectiveAllyCourage;
    }
    
    public void ItemSwapNext(TDPawn _pawn)
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn });
        
        var inventory = _pawn.GetComponentInChildren<Pawn_Inventory>();
        if (inventory is null) return;
        inventory.SwitchNext();
    }

    public void ItemSwapPrevious(TDPawn _pawn)
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn });
        
        var inventory = _pawn.GetComponentInChildren<Pawn_Inventory>();
        if (inventory is null) return;
        inventory.SwitchPreviouse();
    }
}
