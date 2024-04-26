using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "New Spell", fileName = "Spell")]
public class Spell : ScriptableObject
{
    public SpellTypes spellType;
    public float energyUse;
    public GameObject spellPrefab;

    public AudioClip spellSound;
    public MouseInputs[] mouseMovements;
}

