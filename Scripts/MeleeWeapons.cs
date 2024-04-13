using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
public class MeleeWeapons : MonoBehaviour
{
    [Header("Crosshair Related")]
    public Transform crosshairTurner;
    public Transform crosshairPoint;

    [Header("Animation Related")]
    public Animator anim;
    [Tooltip("Animation used to reset values")] public AnimationClip returnAnimation;

    [Header("UI And Camera")]
    public CanvasGroup ui;
    public Camera playerCam;

    [Header("Melee Weapon Related")]
    public Melee currentMelee;
    [Tooltip("Attach Gameobject including collider and this script to camera")]public BlockCollisionDetector blockColDetector;

    [Header("Misc")]
    public Inventory inventory;
    [NonSerialized] public Stats stats;

    private bool alreadyEnded = false;
    private bool canBlock = true;
    [NonSerialized] public bool alreadyBlocked = false;

    [NonSerialized] public bool canSwing = true;

    [NonSerialized] public float mouseX, mouseY;
    [NonSerialized] public float gridX, gridY;
    [NonSerialized] public Collider meleeCollider;

    [NonSerialized] public Vector3 possibleSpot = new();

    public Transform leftHandHolder;
    [NonSerialized] public EquipmentHandler handler;
    private const float maxDistance = 3f;

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip unPenetrableSound;
    public AudioClip nonLivingHit;
    public AudioClip livingHit;
    public AudioClip blockSound;


    private void Start()
    {
        stats = transform.GetComponentInParent<Stats>();
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1;
    }
    private float input;

    private void Update()
    {
        if (!stats.alive) return;

        if(canSwing)
        {
            if(meleeCollider != null)
            {
                if (meleeCollider.enabled)
                {
                    meleeCollider.enabled = false;
                }
            }
        }

        //Gets axis values for the mouse 
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        //checks if either is other than 0
        if (mouseX != 0 || mouseY != 0)
        {

            //adds to the grid the mouse values
            gridX += mouseX;
            gridY += mouseY;

            //if grid point is below 0.5 on Y and too close to the middle then limit the spot 
            if (gridX < 5 && gridX > -5 && gridY < 0.5f)
            {
                if (gridX < 5)
                {
                    possibleSpot = crosshairTurner.position + new Vector3(5, gridY, 0);
                }
                else if (gridX > -5)
                {
                    possibleSpot = crosshairTurner.position + new Vector3(-5, gridY, 0);
                }
            }
            else
            {
                possibleSpot = crosshairTurner.position + new Vector3(gridX, gridY);
            }

            //Clamps the values so crosshairpoint doesnt go too far away from the middle
            gridX = Mathf.Clamp(gridX, -6, 6);
            gridY = Mathf.Clamp(gridY, -3, 6);

            //Moves the crosshairpoint to new location and makes crosshairTurner look at the crosshairpoint
            crosshairPoint.localPosition = new Vector3(gridX, gridY);
            crosshairTurner.LookAt(crosshairPoint);
        }

        if(currentMelee == null)
        {
            return;
        }

        //Left mouse button starts the swing
        if (Input.GetMouseButtonDown(0))
        {
            if (inventory.GetCursorItem() != null) return;

            if (handler.IsSheathed())
            {
                if (DialogueHandler.instance.IsTalking()) return;
                anim.SetTrigger("Draw");
                anim.SetBool("Sheathed", false);
            }
            else
            {
                Swing(possibleSpot.x - crosshairTurner.position.x, possibleSpot.y - crosshairTurner.position.y);
            }
        }

        input = Input.GetAxisRaw("Block");
        //Right mouse button activates blocking
        if ( canBlock && input != 0)
        {
            //really important for some reason script broke without this and spamming right mouse button
            //Just checks current animator state isint return animation
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(returnAnimation.name))
            {
                return;
            }
            canBlock = false;
            canSwing = false;
            alreadyEnded = false;

            //triggers the animation
            anim.SetTrigger("Block");
        }

