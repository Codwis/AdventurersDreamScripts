using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateControlSettings : MonoBehaviour
{
    public Settings settings;
    public GameObject controlPrefab;

    private GameObject[] inputs;
    public static CreateControlSettings instance;

    private void Start()
    {
        if(instance == null)
        {
            instance = this;
        }

        CreateKeys();
    }

    private void CreateKeys()
    {
        inputs = new GameObject[settings.preInputs.Count];
        int i = 0;
        foreach (Inputs input in settings.preInputs)
        {
            GameObject temp = Instantiate(controlPrefab, transform);
            inputs[i] = temp;

            if (PlayerPrefs.HasKey(input.keyName))
            {
                KeyCode c = (KeyCode)PlayerPrefs.GetInt(input.keyName);
                temp.GetComponent<SetupKey>().SetupKeyNow(input.keyName, c.ToString());
            }

            i++;
        }
    }

    public void ResetKeys()
    {
        for(int i = 0; i < inputs.Length; i++)
        {
            Destroy(inputs[i]);
        }
        CreateKeys();
    }
}
