using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.Animations;

public class EquipmentHandler : NetworkBehaviour
{
    [Header("GameObjects for Armor")]
    [Tooltip("Gameobject for helmet")]public GameObject helmetGpx;
    [Tooltip("Gameobject for chest")] public GameObject chestGpx;
    [Tooltip("Gameobject for spaulders")] public GameObject spaulderGpx;
    [Tooltip("Gameobject for gloves")] public GameObject glovesGpx;
    [Tooltip("Gameobject for pants")] public GameObject pantsGpx;
    [Tooltip("Gameobject for right Boot")] public GameObject rightBootGpx;
    [Tooltip("Gameobject for left boot")] public GameObject leftBootGpx;
    public GameObject capeGpx;


    [Header("Melee Related")]

    [Tooltip("Melee weapon main script in the character")]public MeleeWeapons meleeWeapons;
    [Tooltip("Place where melee is held")] public GameObject meleeHolder;
    [Tooltip("Gameobject for the melee gets replaced")] public GameObject meleeGpx;
    [Tooltip("Spot where melee will holster on the back")] public Transform meleeHolster;
    [Tooltip("Spot where single handed weapon go")] public Transform singleHandHolster;
    [NonSerialized] public MeleeWeapons meleeScript;
    private MeshCollider meleeCol;

    [Header("Bow Related")]

    [Tooltip("Gameobject which will hold the bow")] public GameObject bowHolder;
    [Tooltip("Gameobject for bow gets replaced")] public GameObject bowGpx;
    [Tooltip("Spot where bow will holster on the back")] public Transform bowHolster;

    [NonSerialized] public RangedScript rangedScript;
    private MeshCollider bowCol;

    [Header("Magic")]
    [Tooltip("GrimoireGpx")] public GameObject grimoireGpx;
    private GrimoireScript grimoireScript;

    [Header("Misc")]

    [Tooltip("Mask for the player used in other scripts")] public LayerMask playerMask;
    [Tooltip("Literally just empty gameobject")]public GameObject nothingPrefab;
    [Tooltip("Weapon for the fists")]public Weapon fists;
    [Tooltip("Players camera")]public Camera cam;
    [Tooltip("audio clip for taking out")] public AudioClip takeOutSound;

    private AudioSource audioSource;
    private Animator animator;
    private Equipment currentItem;

    private bool unequip = false;
    private bool switchWeapon = false;
    private bool sheathed = true;

    private Stats stats;
    private void Start()
    {
        rangedScript = GetComponent<RangedScript>();
        meleeScript = transform.root.GetComponent<MeleeWeapons>();
        meleeCol = meleeGpx.GetComponent<MeshCollider>();
        bowCol = bowGpx.GetComponent<MeshCollider>();
        stats = GetComponent<Stats>();
        rangedScript.SetHandler(this);
        meleeScript.SetHandler(this);

        if(!TryGetComponent<GrimoireScript>(out grimoireScript))
        {
            grimoireScript = gameObject.AddComponent<GrimoireScript>();
        }

        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        DisableOtherWeapons();
    }

    public void NormalState(int off)
    {
        if(off == 0)
        {
            animator.SetBool("Normal", true);
        }
        else
        {
            animator.SetBool("Normal", false);
        }
    }

