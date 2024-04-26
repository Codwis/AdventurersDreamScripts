using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "New Grimoire", menuName = "Equipment/Grimoire")]
public class Grimoire : Equipment
{
    public List<Spell> spells;

    public AudioClip openSound;
    public List<ElementTypes> elementTypes;
}


[Serializable]
public enum SpellTypes { bolt, spitter, shield, summon }
[Serializable]
public enum ElementTypes { fire, electric }
