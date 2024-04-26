using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Item", menuName = "Regular Item")]
public class Item : ScriptableObject
{
    public string itemName;
    [Tooltip("Prefab for the item")]
    public GameObject prefab;
    [Tooltip("Sprite used in inventory")]
    public Sprite logo;

    [Range(0,1)] public float dropChance;

    public bool dropable = true;
    [Min(1)]
    public int stackSize = 1;

    public Rarity rarityPreset = Rarity.Random;
    public itemTypes itemType = itemTypes.regular;
    public ElementTypes elementType;

    public virtual void Use(Stats source)
    {

    }
}
