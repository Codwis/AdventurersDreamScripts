using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableSlot : Slot
{
    public override int TryAddItem(ItemInSlot item, int amount, bool switching = false)
    {
        if(item.item is Consumeable)
        {
            item.item.Use(GetComponentInParent<Stats>());
            amount -= 1;
        }
        
        return amount;
    }
}
