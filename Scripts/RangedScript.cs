using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
public class RangedScript : MonoBehaviour
{
    //pepeLaugh those who think this isint mine lul tried out the SerializeField didnt really like it or actually maybe
    [Header("Arrow and Bow Related")]

    [Tooltip("Place where arrow shoots from")] public Transform stringSpot;
    [Tooltip("Change In future to different with inventory")] public GameObject arrow;
    [Tooltip("Temporary how much damage arrow deal")] public float arrowDamage; //temp
    [Tooltip("No need to change only for the ai")] public Ranged currentBow;

    private bool pullStarted = false;
    private Transform currentBowLookSpot;

    [NonSerialized] public bool mustShoot = false;
    [NonSerialized] public float staminaConsumed = 0;

    [NonSerialized] public BowGpxScript currentBowScript;
    [NonSerialized] public GameObject currentArrow;
    [NonSerialized] public Animator bowAnim;


    [Header("UI Related Not For AI")]

    [Tooltip("Animator for UI"), SerializeField] private Animator uiAnim;
    [Tooltip("CanvasGroup for the ui crosshair"), SerializeField] private CanvasGroup crosshair;


    [Header("Audio")]

    [SerializeField] public AudioClip shootSound;
    [SerializeField] public AudioClip drawSound;
    [SerializeField] public AudioClip pullSound;

    private AudioSource audioSource;


    [Header("Misc")]
    [SerializeField] private EquipmentHandler handler;
    [SerializeField] private Camera cam;

    [NonSerialized] public float startChargeTime;
    [NonSerialized] public float charge;
    [NonSerialized] public bool canCharge = true;
    [NonSerialized] public Stats stats;


    private void Awake()
    {
        handler.SetRangedScript(this);
        bowAnim = transform.GetComponent<Animator>();
        stats = transform.GetComponent<Stats>();

        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!stats.alive) return;

