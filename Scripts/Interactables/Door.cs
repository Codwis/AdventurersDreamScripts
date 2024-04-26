using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interactable
{
    private Animator animator;
    public bool locked = false;

    public AudioClip lockedSound;
    public AudioClip unlockSound;

    public bool open = false;
    public bool inverse = false;

    public Item itemNeeded;
    public GrimoireScript grimScript;

    public Vector3 openRotation;
    private Vector3 closedRotation;

    public bool moveNotRotate = false;
    private void Start()
    {
        closedRotation = transform.eulerAngles;
        animator = GetComponent<Animator>();
    }

    public override void Interact(Transform source)
    {
        if(locked)
        {
            if(grimScript != null)
            {
                if (grimScript.currentGrimoire != null)
                {
                    DialogueHandler.instance.DisplayText("???", "It hears the hymn of the book");
                    if (unlockSound != null)
                    {
                        Gamemanager.instance.globalAudio.PlayOneShot(unlockSound);

                    }
                    locked = false;
                }
            }
            else if(itemNeeded != null)
            {
                if(Inventory.instance.FindItemAmount(itemNeeded) > 0)
                {
                    Inventory.instance.RemoveItems(itemNeeded, 1, false);
                    DialogueHandler.instance.DisplayText("You", "The key fit");
                    if (unlockSound != null)
                    {
                        Gamemanager.instance.globalAudio.PlayOneShot(unlockSound);
                    }
                    locked = false;
                }
            }
            else
            {
                if (lockedSound != null)
                {
                    Gamemanager.instance.globalAudio.PlayOneShot(lockedSound);
                }
                DialogueHandler.instance.DisplayText("You", "It's locked..");
                return;
            }

        }
        base.Interact(source);



        if(!opening)
        {
            opening = true;
            open = !open;
            StartCoroutine(OpenClose());
        }

    }
    bool opening = false;
    int maxOpenings = 25;
    private IEnumerator OpenClose()
    {
        int openAmount = 0;
        if(open)
        {
            if(!moveNotRotate)
            {
                while (openAmount < maxOpenings)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(openRotation), maxOpenings * Time.deltaTime);
                    openAmount++;
                    yield return new WaitForEndOfFrame();
                }
            }
            else
            {
                while (openAmount < maxOpenings)
                {
                    transform.localPosition = Vector3.Lerp(transform.localPosition, openRotation, maxOpenings * Time.deltaTime);
                    openAmount++;
                    yield return new WaitForEndOfFrame();
                }
            }


        }
        else
        {
            if(!moveNotRotate)
            {
                while (openAmount < maxOpenings)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(closedRotation), maxOpenings * Time.deltaTime);
                    openAmount++;
                    yield return new WaitForEndOfFrame();
                }
            }
            else
            {
                while (openAmount < maxOpenings)
                {
                    transform.localPosition = Vector3.Lerp(transform.localPosition, closedRotation, maxOpenings * Time.deltaTime);
                    openAmount++;
                    yield return new WaitForEndOfFrame();
                }
            }

        }
        opening = false;
    }
}
