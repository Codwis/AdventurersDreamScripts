using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickupable : Interactable
{
    public ItemInSlot itemToPickup;
    public int amount;
    public bool pickedUp = false;
    public override void Interact(Transform source)
    {
        if(!pickedUp)
        {
            if (!Inventory.instance.AddItem(itemToPickup, amount))
            {
                return;
            }
            pickedUp = true;
        }

        base.Interact(source);

        float y = source.GetComponentInChildren<Camera>().transform.position.y;

        if(transform.position.y - y < -0.15f )
        {
            source.GetComponent<Animator>().SetFloat("PickUpHeight", -1);
        }
        else if(transform.position.y - y > 0.15f)
        {
            source.GetComponent<Animator>().SetFloat("PickUpHeight", 0);
        }
        else
        {
            source.GetComponent<Animator>().SetFloat("PickUpHeight", 1);
        }
        source.GetComponent<Animator>().SetTrigger("Pickup");
        Destroy(gameObject);
    }
}
