using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestHandler : MonoBehaviour
{
    // ADD COMMENTS

    [Tooltip("Quest thats in progress")] public Quest quest;
    [NonSerialized] public List<KillTask> enemiesKilledNeeded = new List<KillTask>();
    [NonSerialized] public List<ItemTask> itemsNeeded = new List<ItemTask>();

    private Inventory inventory;

    private void Start()
    {
        //Get the inventory
        inventory = transform.root.GetComponentInChildren<Inventory>();
    }
    public bool ProgressEnemy(AIInfo info = null) //Tries to Progress
    {
        foreach (KillTask enemy in enemiesKilledNeeded)
        {
            if (enemy.enemyToKill == info)
            {
                enemy.amount += 1;
                enemy.questAmountText.text = enemy.amount + "/" + enemy.killAmount;
                if (enemy.amount >= enemy.killAmount)
                {
                    enemiesKilledNeeded.Remove(enemy);
                    return true;
                }
            }
        }
        return false;
    }

    public bool ProgressItem(Item item)
    {
        foreach(ItemTask itemQuest in itemsNeeded)
        {
            if(itemQuest.taskItem == item)
            {
                int amount = inventory.FindItemAmount(item);
                itemQuest.questAmountText.text = amount + "/" + itemQuest.itemAmount;
            }
        }
        return false;
    }

    public bool CheckQuestDone()
    {
        if(enemiesKilledNeeded.Count < 1)
        {
            foreach(ItemTask it in itemsNeeded)
            {
                if(inventory.FindItemAmount(it.taskItem) < it.itemAmount)
                {
                    return false;
                }
            }

            for(int i = 0; i < itemsNeeded.Count; i++)
            {
                inventory.RemoveItems(itemsNeeded[i].taskItem, itemsNeeded[i].itemAmount, true);
            }
        }
        else
        {
            return false;
        }
        return true;
    }
}
