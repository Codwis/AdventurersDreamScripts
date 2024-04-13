using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interactable
{
    private Animator animator;
    public bool locked = false;

    public AudioClip lockedSound;
    public bool open = false;
    public bool inverse = false;
    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public override void Interact(Transform source)
    {
        if(locked)
        {
            if(lockedSound != null)
            {
                Gamemanager.instance.globalAudio.PlayOneShot(lockedSound);
            }
            DialogueHandler.instance.DisplayText("You", "It's locked..");
            return;
        }
        base.Interact(source);
        
        if(inverse)
        {
            animator.SetTrigger("Inverse");
        }
        else
        {
            animator.SetTrigger("Open");
        }
    }
}
