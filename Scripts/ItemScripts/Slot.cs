using System;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{

    [NonSerialized] public Inventory inventory;

    public Image itemLogo;

    private Text itemAmountText;
    public ItemInSlot itemInSlot = null;
    private int itemAmount;
    private Sprite ogSprite;
    private LayerMask playerMask = ~LayerMask.GetMask();

    private ContainerScript containerScript;
    public virtual void Awake()
    {
        containerScript = GetComponentInParent<ContainerScript>();
        itemAmountText = GetComponentInChildren<Text>();
        inventory = GetComponentInParent<Inventory>();

        if(itemLogo == null) itemLogo = GetComponent<Image>();
        ogSprite = itemLogo.sprite;
    }

    public virtual int TryAddItem(ItemInSlot item, int amount, bool switching = false) //Returns amount leftover 
    {
        if (itemInSlot == null || itemInSlot.item == null || switching) //if there isint item in the slot just add it and set the variables
        {
            if (item.item == null)
            {
                itemLogo.sprite = ogSprite;
                itemInSlot = null;
                itemAmount = 0;
                itemLogo.color = Color.gray;
            }
            else
            {
                itemLogo.sprite = item.item.logo;
                itemInSlot = item;
                itemAmount = amount;
                itemLogo.color = Color.white;

                if (itemInSlot.itemNum == 0)
                {
                    itemInSlot.itemNum = EquipmentRarityController.rarities.Count;
                }

                EquipmentRarityController.instance.SetRarity(itemInSlot.itemNum, itemInSlot.item.rarityPreset);
                Rarity ra = EquipmentRarityController.instance.GetRarity(itemInSlot.itemNum); //Add color according to rarity
            }
            amount = 0;
            inventory.UpdateInventory();
        }
        else if(itemInSlot.item == item.item && itemAmount < itemInSlot.item.stackSize) //If its the same item in the slot
        {
            //Adds as many until stack is full or runs out
            for (int i = 0; i < amount; i++)
            {
                if(itemInSlot.item.stackSize < itemAmount + 1) // stack filled returns leftovers
                {
                    inventory.HoldItem(itemInSlot, amount - i, inventory.GetCursorSlot());
                    UpdateItemAmountText();
                    return amount - i;
                }
                itemAmount++;
            }
            UpdateItemAmountText();
            return 0;
        }
        else if(itemInSlot.item != item.item) //If its not the item in the slot then switches with it
        {
            inventory.HoldItem(itemInSlot, itemAmount, inventory.GetCursorSlot()); //Stores the slots item information so the switch can happen

            itemLogo.sprite = item.item.logo;
            itemInSlot = item;
            itemAmount = amount;
        }
        UpdateItemAmountText();
        return amount;
    }

    public void UpdateItemAmountText() //Just updates the number of items in the slot
    {
        if (itemAmountText == null)
            return;
        if(itemAmount > 1) //Only show a number if amount is more than 1
        {
            itemAmountText.text = itemAmount.ToString();
        }
        else
        {
            itemAmountText.text = null;
        }
    }

    public void EmptySlot(bool dropItem = false, bool containerClose = false) //Empties the slot surprisisingly useful
    {
        if (itemInSlot == null) return;

        if (this is EquipmentSlot)
        {
            if(itemInSlot.item is Equipment equipment)
            {
                inventory.handler.ChangeEquipment(equipment, itemInSlot.itemNum, true);
            }
        }

        if (dropItem) //If the item is supposed to drop on the ground
        {
            GameObject tempObject;
            Rigidbody rb;
            Transform temp = transform.root;
            Physics.Raycast(temp.position, Vector3.down, out RaycastHit hit, 10, playerMask);

            if (itemInSlot.item.prefab == null) //If the item doesnt have a prefab then make a cube for it
            {
                tempObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tempObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                tempObject.transform.position = hit.point + Vector3.up * 4;
            }
            else
            {
                 tempObject = Instantiate(itemInSlot.item.prefab, hit.point + Vector3.up * 4, itemInSlot.item.prefab.transform.rotation, null);
                tempObject.TryGetComponent<PickupableEvent>(out PickupableEvent even);
                if (even) even.dialogueToShow = null;
            }

            //Enable collider or add one if needed
            if(tempObject.TryGetComponent<Collider>(out Collider col))
            {
                col.enabled = true;
            }
            else
            {
                tempObject.AddComponent<BoxCollider>();
            }

            rb = tempObject.GetComponentInChildren<Rigidbody>();
            if (rb == null) rb = tempObject.GetComponent<Rigidbody>();

            if(rb != null)
            {
                rb.freezeRotation = false;
                rb.constraints = RigidbodyConstraints.None;
                rb.isKinematic = false;
            }
            else
            {
                tempObject.AddComponent<Rigidbody>();
            }

            //Disable weapon things
            if (tempObject.TryGetComponent<MeleeCollisionDetector>(out MeleeCollisionDetector o)) o.enabled = false;
            if (tempObject.TryGetComponent<BowGpxScript>(out BowGpxScript oo)) oo.enabled = false;

            //Makes it possible to pickup item
            Pickupable able = tempObject.AddComponent<Pickupable>();
            able.amount = itemAmount;
            able.itemToPickup = itemInSlot;
        }

        if (containerScript && containerClose)
        {
            containerScript.UpdateContainer(itemInSlot, itemAmount);
            itemInSlot = null;
            itemLogo.sprite = ogSprite;
            itemAmount = 0;
        }
        else
        {
            //Resets the slots values to empty
            itemInSlot = null;
            itemLogo.sprite = ogSprite;
            itemAmount = 0;

            inventory.UpdateInventory();
        }
        UpdateItemAmountText();
    }

    public bool RemoveOne() //Removes one and if there are more items in the slot return true
    {
        itemAmount--;
        if(itemAmount < 1)
        {
            EmptySlot();
            return false;
        }
        return true;
    }
    public bool IsTaken() //returns true if there is no item in the slot
    {
        if(itemInSlot != null)
        {
            if (itemInSlot.item != null)
            {
                return true;
            }
        }
        return false;
    }

    public int GetItemAmount() //returns the item amount
    {
        return itemAmount;
    }
    public ItemInSlot GetItemInSlot() //Returns item in the slot
    {
        return itemInSlot;
    }

    public void OnMouseDownEvent() //When pointer is first pressed down
    {
        if (itemInSlot != null) //if there is an item stores information
        {
            if(itemInSlot.item != null)
            {
                inventory.HoldItem(itemInSlot, itemAmount, this);
            }
        }
    }

    public void OnMouseUpEvent() //When mouse is released 
    {
        inventory.SlotMouseUp();
    }

    public void OnHover() //When mouse is hovering above slot makes it the hover slot
    {
        inventory.SetCurrentHoverSlot(this);
        if(inventory.itemDescHandler != null)
        {
            if(GetItemInSlot() != null)
            {
                inventory.itemDescHandler.ChangeDescription(GetItemInSlot());
            }
            else
            {
                inventory.itemDescHandler.ClearDescription();
            }
        }
    }
}

[Serializable]
public class ItemInSlot
{
    public Item item;
    public int itemNum;
}
