using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

[System.Serializable]
public class Inventory : MonoBehaviour
{
    public CanvasGroup menu;
    public Image cursorItemImage;
    public EquipmentHandler handler;

    [HideInInspector] public BagScript[] bags;
    private List<Slot> useableSlots = new List<Slot>();
    [HideInInspector] public List<Slot> usedSlots = new List<Slot>();

    [HideInInspector] public List<ItemInSlot> itemsToSave = new List<ItemInSlot>();
    private ItemInSlot cursorHoldingItem;
    private int cursorAmountHolding;
    private Slot cursorTakenSlot;
    private Slot currentHoverSlot;
    private bool bagsOpen = false;
    private bool equipmentOpen = false;
    private EquipmentMenu characterEquipmentMenu;
    [NonSerialized] public PlayerQuestHandler questHandler;

    [NonSerialized] public ItemDescriptionHandler itemDescHandler;

    private PlayerController playerController;

    private bool allowUi = true;
    public Settings settings;

    public static Inventory instance;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        BagScript[] t = GetComponentsInChildren<BagScript>();
        bags = new BagScript[t.Length - 1];
        int count = 0;
        for (int i = 0; i < t.Length; i++)
        {
            if (t[i].GetComponent<ContainerScript>()) continue;
            bags[count] = t[i];
            count++;
        }

