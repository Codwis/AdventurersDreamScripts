using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Tooltip("Add different kinds of dialogue to this")]public DialogueEntry[] entries;
}

[System.Serializable]
public class DialogueEntry //Has only type and text array
{
    public DialogueType dialogueType;
    public string[] dialogueText;
}
