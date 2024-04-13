using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetupKey : MonoBehaviour
{
    public Text inputName;
    public Text currentSet;
    public Button button;



    public void SetupKeyNow(string key, string currentKey)
    {
        inputName.text = key;
        currentSet.text = currentKey;

        button.onClick.AddListener(ButtonClick);
    }

    public void ButtonClick()
    {
        InputHandler.instance.Rebind(inputName.text, currentSet);
    }
}
