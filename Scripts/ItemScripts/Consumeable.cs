using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "New Consumable", menuName = "New Consumable")]
public class Consumeable : Item
{
    public float healAmount;
    public float staminaRegen;

    public float staminaIncrease;
    public float healthIncrease;
    public float strengthIncrease;

    public GameObject summon;
    
    public override void Use(Stats source)
    {
        base.Use(source);
        source.TakeDamage(-healAmount, null);
        source.currentStamina += staminaRegen;

        PlayerPrefs.SetFloat("Damage", PlayerPrefs.GetFloat("Damage") + strengthIncrease);
        PlayerPrefs.SetFloat("Health", PlayerPrefs.GetFloat("Health") + healthIncrease);
        PlayerPrefs.SetFloat("Stamina", PlayerPrefs.GetFloat("Stamina") + staminaIncrease);
        PlayerPrefs.Save();

        if(summon != null)
        {
            Summon(source);
        }

        source.UpdateValues();
    }

    private void Summon(Stats source)
    {
        GameObject temp = Instantiate(summon, source.transform.position - source.transform.forward, summon.transform.rotation);
        
        foreach(AIController i in temp.GetComponentsInChildren<AIController>())
        {
            source.summons.Add(i);
            i.follower = true;
            i.player = source.transform;
        }
    }
}
