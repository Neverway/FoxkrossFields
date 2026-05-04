using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WB_Altertable : MonoBehaviour
{
    private AltertableData session;
    private int currentOperation = 0;
    public ItemInstance itemA, itemB, itemEquivalent;
    public Image itemAIcon, itemBIcon, itemEquivalentIcon, itemOperatorIcon, otherItemOperatorIcon;
    public Sprite fuseSprite, imbueSprite, deconstructSprite;
    public TMP_Text equivalentItemDescription, equivalentItemName;

    public void SetSession(AltertableData _session)
    {
        session = _session;
    }
    
    public void OnEnable()
    {
        SelectOperator(0);
    }

    public void CloseWidget()
    {
        Destroy(gameObject);
    }

    public void SelectOperator(int operation)
    {
        currentOperation = operation;
        switch (operation)
        {
            // Fuse
            case 0:
                itemOperatorIcon.sprite = fuseSprite;
                otherItemOperatorIcon.sprite = fuseSprite;
                break;
            // Imbue
            case 1:
                itemOperatorIcon.sprite = imbueSprite;
                otherItemOperatorIcon.sprite = imbueSprite;
                break;
            // Deconstruct
            case 2:
                itemOperatorIcon.sprite = deconstructSprite;
                otherItemOperatorIcon.sprite = deconstructSprite;
                break;
        }

        PreviewResult();
    }

    public void PutCurrentItemIntoSlot(int slot)
    {
        if (slot == 0)
        {
            itemA = FindObjectOfType<Pawn_ItemInventory>().GetCurrentItem();
            if (itemA != null)
            {
                itemAIcon.sprite = itemA.icon;
                itemAIcon.enabled = true;
            }
            else
            {
                itemAIcon.enabled = false;
            }
        }
        else
        {
            itemB = FindObjectOfType<Pawn_ItemInventory>().GetCurrentItem();
            if (itemB != null)
            {
                itemBIcon.sprite = itemB.icon;
                itemBIcon.enabled = true;
            }
            else
            {
                itemBIcon.enabled = false;
            }
        }
        PreviewResult();
    }

    public void PreviewResult()
    {
        if (session == null) return;
        session.slotA = itemA;
        session.slotB = itemB;
        session.operation = (AltertableData.AlchemyOperation)currentOperation;
        
        var results = session.PreviewAll();
        if (results.Count == 0)
        {
            itemEquivalentIcon.enabled = false;
            equivalentItemName.text = "";
            equivalentItemDescription.text = "";
            return;
        }
        
        itemEquivalent = results[0];
        itemEquivalentIcon.sprite = itemEquivalent.icon;
        itemEquivalentIcon.enabled = itemEquivalent.icon != null;
        equivalentItemName.text = itemEquivalent.displayName;
        equivalentItemDescription.text = itemEquivalent.asset.description;
    }
    
    public void ConfirmOperation()
    {
        if (session == null || itemEquivalent == null) return;

        var inventory = FindObjectOfType<Pawn_ItemInventory>();

        // Consume inputs
        if (itemA != null) inventory.TryRemoveItemInstance(itemA);
        if (itemB != null && currentOperation != (int)AltertableData.AlchemyOperation.Deconstruct) inventory.TryRemoveItemInstance(itemB);

        // Give results
        var results = session.Confirm();
        foreach (var result in results) inventory.TryAddItem(result);

        // Clear slots
        itemA = itemB = itemEquivalent = null;
        itemAIcon.enabled = false;
        itemBIcon.enabled = false;
        itemEquivalentIcon.enabled = false;
        equivalentItemName.text = "";
        equivalentItemDescription.text = "";
    }
}
