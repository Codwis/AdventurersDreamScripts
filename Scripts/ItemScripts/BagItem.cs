using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Bag", menuName ="Equipment/Bag")]
public class BagItem : Equipment
{
    [Tooltip("How many slots does the bag have")]
    public int slotAmount;
}
