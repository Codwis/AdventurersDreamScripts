using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BagSlot : Slot
{
    private BagScript bagScript;

    public override void Awake()
    {
        base.Awake();
        bagScript = GetComponentInChildren<BagScript>();
    }

    public override int TryAddItem(ItemInSlot item, int amount, bool switching = false) //Only adds/Switches item if its a bag
    {
        if(item.item is BagItem bag)
        {
            bagScript.ChangeBag(bag);
            if (bagScript.GetCurrentBag() != bag) //if bag didnt switch then returns
                return amount;

            return base.TryAddItem(item, amount, switching);
        }
        return amount;
    }
}
