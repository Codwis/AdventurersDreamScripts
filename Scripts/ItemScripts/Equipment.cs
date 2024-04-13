using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Equipment : Item
{
    [Header("Graphics Related")]
    public Vector3 localPosition;
    public Vector3 localRotation;
    public Vector3 localScale;

    
    [Header("Only for boots")]
    public GameObject otherBoot;
    public Mesh equipmentMesh;

    [Header("Type")]
    public EquipmentType equipmentType;

    [Header("Sounds")]
    public AudioClip swoosh;
    public AudioClip takeOutSound;
}

public enum Rarity { Common, Uncommon, Rare, Legendary, Random, Creator }