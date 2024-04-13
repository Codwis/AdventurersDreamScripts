using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestSetup : MonoBehaviour
{
    public RectTransform taskParent;
    public GameObject taskPrefab;
    public GameObject amountLeftPrefab;

    public Text questNameText;

    public QuestHandler Setup(Quest questToSetup)
    {
        QuestHandler handler = GetComponent<QuestHandler>();
        handler.quest = questToSetup;
        
        foreach(KillTask y in questToSetup.enemiesKilledNeeded)
        {
            KillTask temp = new KillTask
            {
                enemyToKill = y.enemyToKill,
                killAmount = y.killAmount,
            };

            handler.enemiesKilledNeeded.Add(temp);
        }
        foreach (ItemTask y in questToSetup.itemsNeeded)
        {
            ItemTask temp = new ItemTask
            {
                taskItem = y.taskItem,
                itemAmount = y.itemAmount,
                Gather = y.Gather,
                enemyWhoDropsItem = y.enemyWhoDropsItem,
            };
            handler.itemsNeeded.Add(temp);
        }

        questNameText.text = questToSetup.questName;
        foreach(KillTask quest in handler.enemiesKilledNeeded)
        {
            Text temp = Instantiate(taskPrefab, taskParent).GetComponent<Text>();
            temp.text = "";

            Text tempLeft = Instantiate(amountLeftPrefab, temp.transform).GetComponent<Text>();
            tempLeft.text = "0" + "/" + quest.killAmount;
            quest.questAmountText = tempLeft;

            temp.text += "Slay " + quest.killAmount + " " + quest.enemyToKill.name;
            if (quest.killAmount > 1)
                temp.text += "s";
        }

        foreach(ItemTask quest in handler.itemsNeeded)
        {
            Text temp = Instantiate(taskPrefab, taskParent).GetComponent<Text>();
            temp.text = "";

            Text tempLeft = Instantiate(amountLeftPrefab, temp.transform).GetComponent<Text>();
            tempLeft.text = "0" + "/" + quest.itemAmount;
            quest.questAmountText = tempLeft;

            if (quest.Gather)
            {
                temp.text += "Gather " + quest.itemAmount + " " + quest.taskItem.itemName;
            }
            else
            {
                if(quest.enemyWhoDropsItem == null)
                {
                    temp.text += "Find " + quest.itemAmount + " " + quest.taskItem.itemName;
                    if(quest.itemAmount > 1)
                    {
                        temp.text += "s";
                    }
                }
                else
                {
                    temp.text += "Get " + quest.itemAmount + " " + quest.taskItem.itemName + " from " + quest.enemyWhoDropsItem.name + "s";
                }
            }
        }

        foreach (string str in questToSetup.textTasks)
        {
            Text temp = Instantiate(taskPrefab, taskParent).GetComponent<Text>();
            temp.text = str;
        }
        return handler;
    }
}
