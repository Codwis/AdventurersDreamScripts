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
    public ItemInSlot[] items;
    public int[] itemNums;
    public int[] amount;
    public InventoryData(ItemInSlot[] playerItems, int[] amounts)
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
    public ContainerUnit[] cu;

    public ContainerData(ContainerUnit[] units)
    {

        cu = new ContainerUnit[units.Length];
        for(int i = 0; i < units.Length; i++)
        {
            cu[i] = units[i];
        }
        containerPos = new Transform[cu.Length];

        for(int i = 0; i < cu.Length; i++)
        {
            containerPos[i] = cu[i].transform;
        }
    }
}