        CheckObstructions();
    }


    //Checks if there are any objects forward and changes animator value
    public void CheckObstructions()
    {
        if (anim == null) return;

        if(handler != null)
        {
            float distance = handler.CheckForwardDistance(maxDistance);
            anim.SetFloat("Close", distance);
        }
    }

    //If succesfully blocks an attack allow an instant attack
    public void BlockedAttack(bool ai = false)
    {
        anim.SetTrigger("Return");
        anim.ResetTrigger("Block");
        anim.ResetTrigger("Swing");

        if(blockColDetector != null)
        {
            blockColDetector.gameObject.SetActive(false);
        }

        meleeCollider.enabled = false;

        if(!ai)
        {
            canBlock = true;
            canSwing = true;
        }
    }

    //Sets the equipmenthanndler and gives the handler this script
    public void SetHandler(EquipmentHandler eHandler)
    {
        handler = eHandler;
        handler.SetMeleeWeapons(this);
    }

    //Change the currentmelee
    public void ChangeCurrentWeapon(Melee weapon)
    {
        currentMelee = weapon;
        if(leftHandHolder != null && currentMelee != null)
        {
            leftHandHolder.localPosition = weapon.leftHandHolderSpot;
        }
    }

    //Function for swing
    public virtual void Swing(float x, float y)
    {
        if(stats == null)
        {
            stats = transform.GetComponentInParent<Stats>();
        }
        if (!stats.EnoughStaminaFor(currentMelee.staminaUse))
        {
            return;
        }
        if(this is AIMeleeWeapons ai) //for the ai
        {
            if (canSwing)
            {
                currentHits = 0;
                if (currentMelee.swoosh != null)
                {
                    audioSource.PlayOneShot(currentMelee.swoosh);
                }

                alreadyEnded = false;
                //set values
                alreadyBlocked = false;
                canSwing = false;
                anim.SetFloat("Mouse X", x);
                anim.SetFloat("Mouse Y", y);
                stats.DrainStamina(currentMelee.staminaUse);

                //triggers the animation
                anim.SetTrigger("Swing");
                ai.StartCoroutine(ai.AllowTrueAttack());
            }
        }
        else if (canSwing && !EventSystem.current.IsPointerOverGameObject()) // cant swing if over ui
        {
            currentHits = 0;
            if (currentMelee.swoosh != null)
            {
                audioSource.PlayOneShot(currentMelee.swoosh);
            }

            x = Mathf.Clamp(x, -6, 6);
            y = Mathf.Clamp(y, -3, 3);

            //set values
            anim.SetFloat("Mouse X", x);
            anim.SetFloat("Mouse Y", y);

            canSwing = false;
            alreadyEnded = false;
            alreadyBlocked = false;
            stats.DrainStamina(currentMelee.staminaUse);

            //triggers the animation
            anim.SetTrigger("Swing");
        }
    }

    //Allows attack after animation
    public void AllowAttack()
    {
        if(this is AIMeleeWeapons ai)
        {
            ai.StartCoroutine(ai.AllowTrueAttack());
            return;
        }

        if(meleeCollider != null)
        {
            meleeCollider.enabled = false;
        }

        canSwing = true;
        alreadyEnded = false;
    }

    //allows block after animation
    public void AllowBlock()
    {
        canBlock = true;
    }

    //Disables block after sword has pulled back
    public void DisableBlock()
    {
        canBlock = false;
    }

    public bool IsAttacking()
    {
        return !canSwing;
    }

    //Enable or Disable meleeCollider called with animation events 0 = on 1 = off
    public void EnableMeleeCol(int onOff)
    {
        if(onOff == 0)
        {
            meleeCollider.enabled = true;
        }
        else
        {
            meleeCollider.enabled = false;
        }
    }

    //Enables collider for the blocking 0 = on 1 = off
    public void EnableBlockCol(int onOff)
    {   
        if (onOff == 0)
        {
            blockColDetector.gameObject.SetActive(true);
        }
        else
        {
            blockColDetector.gameObject.SetActive(false);
        }
    }

    //this gets called when animation is over just resets the values
    public virtual void EndAnimation()
    {
        if (alreadyEnded) // if this has already occured
            return;
        alreadyEnded = true;

        EnableBlockCol(1);
        if (this is AIMeleeWeapons ai)
        {
            ai.StopAllCoroutines();
            ai.StartCoroutine(ai.AllowTrueAttack());
            return;
        }

        anim.ResetTrigger("Return");
        anim.ResetTrigger("Block");
        anim.ResetTrigger("Swing");

        Invoke("AllowBlock", currentMelee.blockDelay);
        meleeCollider.enabled = false;
        anim.SetLayerWeight(3, 100);

        alreadyEnded = true;
        canSwing = true;
    }

    private int currentHits = 0;
    //Gets called when collision happened
    public void CollisionDetected(Collision other)
    {

        if (!GetComponent<Stats>()) return;
        if (!GetComponent<Stats>().alive) return;
        if (alreadyBlocked) return;

        if (currentHits >= currentMelee.maxHitsPerSwing)
        {
            return;
        }
        else
        {
            currentHits++;
        }

        //if it was blocked just return
        if (other.gameObject.TryGetComponent<BlockCollisionDetector>(out BlockCollisionDetector detector))
        {
            anim.SetTrigger("Return");
            alreadyBlocked = true;
            AllowAttack();

            if(blockSound != null)
            {
                audioSource.PlayOneShot(blockSound);
            }

            return;
        }

        Stats statss = other.gameObject.transform.GetComponentInParent<Stats>();
        //if hit object has stats then deal damage
        if (statss)
        {
            if (other.gameObject.CompareTag("Unpenetrable"))
            {
                anim.SetTrigger("Return");
                if (unPenetrableSound != null)
                {
                    audioSource.PlayOneShot(unPenetrableSound);
                }
            }
            else
            {
                if (livingHit != null)
                {
                    audioSource.PlayOneShot(livingHit);
                }
            }


            if (currentMelee != null)
            {
                if(this is AIMeleeWeapons)
                {
                    statss.TakeDamage(currentMelee.baseDamage, transform);
                }
                else
                {
                    statss.TakeDamage(stats.damage, transform);
                }
            }
        }
        else
        {
            if(other.gameObject.CompareTag("Unpenetrable"))
            {
                anim.SetTrigger("Return");
                if (unPenetrableSound != null)
                {
                    audioSource.PlayOneShot(unPenetrableSound);
                }
            }
            else
            {
                if (nonLivingHit != null)
                {
                    audioSource.PlayOneShot(nonLivingHit);
                }
            }
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if(anim)
        {
            if(handler != null)
            {
                if (!handler.IsSheathed())
                {
                    if (handler == null) return;
                    if (handler.IsSheathed()) return;
                    if (currentMelee == null) return;
                    if (currentMelee.singleHand) return;

                    anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandHolder.position);
                }
            }
        }
    }
}
