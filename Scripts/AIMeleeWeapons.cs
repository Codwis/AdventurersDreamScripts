using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMeleeWeapons : MeleeWeapons
{
    [Header("AI Options")]
    [Tooltip("Collider in the weapon")]public Collider preMeleeCol;
    [Tooltip("Weapon Objects Transform")] public Transform weapon;
    [Tooltip("Weapon Holder")] public Transform weaponHolder;
    [Tooltip("Are the attacks fully random")]public bool random = true;
    [Tooltip("Only low swings?")]public bool lowSwings = false;
    [Tooltip("Attacks only from up")]public bool normalSwings = false;
    public float swingRange;

    public bool noSkills = true;
    private AIController controller;
    private int raycastMask;

    private const float maxDistance = 2.5f;

    private void Start()
    {
        stats = GetComponent<Stats>();
        controller = GetComponent<AIController>();
        raycastMask = ~LayerMask.GetMask("IgnoreRaycast"); // Gets everything else than ignoreraycasts

        meleeCollider = preMeleeCol;
        preMeleeCol = null;
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1;
        audioSource.volume = 0.25f;
    }

    private void Update()
    {
        if (stats == null) return;
        if (!stats.alive) return;

        if (canSwing)
        {
            if (meleeCollider != null)
            {
                if (meleeCollider.enabled)
                {
                    meleeCollider.enabled = false;
                }
            }
        }
        else if(!canSwing)
        {
            //StartCoroutine(AllowTrueAttack());
        }

        //sets closeness according to maxdistance
        anim.SetFloat("Close", controller.CheckForwardDistance(maxDistance, raycastMask));
        if (canSwing)
        {
            //if close enough to target then get random numbers and swing
            if(controller.CheckForDistance() < swingRange)
            {
                RandomSpot();
                if(noSkills)
                {
                    gridX = -1;
                }
                Swing(gridX, gridY);
            }
        }
    }

    //gets a random spot on a x,y grid
    private void RandomSpot()
    {
        if (normalSwings) //Hits from above
        {

            gridX = Random.Range(-1,2);
            gridX = Mathf.Round(gridX);
            gridY = 2;
            return;
        }
        else // Else hits anywhere on x
        {
            gridX = Random.Range(-6, 6);
        }

        if (lowSwings) //low swings
        {
            while(gridX < 3 && gridX > -3)
            {
                gridX = Random.Range(-6, 6);
            }
            gridY = Random.Range(-3, 0);
        }
        else if(random) //Random Y
        {
            if (gridX > 3 && gridX < -3)
            {
                gridY = Random.Range(-3, 6);
            }
            else
            {
                gridY = Random.Range(0, 6);
            }
        }
    }
    public void GrabWeapon()
    {
        weapon.SetParent(weaponHolder);
        weapon.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    public IEnumerator AllowTrueAttack() 
    {
        yield return new WaitForSeconds(currentMelee.delay + anim.GetCurrentAnimatorStateInfo(0).length);
        alreadyBlocked = false;
        canSwing = true;
        StopAllCoroutines();
    }
}
