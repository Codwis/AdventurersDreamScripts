using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public AudioClip interactableSound;
    public virtual void Interact(Transform source)
    {
        if(interactableSound != null)
        {
            if(!Gamemanager.instance.globalAudio.isPlaying)
            {
                Gamemanager.instance.globalAudio.PlayOneShot(interactableSound);
            }
        }
    }
}
