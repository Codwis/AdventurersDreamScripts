using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIRanged : RangedScript
{
    [Header("AI Stuff")]
    [Tooltip("Maximum Movement Speed")]public float maxSpeed;

    [NonSerialized] public Transform lookSpot;
    private AIController controller;

    private bool enoughToShoot = false;
    private float ratioForTheShot;

    private void Awake()
    {
        
    }
    private void Start()
    {
        currentBowScript = GetComponentInChildren<BowGpxScript>();
        bowAnim = GetComponent<Animator>();
        canCharge = false;
        lookSpot = currentBowScript.arrowLookHere;
        stats = GetComponent<Stats>();
        DrawNewArrow();
    }

    //sets controller early
    public void SetController(AIController aiController)
    {
        controller = aiController;
    }

    public void DrawBow()
    {
        currentBowScript.ActivateConstraint();
    }

    private void Update()
    {
        if (controller == null)
            return;

        //get the ratio and clamp it
        float t = controller.CalculateDistanceRatio();
        t = Mathf.Clamp(t, currentBow.minCharge, 1);

        if (!enoughToShoot)
        {
            CheckForStamina(t);
        }

        if(charge == 0)
        {
            stringSpot.LookAt(lookSpot);
        }
        if(controller.CanShoot() && enoughToShoot) // this needs to be changed at some point
        {
            if (canCharge) // if arrow is ready then start charging 
            {
                if(startChargeTime == 0)
                {
                    startChargeTime = Time.time;
                    controller.SetSpeed(maxSpeed / 2f);
                }

                stringSpot.LookAt(lookSpot);

                //Calculates the current charge and sets animator
                charge = (Time.time - startChargeTime) / currentBow.maxChargeTime;
                bowAnim.SetFloat("Charge", charge);

                if (staminaConsumed < currentBow.staminaUse) //If using max stamina already
                {
                    if (stats.EnoughStaminaFor(currentBow.staminaUse * charge * currentBow.maxChargeTime * Time.deltaTime)) // checks player has enough stamina
                    {
                        //Uses stamina relative to charge and the maxcharge time
                        staminaConsumed += currentBow.staminaUse * charge * currentBow.maxChargeTime * Time.deltaTime;
                        stats.DrainStamina(currentBow.staminaUse * charge * currentBow.maxChargeTime * Time.deltaTime);
                    }
                    else
                    {
                        mustShoot = true;
                    }
                }

                //checks if charge is more than the distance ratio and shoots if nothing is blocking the way
                if (charge >= ratioForTheShot || mustShoot && charge >= currentBow.minCharge)
                {
                    if (!CheckForObjects()) //Resets all values and shoots the arrow
                    {
                        canCharge = false;
                        mustShoot = false;
                        enoughToShoot = false;

                        bowAnim.SetTrigger("Shoot");
                        bowAnim.SetFloat("Charge", 0);

                        currentBowScript.ActivateConstraint(false);
                        stringSpot.LookAt(lookSpot);

                        Shoot(currentArrow);
                    }
                } 
            }
        }
        else if(charge > 0 && !controller.CanShoot()) //cancel the charge if player goes out of range
        {
            CancelCharge();
        }
    }

    //Checks if there is enough stamina to shoot the target
    public void CheckForStamina(float ratio)
    {
        if (ratio < currentBow.minCharge && !enoughToShoot)
        {
            if (stats.EnoughStaminaFor(currentBow.staminaUse * currentBow.minCharge))
            {
                enoughToShoot = true;
                ratioForTheShot = currentBow.minCharge;
            }
        }
        else if (stats.EnoughStaminaFor(currentBow.staminaUse * ratio) && !enoughToShoot)
        {
            enoughToShoot = true;
            ratioForTheShot = ratio;
        }
    }

    //Just cancel the charge resets values and such
    private void CancelCharge()
    {
        bowAnim.SetTrigger("Cancel");
        bowAnim.SetFloat("Charge", 0);

        charge = 0;
        startChargeTime = 0;

        controller.SetSpeed(maxSpeed);
        currentBowScript.ActivateConstraint(false);
    }

    //Checks if there is something in the way of target and arrow
    private bool CheckForObjects()
    {
        return false;
        if (Physics.Raycast(transform.position, controller.GetCurrentTarget() - transform.position, out RaycastHit hit, 10))
        {
            if (hit.transform.root.GetComponent<PlayerController>())
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    
    //After arrow drawing animation (Animation event)
    private void ArrowInPlace() 
    {
        currentBowScript.ActivateConstraint(true);
        staminaConsumed = 0;
    }
    //Makes a new arrow middle of the drawing animation (Animation event)
    private void RespawnArrow()
    {
        currentArrow = Instantiate(arrow, stringSpot.position, stringSpot.rotation, stringSpot);
        canCharge = true;
    }
    //Starts the drawing animation
    private void DrawNewArrow()
    {
        bowAnim.SetTrigger("Draw");
    }

    //Shoots the given arrow
    private void Shoot(GameObject arrow)
    {
        //Removes parent and currentarrow so it cant be shot twice
        arrow.transform.parent = null;
        currentArrow = null;

        //Activate the arrow changes the damage aswell
        ArrowScript arrowScript = arrow.GetComponent<ArrowScript>();
        arrowScript.ChangeDamage(arrowDamage + currentBow.baseDamage * charge);
        arrowScript.enabled = true;
        arrowScript.source = transform;
        arrow.GetComponent<Rigidbody>().isKinematic = false;
        arrow.GetComponent<Collider>().enabled = true;

        //Calculates the offset depending on the charge
        Vector3 offset;
        float aim = currentBow.maxAccuracyOffset * (-charge + 1);
        offset = new Vector3(UnityEngine.Random.Range(-aim, aim), UnityEngine.Random.Range(-aim, aim));

        //Makes arrow look at the spot and adds force relative to the charge
        arrow.transform.LookAt(controller.GetCurrentTarget() + offset);
        arrow.GetComponent<Rigidbody>().AddForce(arrow.transform.forward * (currentBow.maxForce * charge), ForceMode.Impulse);

        //Resets values
        startChargeTime = 0;
        canCharge = false;
        controller.SetSpeed(maxSpeed);
        charge = 0;

        //Respawns a new arrow after delay
        Invoke("DrawNewArrow", currentBow.reloadSpeed);
    }
}
