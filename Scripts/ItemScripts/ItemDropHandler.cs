using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropHandler : MonoBehaviour
{
    public static ItemDropHandler instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            droptable.SetupItems();
        }
    }

    [Tooltip("Item aidroptable object")]public Droptable droptable;

    public List<ItemCont> GetRandomitems(AIInfo info)
    {
        float rand = GetRandomChance();

        List<ItemCont> items = droptable.GetRandomItems(rand, info);
        return items;
    }

    private const float maxOdd = 1000;
    private float GetRandomChance()
    {
        return Random.Range(0, maxOdd) / maxOdd;
    }
}
