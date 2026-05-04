using System.Collections.Generic;
using UnityEngine;

public class AltertableData
{
    public int tier = 2;
    public ItemInstance slotA;
    public ItemInstance slotB;
    public AlchemyOperation operation = AlchemyOperation.Fuse;

    public enum AlchemyOperation { Fuse, Imbue, Deconstruct }

    public ItemInstance Preview()
    {
        switch (operation)
        {
            case AlchemyOperation.Fuse:
                return AlchemySystem.Fuse(slotA, slotB);
            case AlchemyOperation.Imbue:
                return AlchemySystem.Imbue(slotA, slotB);
            case AlchemyOperation.Deconstruct:
                var drops = AlchemySystem.Deconstruct(slotA, tier);
                return drops.Count > 0 ? drops[0] : null;
            default: return null;
        }
    }

    public List<ItemInstance> PreviewAll()
    {
        switch (operation)
        {
            case AlchemyOperation.Fuse:
                var fused = AlchemySystem.Fuse(slotA, slotB);
                return fused != null ? new List<ItemInstance> { fused } : new List<ItemInstance>();
            case AlchemyOperation.Imbue:
                var imbued = AlchemySystem.Imbue(slotA, slotB);
                return imbued != null ? new List<ItemInstance> { imbued } : new List<ItemInstance>();
            case AlchemyOperation.Deconstruct:
                return AlchemySystem.Deconstruct(slotA, tier);
            default: return new List<ItemInstance>();
        }
    }

    public List<ItemInstance> Confirm()
    {
        var results = PreviewAll();
        slotA = null;
        slotB = null;
        return results;
    }
}