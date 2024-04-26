using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicActivator : Interactable
{
    public CanvasGroup ui;
    public HoldSlot orbSpotSlot;
    public HoldSlot bookSpotSlot;

    public GameObject orbSpot;
    public GameObject bookSpot;

    public AudioClip clipToPlay;
    private void Start()
    {
        orbSpotSlot.itemAdded = PlaceItem;
        bookSpotSlot.itemAdded = PlaceItem;

        orbSpotSlot.itemRemoved = RemoveItem;
        bookSpotSlot.itemRemoved = RemoveItem;
    }
    public override void Interact(Transform source)
    {
        base.Interact(source);
        InteractableUiHandler.instance.ShowUi(ui);
    }

    public void Activate()
    {
        if(orbSpot.activeSelf && bookSpot.activeSelf)
        {
            if(bookSpotSlot.itemInSlot.item is Grimoire grim)
            {
                Gamemanager.instance.globalAudio.PlayOneShot(clipToPlay);

                grim.elementTypes.Add(orbSpotSlot.itemInSlot.item.elementType);

                orbSpotSlot.EmptySlot();
            }
        }
    }

    public void PlaceItem(Item item)
    {
        if(item.itemType == itemTypes.orb)
        {
            orbSpot.SetActive(true);
        }
        else
        {
            bookSpot.SetActive(true);
        }
    }
    public void RemoveItem(Item item)
    {
        if (item.itemType == itemTypes.orb)
        {
            orbSpot.SetActive(false);
        }
        else
        {
            bookSpot.SetActive(false);
        }
    }
}
