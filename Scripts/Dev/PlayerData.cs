using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public StatsData statsData;
    public InventoryData invData;
    public QuestData questData;
    public float[] playerPosition;

    public PlayerData(Vector3 pos, StatsData data1, QuestData data3)
    {
        statsData = data1;
        questData = data3;
        playerPosition = new float[3] { pos.x, pos.y, pos.z };
    }
}

[System.Serializable]
public class StatsData
{
    public float health;
    public float maxHealth;
    public float maxStamina;

    public StatsData(float heal, float max, float stamMax)
    {
        health = heal;
        maxHealth = max;
        maxStamina = stamMax;
    }
}

[System.Serializable]
public class InventoryData
{
    public ItemSaveData[] items;
    public int[] itemNums;
    public int[] amount;
    public InventoryData(ItemSaveData[] playerItems, int[] amounts)
    {
        items = playerItems;
        amount = amounts;
    }
}

[System.Serializable]
public class QuestData
{
    public string[] questsDone;

    public QuestData(string[] parame)
    {
        questsDone = parame;
    }
}

[System.Serializable]
public class ContainerData
{
    public Transform[] containerPos;
    public ContainerItemData[] items;

    public ContainerData(ContainerUnit[] units, bool newGame)
    {
        items = new ContainerItemData[units.Length];

        for(int i = 0; i < units.Length; i++)
        {
            ContainerItemData temp = new ContainerItemData();
            items[i] = temp;

            if (units[i].items == null)
            {
                items[i].items = null;
            }
            else
            {
                items[i].items = units[i].items.ToArray();
            }

            items[i].contId = units[i].containerId;
        }

        containerPos = new Transform[units.Length];

        for(int i = 0; i < units.Length; i++)
        {
            containerPos[i] = units[i].transform;
        }
    }
}

[System.Serializable]
public class ContainerItemData
{
    public ulong contId;
    public ItemCont[] items;
}