using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentMenu : MonoBehaviour
{
    private RectTransform rectTransform;
    private CanvasGroup group;
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        group = GetComponent<CanvasGroup>();
    }
    public void CloseOpen(bool open)
    {
        if (open)
        {
            group.alpha = 1;
        }
        else
        {
            group.alpha = 0;
        }

        group.interactable = open;
        group.blocksRaycasts = open;
    }
    public void CloseOpen()
    {
        if(group.alpha == 0)
        {
            group.alpha = 1;
            group.interactable = true;
            group.blocksRaycasts = true;
        }
        else
        {
            group.alpha = 0;
            group.interactable = false;
            group.blocksRaycasts = false;
        }
    }
    public bool IsOpen()
    {
        return gameObject.activeSelf;
    }

    public bool GetState()
    {
        return gameObject.activeSelf;
    }

    private Vector3 offset;
    public void GetPointerDown() //When pointer clicks down it gets offset to the mouse
    {
        offset = rectTransform.position - Input.mousePosition;
    }
    public void DragMenu()
    {
        Vector3 pos = Input.mousePosition - rectTransform.position + offset;
        rectTransform.position += pos;

        rectTransform.position = new Vector3(Mathf.Clamp(rectTransform.position.x, 0, Screen.width), Mathf.Clamp(rectTransform.position.y, 0, Screen.height));
    }
}
