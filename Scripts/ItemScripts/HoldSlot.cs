using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldSlot : Slot
{
    public EquipmentType eqNeeded;
    public itemTypes itemTypeNeeded;

    public delegate void itemAddedDelegate(Item item);
    public itemAddedDelegate itemAdded;

    public delegate void itemRemovedDelegate(Item item);
    public itemRemovedDelegate itemRemoved;

    public override int TryAddItem(ItemInSlot item, int amount, bool switching = false)
    {
        if(item.item is Equipment eq)
        {
            if(eq.equipmentType != eqNeeded)
            {
                return amount;
            }
        }
        if(item.item.itemType != itemTypeNeeded)
        {
            return amount;
        }

        itemAdded.Invoke(item.item);
        return base.TryAddItem(item, amount, switching);
    }

    public override void EmptySlot(bool dropItem = false, bool containerClose = false)
    {
        itemRemoved.Invoke(itemInSlot.item);
        base.EmptySlot(dropItem, containerClose);
    }

}

public enum itemTypes { regular, orb }