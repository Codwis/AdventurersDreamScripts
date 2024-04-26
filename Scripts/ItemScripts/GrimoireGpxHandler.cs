using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrimoireGpxHandler : MonoBehaviour
{
    public Material activeElectric;
    public Material activeFire;

    public int electricIndex;
    public int fireIndex;

    public void ActivateElement(ElementTypes elementType, SkinnedMeshRenderer rend)
    {
        List<Material> mats = new List<Material>();
        rend.GetMaterials(mats);
        if (elementType == ElementTypes.electric)
        {
            mats[electricIndex] = activeElectric;
        }
        else if(elementType == ElementTypes.fire)
        {
            mats[fireIndex] = activeFire;
        }
        rend.SetMaterials(mats);
    }
}
