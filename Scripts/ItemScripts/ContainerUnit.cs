using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class ContainerUnit : Interactable
{
     [Tooltip("Items in container")] public List<ItemCont> items = new List<ItemCont>();
    public bool locked = false;
    [HideInInspector] public ulong containerId;

    public static int cuCount = 0;
    private void Start()
    {
       
        containerId = (ulong)cuCount;
        cuCount++;

        if (!Gamemanager.newGame)
        {
            ItemCont[] tempItems;
            SaveSystem.LoadContainer(containerId, out tempItems);
            items = new List<ItemCont>();

            foreach (ItemCont item in tempItems) items.Add(item); 
        }
    }
    //When gets interacted it opens the container and transfers items to containerscript and displays the container
    public override void Interact(Transform source) 
    {
        if (locked) return;
        if (Inventory.instance.menu.interactable) return;

        base.Interact(source);
        ItemCont[] temp;

        if(!Gamemanager.newGame || source.GetComponent<PlayerController>().devLoadGame)
        {
            SaveSystem.LoadContainer(containerId, out temp);

            if (temp != null)
            {
                items = new List<ItemCont>();
                foreach (ItemCont ite in temp)
                {
                    items.Add(ite);
                }
            }
        }
        
        if(ContainerScript.instance.current == null)
        {
            ContainerScript.instance.OpenStorage(this);
            items.Clear();
        }
    }

    //Basically just adds back the items after closing old name
    public void ChangeItems(ItemInSlot item, int amount)
    {
        if(amount > 0)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].item.item == item.item)
                {
                    items[i].amount += amount;
                    return;
                }
            }
            items.Add(new ItemCont { item = item, amount = amount });
        }
    }

    public void SetRandomItems(AIInfo info)
    {
        items = ItemDropHandler.instance.GetRandomitems(info);
    }

    public void AddItem(ItemCont toAdd)
    {
        items.Add(toAdd);
    }
    public void AddItem(Item item)
    {
        ItemCont temp = new ItemCont();
        ItemInSlot temp2 = new ItemInSlot();
        temp2.item = item;
        temp.item = temp2;
        temp.amount = 1;
        items.Add(temp);
    }

    public void Unlock()
    {
        locked = false;
    }

    public void LoadItems(ItemCont[] itemsIn)
    {
        items = new List<ItemCont>();
        foreach(ItemCont it in itemsIn)
        {
            items.Add(it);
        }
    }

}
//Small container class for each item
[Serializable]
public class ItemCont
{
    public ItemInSlot item;
    [Range(0,1)] public float extraDropChance;
    public int amount;
}