    //Sheaths the weapon gets called middle of sheathing
    public void Sheath()
    {
        if(currentItem != null)
        {
            if (currentItem.takeOutSound != null)
            {
                audioSource.PlayOneShot(currentItem.takeOutSound);
            }

        }

        animator.ResetTrigger("PutAway");
        sheathed = true;

        if(rangedScript.currentArrow != null) //Hides the arrow
        {
            rangedScript.StopAllCoroutines();
            rangedScript.currentArrow.SetActive(false);
            rangedScript.charge = 0;
            rangedScript.bowAnim.SetFloat("Charge", 0);
        }
        if(rangedScript.currentBowScript != null) //Disables the constraint on the bow string
        {
            rangedScript.currentBowScript.ActivateConstraint(false);
        }

        //If unequipping then hide weapons
        if (unequip)
        {
            unequip = false;

            meleeHolder.SetActive(false);
            meleeGpx.SetActive(false);

            bowHolder.SetActive(false);
            bowGpx.SetActive(false);
        }
        else if (switchWeapon)
        {
            switchWeapon = false;
            if(currentItem is Ranged) //Hides the Melee Weapon
            {
                meleeHolder.SetActive(false);
                meleeGpx.SetActive(false);
                meleeScript.enabled = false;
            }
            else if(currentItem is Melee) //Hides the Ranged 
            {
                bowHolder.SetActive(false);
                bowGpx.SetActive(false);
                rangedScript.enabled = false;
            }
        }

        //Sets weapons to holster by resetting values

        if(meleeScript.currentMelee != null)
        {
            meleeScript.canSwing = false;
            if(meleeScript.currentMelee.singleHand)
            {
                meleeGpx.transform.parent = singleHandHolster;
            }
            else
            {
                meleeGpx.transform.parent = meleeHolster;
            }
            meleeGpx.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }


        bowGpx.transform.parent = bowHolster;
        bowGpx.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    //This gets called middle of draw animation For Sword
    public void DrawSword()
    {
        if (currentItem != null)
        {
            if (currentItem.takeOutSound != null)
            {
                audioSource.PlayOneShot(currentItem.takeOutSound);
            }
        }

        animator.SetBool("Sheathed", false);
        ChangeWeaponGraphics(currentItem);

        ResetTriggers();

        meleeHolder.SetActive(true);
        meleeGpx.SetActive(true);

        sheathed = false;

        if(meleeScript != null)
        {
            meleeScript.canSwing = true;
        }

        meleeGpx.transform.parent = meleeHolder.transform;
        meleeGpx.transform.localEulerAngles = currentItem.localRotation;
        meleeGpx.transform.localPosition = currentItem.localPosition;
    }

    //This gets called middle of draw animation For Bow
    public void DrawBow()
    {
        if (currentItem != null)
        {
            if (currentItem.takeOutSound != null)
            {
                audioSource.PlayOneShot(currentItem.takeOutSound);
            }
        }

        animator.SetBool("Sheathed", false);
        ChangeWeaponGraphics(currentItem);

        ResetTriggers();

        bowHolder.SetActive(true);
        bowGpx.SetActive(true);

        sheathed = false;

        bowGpx.transform.parent = bowHolder.transform;
        bowGpx.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    //Disables other scripts for weapons and all if null
    private void DisableOtherWeapons(Equipment weapon = null)
    {
        if(weapon == null)
        {
            rangedScript.enabled = false;
            meleeScript.enabled = true;
            meleeGpx.SetActive(false);
            bowGpx.SetActive(false);

            animator.SetBool("Bow", false);
            animator.SetBool("Melee", false);

            rangedScript.ChangeCurrentBow(null);
            meleeScript.ChangeCurrentWeapon(null);

            meleeScript.ui.alpha = 1;
            rangedScript.SetCrosshairAlpha(0);
        }
        else if(weapon is Ranged)
        {
            rangedScript.enabled = true;
            meleeScript.enabled = false;
            meleeGpx.SetActive(false);
            bowGpx.SetActive(true);

            animator.SetBool("Melee", false);
            animator.SetBool("Bow", true);

            meleeScript.ui.alpha = 0;
            rangedScript.SetCrosshairAlpha(1);
        }
        else if(weapon is Melee)
        {
            rangedScript.enabled = false;
            meleeScript.enabled = true;
            meleeGpx.SetActive(true);
            bowGpx.SetActive(false);

            animator.SetBool("Bow", false);
            animator.SetBool("Melee", true);
            
            meleeScript.ui.alpha = 1;
            rangedScript.SetCrosshairAlpha(0);
        }
    }
    private void ResetTriggers()
    {
        animator.ResetTrigger("PutAway");
        animator.ResetTrigger("Draw");
        animator.ResetTrigger("Block");
        animator.ResetTrigger("Swing");
    }

    //Changes current weapon to given item and can be removed
    public void ChangeWeapon(Equipment item, bool removeWeapon = false)
    {
        if(sheathed || removeWeapon)
        {
            animator.SetBool("Sheathed", false);
        }
        else if(!removeWeapon)
        {
            animator.SetBool("Sheathed", true);
        }
        currentItem = item;
        if (removeWeapon) //Puts away the weapon
        {
            DisableOtherWeapons();
            animator.SetTrigger("PutAway");
            unequip = true;

            meleeScript.ui.alpha = 1;
            rangedScript.SetCrosshairAlpha(0);
            currentItem = null;
            rangedScript.currentBow = null;
            meleeScript.currentMelee = null;
            return;
        }
        //If its ranged type set variables for it
        else if (item is Ranged ranged)
        {
            DisableOtherWeapons(item);
            if(rangedScript.currentArrow != null)
            {
                Destroy(rangedScript.currentArrow);
            }

            //If Weapon sheathed then take it out
            if (sheathed)
            {
                animator.SetTrigger("Draw");
            }
            else //Switches weapons
            {
                switchWeapon = true;
                animator.SetTrigger("PutAway"); //Puts away current weapon
                animator.SetTrigger("Draw"); //Sets trigger for the bow
            }

            rangedScript.ChangeCurrentBow(ranged);
        }
        //Basically same as ranged sets variables to melee
        else if (item is Melee melee)
        {
            DisableOtherWeapons(item);

            if(melee.singleHand)
            {
                animator.SetBool("Singlehand", true);
            }
            else
            {
                animator.SetBool("Singlehand", false);
            }

            if (sheathed)
            {
                animator.SetTrigger("Draw");
            }
            else //Switches weapons
            {
                switchWeapon = true;

                animator.SetTrigger("PutAway");
                animator.SetTrigger("Draw");
            }

            meleeWeapons.ChangeCurrentWeapon(melee);
        }
        else //Removes the weapon and changes to fists
        {
            animator.SetBool("Singlehand", false);

            meleeHolder.SetActive(false);
            bowHolder.SetActive(false);

            meleeScript.ui.alpha = 1;
            rangedScript.SetCrosshairAlpha(0);

            meleeWeapons.ChangeCurrentWeapon((Melee)fists);
        }
    }

    //Changes the graphics to the new weapon
    public void ChangeWeaponGraphics(Equipment weapon)
    {
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        if (weapon.prefab == null) //Makes sure there is prefab
        {
            return;
        }
        MeshRenderer meshTemp = weapon.prefab.GetComponent<MeshRenderer>();
        MeshFilter filterTemp = weapon.prefab.GetComponent<MeshFilter>();

        //Changes the correct weapon graphics to the new graphics
        if (weapon is Ranged)
        {
            Transform parent = bowHolder.transform;
            //gets the parent removes old one and makes new one and sets collider and script
            Destroy(bowGpx);
            bowGpx = Instantiate(weapon.prefab, weapon.prefab.transform.localPosition, weapon.prefab.transform.localRotation, parent);
            bowGpx.transform.localScale = weapon.localScale;
            bowGpx.GetComponentInChildren<Collider>().gameObject.layer = gameObject.layer;
            rangedScript.ChangeCurrentBowScript(bowGpx.GetComponent<BowGpxScript>());
        }
        else
        {
            //gets the parent removes old one and makes new one and sets collider
            Transform parent = meleeGpx.transform.parent;
            meshRenderer = meleeGpx.GetComponent<MeshRenderer>();
            meshFilter = meleeGpx.GetComponent<MeshFilter>();

            meleeCol.enabled = false;
            meleeCol.sharedMesh = weapon.prefab.GetComponent<MeshCollider>().sharedMesh;

            meleeGpx.transform.localScale = weapon.localScale;
            meleeScript.meleeCollider = meleeCol;
            meshFilter.sharedMesh = filterTemp.sharedMesh;
            meshRenderer.sharedMaterials = meshTemp.sharedMaterials;
        }
    }

    //Is bow currently equipped
    public bool IsBowEquipped()
    {
        return rangedScript.enabled;
    }

    //Is Weapon currently sheathed
    public bool IsSheathed()
    {
        return sheathed;
    }
    public bool IsWeaponEquipped()
    {
        if (sheathed) return true;
        if (!meleeScript.enabled && !rangedScript.enabled) return false;
        return true;
    }

    //Changes equipment currently armor or weapon
    public void ChangeEquipment(Equipment equipment, int itemNum, bool removeEquipment = false)
    {
        Transform parent;
        GameObject temp = meleeGpx;
        //if piece gets removed
        if (removeEquipment)
        {

            switch (equipment.equipmentType)
            {
                //For weapon
                case EquipmentType.weapon:
                    if (equipment is Weapon wep) //Add armor
                    {
                        stats.AddDamage(-wep.baseDamage, EquipmentRarityController.rarities[itemNum]);
                    }
                    ChangeWeapon(equipment, removeEquipment);
                    return;

                //all different options for armor
                case EquipmentType.helmet:
                    parent = helmetGpx.transform.parent;
                    Destroy(helmetGpx);
                    temp = Instantiate(nothingPrefab, parent);
                    helmetGpx = temp;
                    break;

                case EquipmentType.chest:
                    parent = chestGpx.transform.parent;
                    Destroy(chestGpx);
                    temp = Instantiate(nothingPrefab, parent);
                    chestGpx = temp;
                    break;

                case EquipmentType.spaulder:
                    parent = spaulderGpx.transform.parent;
                    Destroy(spaulderGpx);
                    temp = Instantiate(nothingPrefab, parent);
                    spaulderGpx = temp;
                    break;

                case EquipmentType.gloves:
                    parent = glovesGpx.transform.parent;
                    Destroy(glovesGpx);
                    temp = Instantiate(nothingPrefab, parent);
                    glovesGpx = temp;
                    break;

                case EquipmentType.pants:
                    parent = pantsGpx.transform.parent;
                    Destroy(pantsGpx);
                    temp = Instantiate(nothingPrefab, parent);
                    pantsGpx = temp;
                    break;

                case EquipmentType.boots:
                    parent = rightBootGpx.transform.parent;
                    Destroy(rightBootGpx);
                    temp = Instantiate(nothingPrefab, parent);
                    rightBootGpx = temp;

                    parent = leftBootGpx.transform.parent;
                    Destroy(leftBootGpx);
                    leftBootGpx = Instantiate(nothingPrefab, parent);

                    break;
                case EquipmentType.cape:
                    parent = capeGpx.transform.parent;
                    Destroy(capeGpx);
                    capeGpx = Instantiate(nothingPrefab, parent);
                    break;

                case EquipmentType.grimoire:
                    parent = grimoireGpx.transform.parent;
                    Destroy(grimoireGpx);
                    grimoireGpx = Instantiate(nothingPrefab, parent);
                    grimoireScript.UnEquip();
                    break;
            }

            if (equipment is ArmorItem armor) //Add armor
            {
                stats.AddArmor(-armor.baseArmor, EquipmentRarityController.rarities[itemNum]);
            }
            return;
        }


        //If equipment gets set
        switch (equipment.equipmentType)
        {
            //For weapon
            case EquipmentType.weapon:

                if (equipment is Weapon wep) //Add armor
                {
                    if(EquipmentRarityController.rarities.TryGetValue(itemNum, out Rarity ra))
                    {
                        stats.AddDamage(wep.baseDamage, ra);
                    }
                    else
                    {
                        ra = EquipmentRarityController.instance.GetRarity(itemNum);
                        stats.AddDamage(wep.baseDamage, ra);
                    }

                }
                ChangeWeapon(equipment);
                return;
                

            //For all armor types
            case EquipmentType.helmet:
                parent = helmetGpx.transform.parent;
                Destroy(helmetGpx);
                temp = Instantiate(equipment.prefab);
                helmetGpx = temp;
                break;

            case EquipmentType.chest:
                parent = chestGpx.transform.parent;
                Destroy(chestGpx);
                temp = Instantiate(equipment.prefab, parent);
                chestGpx = temp;
                break;

            case EquipmentType.spaulder:
                parent = spaulderGpx.transform.parent;
                Destroy(spaulderGpx);
                temp = Instantiate(equipment.prefab, parent);
                spaulderGpx = temp;
                break;

            case EquipmentType.gloves:
                parent = glovesGpx.transform.parent;
                Destroy(glovesGpx);
                temp = Instantiate(equipment.prefab, parent);
                glovesGpx = temp;
                break;

            case EquipmentType.pants:
                parent = pantsGpx.transform.parent;
                Destroy(pantsGpx);
                temp = Instantiate(equipment.prefab, parent);
                pantsGpx = temp;
                break;

            case EquipmentType.boots:

                parent = rightBootGpx.transform.parent;
                Destroy(rightBootGpx);
                temp = Instantiate(equipment.prefab, parent);
                rightBootGpx = temp;

                //parent = leftBootGpx.transform.parent;
                //Destroy(leftBootGpx);
                //leftBootGpx = Instantiate(equipment.otherBoot, parent);
                break;
            case EquipmentType.cape:
                parent = capeGpx.transform.parent;
                Destroy(capeGpx);
                temp = Instantiate(equipment.prefab, parent);
                capeGpx = temp;
                break;

            case EquipmentType.grimoire:
                parent = grimoireGpx.transform.parent;
                Destroy(grimoireGpx);
                temp = Instantiate(equipment.prefab, parent);
                temp.transform.SetLocalPositionAndRotation(equipment.localPosition, Quaternion.Euler(equipment.localRotation));
                temp.transform.localScale = equipment.localScale;
                audioSource.PlayOneShot(equipment.takeOutSound);

                grimoireGpx = temp;
                grimoireScript.Equip((Grimoire)equipment, grimoireGpx.GetComponent<SkinnedMeshRenderer>());
                break;
        }

        if (equipment is ArmorItem armo) //Add armor
        {
            if(EquipmentRarityController.rarities.TryGetValue(itemNum, out Rarity r))
            {
                stats.AddArmor(armo.baseArmor, r);
            }
            else
            {
                r = EquipmentRarityController.instance.GetRarity(itemNum);
                stats.AddArmor(armo.baseArmor, r);
            }

            if(temp.GetComponent<ArmorPutOn>())
            {
                temp.GetComponent<ArmorPutOn>().PutOn(animator, armo);
            }

        }
    }




    //Checks if something is infront
    public float CheckForwardDistance(float maxDistance)
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, maxDistance, playerMask))
        {
            return maxDistance - hit.distance;
        }
        else
        {
            return 0;
        }
    }

    //Sets melee script
    public void SetMeleeWeapons(MeleeWeapons meleeWp)
    {
        meleeWeapons = meleeWp;
    }

    //Sets ranged script
    public void SetRangedScript(RangedScript bow)
    {
        rangedScript = bow;
    }

    }
public enum EquipmentType { helmet, chest, spaulder, gloves, pants, boots, weapon, grimoire, cape, bag}