        if(currentBow == null && currentArrow != null)
        {
            Destroy(currentArrow);
            
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (handler.IsSheathed() && Input.GetMouseButton(0))
        {
            charge = 0;
            mustShoot = false;
            pullStarted = false;
            bowAnim.SetTrigger("Draw");
            bowAnim.SetBool("Sheathed", false);
        }
        else if (canCharge && !handler.IsSheathed())
        {
            stringSpot.LookAt(currentBowLookSpot);
            //Sets start time when mouse 0 is first pressed
            if (!pullStarted && Input.GetMouseButtonDown(0) && currentArrow != null)
            {
                if(!stats.EnoughStaminaFor(currentBow.staminaUse * currentBow.minCharge))
                {
                    return;
                }
                if (pullSound != null)
                {
                    audioSource.PlayOneShot(pullSound);
                }

                pullStarted = true;
                staminaConsumed = 0;
                mustShoot = false;
                startChargeTime = Time.time;
                currentBowScript.ActivateConstraint(true);
                stringSpot.LookAt(currentBowLookSpot);
            }
            else if (pullStarted && Input.GetMouseButton(0) && !mustShoot && currentArrow != null)
            {
                //Increases charge if held down
                charge = (Time.time - startChargeTime) / currentBow.maxChargeTime;
                charge = Mathf.Clamp(charge, 0, 1);
                
                if (staminaConsumed < currentBow.staminaUse) //If using max stamina already
                {
                    if(stats.EnoughStaminaFor(currentBow.staminaUse * charge * currentBow.maxChargeTime * Time.deltaTime)) // checks player has enough stamina
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

                //Sets animator floats
                uiAnim.SetFloat("Charge", charge);
                bowAnim.SetFloat("Charge", charge);
            }
            else if (Input.GetMouseButtonUp(0) || mustShoot)
            {
                //Mouse released clamps the charge
                charge = Mathf.Clamp01(charge);
                pullStarted = false;
                mustShoot = false;

                //If charge is more than mininum required
                //And there is an arrow then shoots the arrow
                if (charge >= currentBow.minCharge)
                {
                    if (currentArrow != null)
                    {
                        Shoot(currentArrow);
                    }
                }

                //Resets animator floats
                uiAnim.SetFloat("Charge", 0);
                bowAnim.SetFloat("Charge", 0);
            }
        }
    }

    //Stops charge if too close to object
    private void StopCharge()
    {
        uiAnim.SetFloat("Charge", 0);
        bowAnim.SetFloat("Charge", 0);
    }

    //Checks if there is something in front
    public void CheckObstructions()
    {
        float distance = handler.CheckForwardDistance(0.5f);
        if(distance > 0.1f)
        {
            canCharge = false;
            StopCharge();
        }
        else
        {
            
            canCharge = true;
        }
    }

    //Starts arrow spawning process
    private IEnumerator StartRespawnArrow()
    {
        yield return new WaitForSeconds(currentBow.reloadSpeed);
        if(bowAnim != null)
        {
            bowAnim.SetFloat("Charge", 0);
            bowAnim.SetTrigger("Draw");
        }
    }
    //Respawns the arrow (Animation event)
    private void RespawnArrow()
    {
        charge = 0;
        bowAnim.SetFloat("Charge", 0);
        currentArrow = Instantiate(arrow, stringSpot.position, stringSpot.rotation, stringSpot);
        currentArrow.gameObject.layer = gameObject.layer;
        currentArrow.transform.localRotation = Quaternion.identity;
    }

    //Gets called when arrow is on the bow (animation event)
    public void ArrowReady() 
    {
        if (drawSound != null)
        {
            audioSource.PlayOneShot(drawSound);
        }

        currentBowScript.ActivateConstraint(true);
        stringSpot.LookAt(currentBowLookSpot);
    }

    //Changes the current bow to a new one
    public void ChangeCurrentBow(Ranged rangedWeapon)
    {
        charge = 0;
        bowAnim.SetFloat("Charge", 0);

        currentBow = rangedWeapon;
    }
    //Change current bow script to new one
    public void ChangeCurrentBowScript(BowGpxScript script)
    {
        currentBowScript = script;
        currentBowScript.SetConstraint(stringSpot);
        currentBowLookSpot = currentBowScript.arrowLookHere;
        stringSpot.localRotation = currentBowScript.originalRotation;
    }

    public void SetHandler(EquipmentHandler handlerToSet)
    {
        handler = handlerToSet;
    }

    public void SetCrosshairAlpha(float alpha)
    {
        crosshair.alpha = alpha;
    }

    //Shoots the arrow when called
    private void Shoot(GameObject arrow)
    {
        if (currentBow.swoosh != null)
        {
            audioSource.PlayOneShot(currentBow.swoosh);
        }

        //Removes parent and currentarrow so it cant be shot twice
        arrow.transform.parent = null;
        currentArrow = null;

        bowAnim.SetTrigger("Shoot");
        currentBowScript.ActivateConstraint(false);

        //Activate the arrow changes the damage aswell
        ArrowScript arrowScript = arrow.GetComponentInChildren<ArrowScript>();
        arrowScript.ChangeDamage(arrowDamage + stats.damage * charge);
        arrowScript.enabled = true;
        arrowScript.source = transform;
        arrow.GetComponentInChildren<Rigidbody>().isKinematic = false;
        arrow.GetComponentInChildren<Collider>().enabled = true;

        //Calculates the offset depending on the charge
        Vector3 offset;
        float aim = currentBow.maxAccuracyOffset * (-charge + 1);
        offset = new Vector3(UnityEngine.Random.Range(-aim, aim), UnityEngine.Random.Range(-aim, aim));

        //Makes arrow look at the spot and adds force relative to the charge
        arrow.transform.LookAt(cam.transform.position + (cam.transform.forward * 10) + offset + Vector3.up);
        arrow.GetComponentInChildren<Rigidbody>().AddForce(arrow.transform.forward * (currentBow.maxForce * charge), ForceMode.Impulse);

        charge = 0;

        StartCoroutine(StartRespawnArrow());
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (this is AIRanged) return;
        return;
        Quaternion rot = Quaternion.Euler(cam.transform.forward + Vector3.up * 90);
        Quaternion rott = Quaternion.LookRotation(Vector3.up);
        bowAnim.SetBoneLocalRotation(HumanBodyBones.Spine, rott);
    }
}
