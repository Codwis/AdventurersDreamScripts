using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Settings", menuName = "Misc/SettingsFile")]
public class Settings : ScriptableObject //Settings file
{
    [Tooltip("Set all the inputs early")] public List<Inputs> preInputs = new List<Inputs>();
    
    public void ChangeKey(string keyName, KeyCode key) //just simple way to rebind a key in the file
    {
        PlayerPrefs.SetInt(keyName, (int)key);
    }
}

[Serializable]
public struct Inputs
{
    public string keyName;
    public KeyCode key;
}
