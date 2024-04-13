using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapHider : MonoBehaviour
{
    public RawImage miniMap;
    public Image playerDot;
    private void OnTriggerEnter(Collider other)
    {
        miniMap.color = Color.clear;
        playerDot.color = Color.clear;
    }
    private void OnTriggerExit(Collider other)
    {
        miniMap.color = Color.white;
        playerDot.color = Color.red;
    }
}
