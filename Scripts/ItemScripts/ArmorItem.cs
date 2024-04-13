using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Armor", menuName ="Equipment/Armor")]
public class ArmorItem : Equipment
{
    public int baseArmor;
    public HumanBodyBones boneToOffset;
    public Vector3 offset;
    public Vector3 scaleOffset;
}
