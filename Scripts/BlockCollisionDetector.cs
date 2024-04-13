using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCollisionDetector : MonoBehaviour
{
    [Tooltip("Main melee weapons script in the character")] public MeleeWeapons meleeWeapons;
    private void Start()
    {
        meleeWeapons = transform.root.GetComponent<MeleeWeapons>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.transform.root == gameObject.transform.root)
            return;

        //If there is a collision detector can assume its a weapon and block
        if (collision.gameObject.TryGetComponent<MeleeCollisionDetector>(out MeleeCollisionDetector teehee))
        {
            meleeWeapons.BlockedAttack();
            teehee.GetComponentInParent<MeleeWeapons>().BlockedAttack(true);
        }
    }
}
