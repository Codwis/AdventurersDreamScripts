using IngameDebugConsole;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hotbar : MonoBehaviour
{
    public Inventory inventory;
    public ItemInSlot exampleBag;
    public ItemInSlot exampleItem;
    public ItemInSlot exampleItem2;

#if UNITY_EDITOR
    private bool active = true;
#else
    private bool active = false;
#endif

    //Under Work// Temporary adds items for debugging
    void Update()
    {
        if (!active) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            inventory.AddItem(exampleBag, 1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            inventory.AddItem(exampleItem, 1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            inventory.AddItem(exampleItem2,1);
        }
    }


    [ConsoleMethod("EnableNumbers","Enables 1-3 you can spawn items")]
    public void EnableNumbers()
    {
        active = !active;
    }
}
