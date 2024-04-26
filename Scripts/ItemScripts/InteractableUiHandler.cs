using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableUiHandler : MonoBehaviour
{
    public static InteractableUiHandler instance;
    private CanvasGroup currentUi;
    [NonSerialized] public bool open = false;
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void ShowUi(CanvasGroup group)
    {
        open = true;
        currentUi = group;
        currentUi.alpha = 1;
        currentUi.blocksRaycasts = true;
        currentUi.interactable = true;
    }

    public void HideUi()
    {
        open = false;
        currentUi.alpha = 0;
        currentUi.blocksRaycasts = false;
        currentUi.interactable = false;
        currentUi = null;
    }
}
