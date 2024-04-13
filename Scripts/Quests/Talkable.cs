using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Talkable : Interactable
{
    // ADD COMMENTS

    public Dialogue[] textToSay;

    private bool introduced = false;
    private int talkAmount = 0;

    public bool talkNormalAfter = true;
    [NonSerialized] public AIController controller;
    [NonSerialized] public Stats stats;
    private void Start()
    {
        TryGetComponent<AIController>(out controller);
        TryGetComponent<Stats>(out stats);
    }

    public override void Interact(Transform source)
    {
        base.Interact(source);
        if(stats != null)
        {
            if (stats.faction.CheckIfEnemy(source.gameObject.layer))
            {
                return;
            }
        }

        if (textToSay != null)
        {
            if(TryGetComponent<Animator>(out Animator te))
            {
                source.GetComponentInChildren<DialogueHandler>().DisplayText(name, textToSay, te);
            }
            else
            {
                source.GetComponentInChildren<DialogueHandler>().DisplayText(name, textToSay);
            }

            textToSay = null;
            introduced = true;
        }
        else if (!introduced && talkNormalAfter)
        {
            source.GetComponentInChildren<DialogueHandler>().DisplayText(name, DialogueType.Introductions);
            talkAmount++;
            introduced = true;
            StartCoroutine(ResetAmount());
        }
        else if(talkAmount == 0 && talkNormalAfter)
        {
            source.GetComponentInChildren<DialogueHandler>().DisplayText(name, DialogueType.Greetings);
            talkAmount++;
            StartCoroutine(ResetAmount());
        }
        else if(talkNormalAfter)
        {
            source.GetComponentInChildren<DialogueHandler>().DisplayText(name, DialogueType.SmallTalk);
        }

        controller.SetLookOfInterest(source.GetComponent<PlayerController>().cam.transform);
        StartCoroutine(ResetInterest());
    }

    private const float resetTime = 30f;
    public IEnumerator ResetInterest()
    {
        yield return new WaitForSeconds(resetTime);
        controller.RemoveLookOfInterest();
    }


    private IEnumerator ResetAmount()
    {
        yield return new WaitForSeconds(120);
        talkAmount = 0;
    }
}
