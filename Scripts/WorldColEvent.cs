using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldColEvent : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        DialogueHandler.instance.DisplayText("???", "You are not ready for the challenge...");
    }
}
