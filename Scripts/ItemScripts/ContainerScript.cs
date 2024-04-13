using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerScript : MonoBehaviour
{
    public static ContainerScript instance;

    [Tooltip("CanvasGroup for the container ui")] public CanvasGroup cg;
    [NonSerialized] public List<GameObject> slots;
    [NonSerialized] public ContainerUnit current;

    private bool open = false;

    [HideInInspector] public List<ContainerUnit> containerUnitsToSave = new List<ContainerUnit>();
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        slots = GetComponent<BagScript>().useableSlots;
    }

    //Opens the storage with given containerUnit info
    public void OpenStorage(in ContainerUnit cu)
    {
        current = cu;

        if(cu.items.Count > 0) //adds the items to the slots
        {
            for (int i = 0; i < cu.items.Count; i++)
            {
                slots[i].GetComponent<Slot>().TryAddItem(cu.items[i].item, cu.items[i].amount);
            }
        }

        //Opens bags and makes cursor visible and enables ui
        Inventory.instance.OpenBags(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        cu.items.Clear();

        cg.alpha = 1;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        open = true;
    }

    //Close the currentStorage
    public void CloseStorage()
    {
        if (!open) return;

        if(containerUnitsToSave.Contains(current))
        {
            containerUnitsToSave.Remove(current);
        }
        containerUnitsToSave.Add(current);

        //empties each slot
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].GetComponent<Slot>().EmptySlot(containerClose: true);
        }

        //Checks if mouse can be hidden
        if(Inventory.instance.CanDisableMouse())
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        //Disables ui
        current = null;
        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    //Update is old word bascically just adds items to save file
    public void UpdateContainer(ItemInSlot item, int amount)
    {
        current.ChangeItems(item, amount);
        slots = GetComponent<BagScript>().useableSlots;
    }


    private void OnApplicationQuit()
    {
        CloseStorage();
    }
}
