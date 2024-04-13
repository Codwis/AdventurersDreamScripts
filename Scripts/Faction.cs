using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Faction", menuName = "Create New Faction")]
public class Faction : ScriptableObject
{
    public LayerMask enemies;
    public LayerMask friendly;
    
    public void AddEnemy(int layer)
    {
        int layerVal = 1 << layer;
        enemies |= layerVal;
    }

    public void RemoveEnemy(int layer)
    {
        int layerVal = ~(1 << layer);
        enemies &= layerVal;
    }

    public bool CheckIfEnemy(int layer)
    {
        if(((1 << layer) & enemies) != 0 )
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
