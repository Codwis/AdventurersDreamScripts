using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDescriptionHandler : MonoBehaviour
{
    public Text itemName;
    public Text rarity;
    public Image itemImage;
    public CanvasGroup cg;

    private void Start()
    {
        Inventory.instance.SetDescriptionHandler(this);
        cg = GetComponent<CanvasGroup>();
        ClearDescription();
    }

    public void ChangeDescription(ItemInSlot item)
    {
        if(item.item != null)
        {
            itemName.text = item.item.itemName;
            EquipmentRarityController.rarities.TryGetValue(item.itemNum, out Rarity rar);
            rarity.text = rar.ToString();
            itemImage.sprite = item.item.logo;

            cg.alpha = 1;
        }
    }
    public void ClearDescription()
    {
        cg.alpha = 0;
    }
}
