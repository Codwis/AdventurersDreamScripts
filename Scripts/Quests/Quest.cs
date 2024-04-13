using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Quest", menuName = "Quests/Basic Quest")]
public class Quest : ScriptableObject
{
    [Tooltip("Name of the quest")] public string questName;

    [Tooltip("Text to say when quest is started")] public Dialogue[] questDialogue;
    [Tooltip("What to say when quest is not done")] public string questNotDoneDialogue;
    [Tooltip("What to say when quest gets completed")] public string questCompleteDialogue;
    [Tooltip("What to say after quest is like after thank you")] public string afterQuestText;

    [Tooltip("What to items to reward player with")]public RewardItem[] rewardItems;
    [Tooltip("What sequel quest should open after this leave empty if complete")] public Quest nextStep;

    public List<string> textTasks = new List<string>();
    [Tooltip("What enemies needed to be killed. can be left empty")] public List<KillTask> enemiesKilledNeeded = new List<KillTask>();
    [Tooltip("What items needed to complete this quest. can be left empty")]public List<ItemTask> itemsNeeded = new List<ItemTask>();
    public bool deniable;
}

[System.Serializable]
public class KillTask //Task where player has to kill something
{
    [Tooltip("Enemy that needs to be killed")] public AIInfo enemyToKill;
    [Tooltip("How many of that enemy needs to be killed")] public int killAmount; 

    [NonSerialized] public Text questAmountText;
    [NonSerialized] public int amount;
}

[System.Serializable]
public class ItemTask // Task where player has to have certain item
{
    [Tooltip("Item that player needs")]public Item taskItem;
    [Tooltip("Amount of that item required")] public int itemAmount;
    [Tooltip("Do you have to gather it")] public bool Gather;
    [Tooltip("enemy info if an enemy drops the item")] public AIInfo enemyWhoDropsItem;

    [NonSerialized] public Text questAmountText;
    [NonSerialized] public int amount;
}

[System.Serializable]
public class RewardItem //Class for the list so can drop right amounts
{
    [Tooltip("Item to give to player")] public ItemInSlot item;
    [Tooltip("amount of the item to give to player")] public int amount;
}