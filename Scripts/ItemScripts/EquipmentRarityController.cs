using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentRarityController : MonoBehaviour
{
    public static Dictionary<int, Rarity> rarities = new Dictionary<int, Rarity>();

    public static EquipmentRarityController instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void SetRarity(int itemNum, Rarity rarVal = Rarity.Random)
    {
        if (rarities.TryGetValue(itemNum, out _)) return;

        if (itemNum == -1)
        {
            rarities.Add(itemNum, Rarity.Common);
            return;
        }
        else if(itemNum == -2)
        {
            rarities.Add(itemNum, Rarity.Uncommon);
            return;
        }
        else if (itemNum == -3)
        {
            rarities.Add(itemNum, Rarity.Rare);
            return;
        }
        else if (itemNum == -4)
        {
            rarities.Add(itemNum, Rarity.Legendary);
            return;
        }
        else if (itemNum == -676)
        {
            rarities.Add(itemNum, Rarity.Creator);
            return;
        }

        if (rarVal != Rarity.Random)
        {
            rarities.Add(itemNum, rarVal);
            return;
        }

        Rarity rarity;
        int rand = UnityEngine.Random.Range(0, 1000);
        if (rand > 980) rarity = Rarity.Legendary;
        else if (rand > 800) rarity = Rarity.Rare;
        else if (rand > 500) rarity = Rarity.Uncommon;
        else
        {
            rarity = Rarity.Common;
        }

        rarities.Add(itemNum, rarity);
    }
    public Rarity GetRarity(int itemNum)
    {
        rarities.TryGetValue(itemNum, out Rarity ra);
        return ra;
    }

    public static void SaveData()
    {
        SaveSystem.SaveRarities(rarities);
    }
    public static void LoadData()
    {
        rarities = SaveSystem.LoadRarities();
    }
}