        playerController = GetComponentInParent<PlayerController>();
        bags = GetComponentsInChildren<BagScript>();
        characterEquipmentMenu = GetComponentInChildren<EquipmentMenu>();
        questHandler = GetComponentInChildren<PlayerQuestHandler>();
    }


    private void Update()
    {
        if (settings == null) return;
        if (Input.GetKeyDown((KeyCode)PlayerPrefs.GetInt("Menu")))
        {
            if(menu.interactable)
            {
                playerController.Pause(true);
                AllowUi(true);
                menu.interactable = false;
                menu.alpha = 0;
                menu.blocksRaycasts = false;

                if (!equipmentOpen && !bagsOpen) //if either menu is open then hide cursor
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else //else keep it shown
                {
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = true;
                }
            }
            else
            {
                AllowUi(false);
                playerController.Pause(false);
                ContainerScript.instance.CloseStorage();

                menu.interactable = true;
                menu.alpha = 1;
                menu.blocksRaycasts = true;

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }


        }
        if (Input.GetKeyDown((KeyCode)PlayerPrefs.GetInt("Bags")) && allowUi) //B is pressed it tries opening each bag
        {
            if (bags.Length < 1) return;
            bagsOpen = !bagsOpen;

            if (!equipmentOpen && !bagsOpen) //if either menu is open then hide cursor
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else //else keep it shown
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }

            OpenBags(bagsOpen);
        }
        if (Input.GetKeyDown((KeyCode)PlayerPrefs.GetInt("Equipment")) && allowUi) //Same like bags but for equipment
        {
            equipmentOpen = !equipmentOpen;
            if(!equipmentOpen && !bagsOpen)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }

            characterEquipmentMenu.CloseOpen(equipmentOpen);
        }

        if(cursorItemImage != null)
        {
            cursorItemImage.transform.position = Input.mousePosition;
        }
    }
    
    public bool CanFitQuestRewards(Quest quest) //Checks if the quest rewards can fit in the inventory 
    {
        int i = 0;
        foreach(RewardItem reward in quest.rewardItems) //Goes through all the rewards
        {
            for(int ii = reward.amount; ii >= reward.item.item.stackSize; ii--) // if it gives like two swords dont fit in same stack it calculates it 
            {
                i++;
            }
        }
        
        foreach(ItemTask itemTask in quest.itemsNeeded) //Goes through all the items needed
        {
            for (int ii = itemTask.itemAmount; ii >= itemTask.taskItem.stackSize; ii--) //For all the new possible free slots remove slot
            {
                i--;
            }
        }
        if(i > useableSlots.Count) //if not enough space even after giving quest items then cant return quest
        {
            return false;
        }
        return true;
    }

    public bool IsInventoryOpen() //returns state of the inventroy
    {
        return bagsOpen;
    }


    //Remember To add here when adding new pages like skills hm hm hm hm hm hm hm hm
    public bool CanDisableMouse()
    {
        if(!bagsOpen)
        {
            return !equipmentOpen;
        }
        else
        {
            return false;
        }
    }

    public void AllowUi(bool allow) //Disable opening Ui
    {
        allowUi = allow;
        if (!allow)
        {
            CloseUi();
        }
    }
    public void CloseUi() //Goes through equipment menu and the bags disables them
    {
        characterEquipmentMenu.CloseOpen(false);

        if (bags[0].GetCurrentBag() != null)
        {
            foreach (BagScript bag in bags)
            {
                bag.OpenCloseBag(false);
            }
        }
        itemDescHandler.cg.alpha = 0;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OpenBags(bool open)
    {
        if (bags[0].GetCurrentBag() != null)
        {
            bagsOpen = open;
            foreach (BagScript bag in bags)
            {
                bag.OpenCloseBag(open);
            }
        }
        if(!bagsOpen)
        {
            itemDescHandler.cg.alpha = 0;
        }
        else
        {
            itemDescHandler.cg.alpha = 1;
        }
    }
    public bool AddItem(ItemInSlot item, int amount) //Adds the given Item and amount of the item to inventory
    {
        
        if (item.item is BagItem newBag) //If its a bag and there is a bagscript without a bag it puts it there
        {
            foreach (BagScript bag in bags)
            {
                if (!bag.GetCurrentBag())
                {
                    if (bag.GetComponentInParent<BagSlot>())
                    {
                        bag.GetComponentInParent<BagSlot>().TryAddItem(item, amount);
                        itemsToSave.Add(item);
                        return true;
                    }
                }
            }
        }

        foreach(Slot slot in usedSlots) //Checks if there is already that item in the inventory
        {
            if(slot.GetItemInSlot().item == item.item)
            {
                if(slot.GetItemAmount() < item.item.stackSize) //If there is space in the stack then add it to it
                {
                    amount = slot.TryAddItem(item, amount);
                    if(amount <= 0) //only returns if it added the entire amount
                    {
                        questHandler.TryProceedingQuest(null, item.item);
                        itemsToSave.Add(item);
                        return true;
                    }
                }
            }
        }
        foreach(Slot slot in useableSlots) //Last resort adds it to the next possible useable slot
        {
            slot.TryAddItem(item, amount);
            questHandler.TryProceedingQuest(null, item.item);
            itemsToSave.Add(item);
            return true;
        }
        return false;
    }

    public void UpdateInventory() //Goes through all the bags and sorts and adds useable slots
    {
        useableSlots.Clear();

        foreach(BagScript bag in bags) 
        {
            if (bag.GetComponent<ContainerScript>()) continue;

            bag.UpdateUseableSlots();
            foreach(GameObject slotObject in bag.useableSlots)
            {
                Slot temp = slotObject.GetComponent<Slot>();
                if (usedSlots.Contains(temp)) // if usedslots contain the slot remove it from there 
                {
                    usedSlots.Remove(temp);
                }
                if(useableSlots.Contains(temp))
                {
                    continue;
                }
                useableSlots.Add(temp);
            }
        }
    }


    public void HoldItem(ItemInSlot item, int amount, Slot slot) //Stores the information from the slot pointer was pressed on
    {
        if(item != null)
        {
            if(item.item != null)
            {
                cursorItemImage.sprite = item.item.logo;
                cursorItemImage.color = Color.white;
            }
        }
        else
        {
            cursorItemImage.color = Color.clear;
        }

        cursorAmountHolding = amount;
        cursorHoldingItem = item;
        cursorTakenSlot = slot;
    }

    //Functions to retrieve stored information
    public ItemInSlot GetCursorItem()
    {
        return cursorHoldingItem;
    }
    public int GetCursorAmount()
    {
        return cursorAmountHolding;
    }
    public Slot GetCursorSlot()
    {
        return cursorTakenSlot;
    }

    public EquipmentMenu GetEquipmentMenu()
    {
        return characterEquipmentMenu;
    }

    public int FindItemAmount(Item itemToFind) //Finds how many of the said item are in the inventory
    {
        int returnVal = 0;
        foreach(Slot slot in usedSlots)
        {
            if(slot.GetItemInSlot().item == itemToFind)
            {
                returnVal += slot.GetItemAmount();
            }
        }
        return returnVal;
    }

    public void RemoveItems(Item item, int amountToRemove, bool questRemove) //Removes given item and amount of the item from inventory
    {
        if (!questRemove && !item.dropable) return;
        ItemInSlot temp = new ItemInSlot();

        for (int i = 0; i < amountToRemove; i++) //for each to remove
        {
            for(int ii = 0; ii < usedSlots.Count; ii++) //Goes through all the used slots
            {
                if (usedSlots[ii].GetItemInSlot().item == item) //If slot has the item
                {
                    while (usedSlots[ii].RemoveOne()) //removes items from the slot one by one
                    {
                        usedSlots[ii].UpdateItemAmountText();
                        if(i < amountToRemove)
                        {
                            break;
                        }
                        i++;
                    }

                    i++;

                    if(i >= amountToRemove) //breaks out of the loop
                    {
                        break;
                    }

                    UpdateInventory();
                    ii--;
                }
            }
        }

        UpdateInventory();
    }

    public void EmptyCertainSlot(Slot slotToEmpty) //Empties given slot
    {
        slotToEmpty.EmptySlot(true);
    }

    public void SlotMouseUp() // When mouse is released
    {
        cursorItemImage.color = Color.clear;
        if (cursorHoldingItem != null && !EventSystem.current.IsPointerOverGameObject())
        {
            if (cursorTakenSlot is not BagSlot)
            {
                cursorHoldingItem = null;
                EmptyCertainSlot(cursorTakenSlot);
                return;
            }
        }

        if(cursorHoldingItem != null)
        {
            questHandler.TryProceedingQuest(item: cursorHoldingItem.item);
        }


        if (cursorHoldingItem == null || currentHoverSlot == null) // if there is information stored return
        {
            cursorHoldingItem = null;
            return;
        }
        if (currentHoverSlot == cursorTakenSlot)
            return;

        if(currentHoverSlot is ConsumableSlot)
        {
            if(currentHoverSlot.TryAddItem(cursorTakenSlot.itemInSlot, cursorTakenSlot.GetItemAmount()) < cursorTakenSlot.GetItemAmount())
            {
                cursorTakenSlot.RemoveOne();
                cursorTakenSlot.UpdateItemAmountText();
            }

        }

        //If the slot the mouse is above of is not empty then swamps the items
        else if (currentHoverSlot.TryAddItem(cursorHoldingItem, cursorAmountHolding) > 0)
        {
            cursorTakenSlot.TryAddItem(cursorHoldingItem, cursorAmountHolding, true);
        }

        else // Otherwise empties the slot where items were taken
        {
            if (cursorTakenSlot is BagSlot) // if its a bag it cannot be taken off without switching
            {
                currentHoverSlot.EmptySlot();
                cursorTakenSlot.TryAddItem(cursorHoldingItem, cursorAmountHolding);
                return;
            }
            cursorTakenSlot.EmptySlot();
        }

        //Clears the information
        currentHoverSlot = null;
        HoldItem(null, 0, null);
        cursorItemImage.color = Color.clear;
    }
    public void SetCurrentHoverSlot(Slot slot) //Gets called when pointer is above of slot
    {
        currentHoverSlot = slot;
    }

    public void AddUsedSlot(Slot slot) // adds given slot to the used list
    {
        if (!usedSlots.Contains(slot)) // if it already exists in the list just skip
        {
            usedSlots.Add(slot);
            UpdateInventory();
        }
    }

    public void SetDescriptionHandler(ItemDescriptionHandler han)
    {
        itemDescHandler = han;
    }

    public void SetSettings(Settings set)
    {
        settings = set;
    }

    public void EmptyInventory()
    {
        foreach(Slot slot in usedSlots)
        {
            slot.EmptySlot(false);
        }
    }

    public void LoadData()
    {
        InventoryData data = SaveSystem.LoadInventory();
        int count = 0;

        if (data == null) return;

        bool no = false;

        EquipmentSlot[] t = GetComponentsInChildren<EquipmentSlot>();
        foreach (ItemInSlot sl in data.items)
        {
            if(sl.item is Equipment eq)
            {
                if(eq.equipmentType != EquipmentType.bag)
                {
                    foreach (EquipmentSlot slot in t)
                    {
                        if (slot.equipmentType == eq.equipmentType && slot.itemInSlot == null)
                        {
                            slot.TryAddItem(sl, 1);
                            no = true;
                            break;
                        }
                    }
                    if(!no)
                    {
                        AddItem(sl, 1);
                    }
                    no = false;

                    count++;
                    continue;
                }

            }
            AddItem(sl, data.amount[count]);
            count++;
        }
    }

    public void ClosePauseMenu()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        menu.alpha = 0;
        menu.interactable = false;
        menu.blocksRaycasts = false;
    }
}
