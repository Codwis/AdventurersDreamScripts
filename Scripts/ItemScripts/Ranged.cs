using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "New Ranged", menuName = "Equipment/New Ranged Weapon")]
public class Ranged : Weapon
{
    [Tooltip("How much offset there can be in a shot")]
    public float maxAccuracyOffset;
    [Range(0, 1), Tooltip("How much of the charge should be so arrow can be shot")]
    public float minCharge;
    [Tooltip("How long can the bow be charged for")]
    public float maxChargeTime;
    [Tooltip("Maximum force that can be added to arrow when shot relative to charge times")]
    public float maxForce;

    [Tooltip("How Fast next arrow or bolt is loaded in seconds")]
    public float reloadSpeed;
    [Tooltip("How much stamina does the bow use")]
    public float staminaUse;
}
