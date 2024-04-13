using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeCollisionDetector : MonoBehaviour
{
    [Tooltip("Main script in the character")]private MeleeWeapons mainScript;
    private void Start()
    {
        mainScript = transform.GetComponentInParent<MeleeWeapons>();
    }
    public void StopCollider()
    {
        mainScript.meleeCollider.enabled = false;
    }
    private void OnCollisionEnter(Collision collision) 
    {
        //Tests if its own self
        if (collision.gameObject.GetComponentInParent<MeleeWeapons>() == mainScript)
        {
            return;
        }

        if(mainScript != null)
        {
            mainScript.CollisionDetected(collision);
        }
    }
}
