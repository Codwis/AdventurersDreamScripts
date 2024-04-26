using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEditor;
using System;

public static class SaveSystem
{
    public static void SavePlayer(Inventory inventory, Stats stats, Vector3 pos)
    {

        BinaryFormatter formatter = new BinaryFormatter();

        string path = Application.persistentDataPath + "/player.dat";

        FileStream stream = new FileStream(path, FileMode.Create);

        StatsData statsData = new StatsData(stats.health, stats.maxHealth, stats.maxStamina);

        EquipmentSlot[] eqSlots = inventory.GetComponentsInChildren<EquipmentSlot>();

        ItemSaveData[] items = new ItemSaveData[inventory.usedSlots.Count + Inventory.instance.bags.Length + eqSlots.Length];

        int[] amounts = new int[items.Length];
        int count = 0;
        BagScript[] bags = inventory.bags;

        foreach(BagScript bag in bags)
        {
            if (bag.GetComponent<ContainerScript>()) continue;

            if (bag.currentBag == null) continue;

            ItemInSlot tempItem = new ItemInSlot();
            tempItem.item = bag.currentBag;

            ItemSaveData temp = new ItemSaveData(tempItem, bag.bagSlot);

            items[count] = temp;
            count++;
        }

        foreach(EquipmentSlot eqSl in inventory.GetComponentsInChildren<EquipmentSlot>())
        {
            if (eqSl.itemInSlot.item == null) continue;
            ItemSaveData itemData = new ItemSaveData(eqSl.itemInSlot, eqSl);
            Debug.Log(eqSl.name);
            items[count] = itemData;
            count++;
        }

        foreach(Slot slo in inventory.usedSlots)
        {
            ItemSaveData itemData = new ItemSaveData(slo.itemInSlot, slo);
            items[count] = itemData;
            amounts[count] = slo.GetItemAmount();
            count++;
        }

        InventoryData invData = new InventoryData(items, amounts);

        PlayerQuestHandler handler = stats.GetComponentInChildren<PlayerQuestHandler>();
        string[] quests = handler.questsDone.ToArray();

        QuestData questData = new QuestData(quests);



        PlayerData data = new PlayerData(pos, statsData, questData);


        formatter.Serialize(stream, data);
        stream.Close();

        path = Application.persistentDataPath + "/inventory.json";
        string json = JsonUtility.ToJson(invData);
        File.WriteAllText(path, json);

        path = Application.persistentDataPath + "/containers.json";
        ContainerUnit[] containers = UnityEngine.Object.FindObjectsByType<ContainerUnit>(FindObjectsSortMode.InstanceID);
        ContainerData contData = new ContainerData(containers, Gamemanager.newGame);

        json = JsonUtility.ToJson(contData);

        File.WriteAllText(path, json);
    }

    public static void SaveContainers()
    {
        string path, json;
        path = Application.persistentDataPath + "/containers.json";
        ContainerUnit[] containers = UnityEngine.Object.FindObjectsByType<ContainerUnit>(FindObjectsSortMode.InstanceID);
        ContainerData contData = new ContainerData(containers, Gamemanager.newGame);

        json = JsonUtility.ToJson(contData);

        File.WriteAllText(path, json);
    }

    public static void SaveRarities(Dictionary<int, Rarity> rarities)
    {
        string path = Application.persistentDataPath + "/Rarities.json";
        string json = JsonUtility.ToJson(rarities);

        File.WriteAllText(path, json);
    }
    public static Dictionary<int, Rarity> LoadRarities()
    {
        Dictionary<int, Rarity> rarities;
        string path = Application.persistentDataPath + "/Rarities.json";
        string json = File.ReadAllText(path);
        rarities = JsonUtility.FromJson<Dictionary<int,Rarity>>(json);

        return rarities;
    }

    public static PlayerData LoadPlayer()
    {
        string path = Application.persistentDataPath + "/player.dat";
        if(File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            PlayerData data = (PlayerData)formatter.Deserialize(stream);

            stream.Close();
            return data;
        }
        else
        {
            return null;
        }
    }
    public static InventoryData LoadInventory()
    {
        string path = Application.persistentDataPath + "/inventory.json";
        if(File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<InventoryData>(json);
        }
        else
        {
            Debug.Log("Uhoh");
        }
        return null;
    }

    public static void LoadContainer(ulong id, out ItemCont[] ti)
    {
        string path = Application.persistentDataPath + "/containers.json";
        if(File.Exists(path))
        {
            string json = File.ReadAllText(path);
            ContainerData contData = JsonUtility.FromJson<ContainerData>(json);
            int i = 0;

            foreach (ContainerItemData container in contData.items)
            {
                if (container.contId.Equals(id))
                {

                    ti = container.items;
                    return;

                }
                i++;
            }
        }

        ti = null;
    }
}

[Serializable]
public class ItemSaveData
{
    public ItemInSlot item;
    public Slot slotType;

    public ItemSaveData(ItemInSlot itemIn, Slot slot)
    {
        item = itemIn;
        slotType = slot;
    }
}
