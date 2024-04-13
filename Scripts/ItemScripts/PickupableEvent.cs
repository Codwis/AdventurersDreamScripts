using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupableEvent : Pickupable
{
    public GameObject[] activateObjects;
    public Dialogue[] dialogueToShow;
    public bool note = false;
    public override void Interact(Transform source)
    {
        if(note)
        {
            if (dialogueToShow != null)
            {
                source.GetComponentInChildren<DialogueHandler>().DisplayText("???", dialogueToShow);

                if (interactableSound != null)
                {
                    if (!Gamemanager.instance.globalAudio.isPlaying)
                    {
                        Gamemanager.instance.globalAudio.PlayOneShot(interactableSound);
                    }
                }
                return;
            }
        }

        if(itemToPickup != null)
        {
            if (!Inventory.instance.AddItem(itemToPickup, amount))
            {
                return;
            }
        }

        pickedUp = true;
        base.Interact(source);

        if(activateObjects != null)
        {
            for (int i = 0; i < activateObjects.Length; i++)
            {
                Instantiate(activateObjects[i]);
            }
        }
        if(dialogueToShow != null)
        {
            source.GetComponentInChildren<DialogueHandler>().DisplayText("You",dialogueToShow );
        }
    }
}
