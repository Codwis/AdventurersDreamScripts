using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AiInfo", menuName = "Create AiInfo")]
public class AIInfo : ScriptableObject
{
    [Tooltip("Ai name")] public string aiName;
    [Tooltip("What type of ai is it")] public AiType aiType;
    [Tooltip("At what distance does ranged ai shoot")] public float shootRange;
    [Tooltip("At what distance does ai agro")] public float agroRange;
    [Tooltip("At what distance does ai start backing away")]public float backupRange = 0.75f;
    [Tooltip("0 Is fully accurate"), Range(0,10)] public float accuracy;
    [Tooltip("How far can they pickup object")] public float pickupRange;
    [Tooltip("Can ai spawn random sized")]public bool randomSize;
    [Tooltip("Smallest and Biggest size ai can be")]public float minRandom = 2, maxRandom = 4;
}
