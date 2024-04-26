using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BagScript : MonoBehaviour
{
    [Tooltip("prefab for the slots")] public GameObject slotPrefab;
    [NonSerialized] public List<GameObject> useableSlots = new List<GameObject>();

    [Tooltip("What do the new slot gameobjects parent to")]public Transform slotParent;

    private List<GameObject> slots = new List<GameObject>();
    [HideInInspector] public BagItem currentBag;
    private Inventory inventory;
    private RectTransform rectTransform;
    private CanvasGroup group;

    [NonSerialized] public BagSlot bagSlot;
    private void Start()
    {
        bagSlot = GetComponentInParent<BagSlot>();
        inventory = Inventory.instance;

        rectTransform = GetComponent<RectTransform>();
        group = GetComponent<CanvasGroup>();

        Slot[] sl = GetComponentsInChildren<Slot>();
        if(sl.Length > 0)
        {
            foreach(Slot slot in sl)
            {
                useableSlots.Add(slot.gameObject);
            }
        }
    }


    public void ChangeBag(BagItem newBag) // Changes bag to the new given one
    {

        if(currentBag == null) // If there isint a bag add the slots
        {
            AddSlots(newBag.slotAmount);
        }
        else if(newBag.slotAmount >= currentBag.slotAmount) //If there is a bag and new bag has more or equal slots adds amount of slots needed
        {
            int newSlotAmount = newBag.slotAmount - currentBag.slotAmount;
            AddSlots(newSlotAmount);
        }

        //If new bag has less slots than current
        else if(newBag.slotAmount < currentBag.slotAmount)
        {
            //Update to see if there are enough useable slots
            UpdateUseableSlots();
            int slotAmount = currentBag.slotAmount - newBag.slotAmount;

            //If there arent enough useable slots for how many slots the new bag takes away
            if (useableSlots.Count < slotAmount)
            {
                return;
            }

            //Removes enough amount of slots to suit new bags slotamount
            for (int i = 0; i < slotAmount; i++)
            {
                slots.Remove(useableSlots[useableSlots.Count - 1]);
                Destroy(useableSlots[useableSlots.Count - 1].gameObject);
                inventory.UpdateInventory();
            }
            
        }


        if (inventory.IsInventoryOpen()) //If inventory is open then keep it open when switching bags
        {
            OpenCloseBag(true);
        }
        else
        {
            OpenCloseBag(false);
        }

        currentBag = newBag;
    }

    
    private void AddSlots(int amount) //adds given amount of slots
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject temp = Instantiate(slotPrefab, slotParent);
            slots.Add(temp);
            inventory.UpdateInventory();
        }
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            gameObject.SetActive(false);
        }

    }


    public void UpdateUseableSlots() //gets and sorts useable slots
    {
        useableSlots.Clear();
        //Goes through each slot
        foreach (GameObject slotGO in slots)
        {
            //Gets the slot component and checks if its taken
            Slot slot = slotGO.GetComponent<Slot>();
            if (!slot.IsTaken())
            {
                useableSlots.Add(slotGO);
            }
            else
            {
                inventory.AddUsedSlot(slot);
            }
        }
    }


    public void OpenCloseBag(bool open) //Closes and opens the bag doesnt open if no slots
    {
        if(slots.Count > 0)
        {
            if (open)
            {
                group.alpha = 1;
                group.interactable = open;
                group.blocksRaycasts = open;
            }
            else
            {
                group.alpha = 0;
                group.interactable = false;
                group.blocksRaycasts = false;
            }
        }
        else
        {
            group.alpha = 0;
            group.interactable = false;
            group.blocksRaycasts = false;
        }
    }

    public BagItem GetCurrentBag() //returns currentBag to avoid using public on variable
    {
        return currentBag;
    }

    private Vector3 offset;
    public void GetPointerDown() //When pointer clicks down it gets offset to the mouse
    {
        offset = rectTransform.position - Input.mousePosition;
    }
    public void DragBag() //Moves the bag when gameObject is dragged around
    {
        //Calculates the proper spot and wont snap onto the mouse because offset
        Vector3 pos = Input.mousePosition - rectTransform.position + offset;
        rectTransform.position += pos;

        //Clamps Bag UI withing the screen
        rectTransform.position = new Vector3(Mathf.Clamp(rectTransform.position.x, 0, Screen.width), Mathf.Clamp(rectTransform.position.y, rectTransform.rect.height / 3, Screen.height + rectTransform.rect.height / 3));
    }
}
