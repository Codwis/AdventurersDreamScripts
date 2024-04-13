using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : MonoBehaviour
{
    [Tooltip("Create a toolholder for each tool")] public string toolHolderName;
    public ToolTypes toolType;
    public bool holdable = false;
    private void Start()
    {
        
    }
}
public enum ToolTypes { pickaxe, axe, sack, watering_can, fishing_rod, scythe }