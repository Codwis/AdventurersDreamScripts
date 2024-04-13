using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "New Melee", menuName = "Equipment/New Melee")]
public class Melee : Weapon
{
    public float delay;
    public float staminaUse;

    public bool singleHand = false;
    public float blockDelay;
    public int maxHitsPerSwing = 2;
    public MeleeType meleeType = MeleeType.Sword;

    [Space(2)]
    public Vector3 leftHandHolderSpot;
}
public enum MeleeType { Sword, Axe, Pickaxe }
