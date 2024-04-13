using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentSlot : Slot
{
    public EquipmentType equipmentType;

    //Under Work// Gonna work kinda like BagSlot
    public override int TryAddItem(ItemInSlot item, int amount, bool switching = false)
    {
        if(item.item is Equipment equipment)
        {
            if (equipment.equipmentType == equipmentType)
            {
                if(itemInSlot != null)
                {
                    if (itemInSlot.item != null)
                    {
                        inventory.handler.ChangeEquipment((Equipment)itemInSlot.item, itemInSlot.itemNum, true);
                    }
                }
                inventory.handler.ChangeEquipment(equipment, item.itemNum);

                return base.TryAddItem(item, amount, switching);
            }
        }
        return amount;
    }
}
