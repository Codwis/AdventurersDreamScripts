using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadClearHandler : MonoBehaviour
{
    public AIController[] aiControllers;
    public bool complete = false;
    public Quest ambushQuest;
    public bool quest = false;

    [NonSerialized] public int dead = 0;

    private void Start()
    {
        List<KillTask> tasks = new List<KillTask>();
        aiControllers = GetComponentsInChildren<AIController>();

        //KillTask task = new KillTask();
        //ambushQuest.questName = "Ambush";
        //task.enemyToKill = aiControllers[0].info;
        //task.killAmount = aiControllers.Length;
        //tasks.Add(task);

        //ambushQuest.enemiesKilledNeeded = tasks;
    }

    private void Update()
    {
        if(dead >= aiControllers.Length)
        {
            complete = true;
            FindObjectOfType<PlayerQuestHandler>().TryProceedingQuest(questToComplete: ambushQuest);
            Destroy(this);
        }
    }


    public void ChangeQuest(Quest quest)
    {
        ambushQuest = quest;
    }
}
