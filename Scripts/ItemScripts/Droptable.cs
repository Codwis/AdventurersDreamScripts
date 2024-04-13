using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Droptable", menuName = "AIdroptable")]
public class Droptable : ScriptableObject
{
    public Dictionary<AIInfo, ItemCont[]> itemsAi = new Dictionary<AIInfo, ItemCont[]>();
    public List<AiDropItems> dropableItems = new List<AiDropItems>();

    public void SetupItems()
    {
        foreach(AiDropItems ai in dropableItems)
        {
            if(itemsAi.ContainsKey(ai.aiInfo))
            {
                itemsAi[ai.aiInfo] = ai.ItemsPossibleToDrop;

            }
            else
            {
                itemsAi.Add(ai.aiInfo, ai.ItemsPossibleToDrop);
            }
        }
    }

    public List<ItemCont> GetRandomItems(float odd, AIInfo info)
    {
        List<ItemCont> items = new List<ItemCont>();
        ItemCont item;
        if(itemsAi.ContainsKey(info))
        {
            foreach (ItemCont i in itemsAi[info])
            {
                for(int ii = 1; ii <= i.amount; ii++)
                {
                    if ((odd / ii) <= i.item.item.dropChance + i.extraDropChance)
                    {
                        item = new ItemCont();
                        item.item = i.item;
                        item.amount = 1;
                        items.Add(item);
                    }
                }
            }
        }
        else
        {
            Debug.Log("Add " + info.name + " To droptable");
        }



        return items;
    }
}

[Serializable]
public struct AiDropItems
{
    public AIInfo aiInfo;
    public ItemCont[] ItemsPossibleToDrop;
}
