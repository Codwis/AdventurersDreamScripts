using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Stats : MonoBehaviour
{
    [NonSerialized] public bool alive = true;

    [NonSerialized] public float health;
    [NonSerialized] public float currentStamina;

    [Header("MaxAmounts")]
    [Tooltip("Max health possible may be increased in code")] public float maxHealth;
    [Tooltip("Same as health but for stamina")] public float maxStamina;

    [Header("Bars")]
    [Tooltip("This stats owners healthbar")] public Slider healthBar;
    private Text healthText;
    [Tooltip("Same as health but staminabar not for enemies maybe")]public Slider staminaBar;
    private Text staminaText;

    [Header("Stats texts")]
    [Tooltip("Where amount of damage is displayed")]public Text damageText;
    [Tooltip("Where amount of armor is displayed")] public Text armorText;

    public float armor;
    public float damage;

    [Header("Misc")]
    [Tooltip("Which faction belong to")] public Faction faction;
    [Tooltip("Jump delay between jumps")] public float jumpDelay;

    public bool chosen = false;

    public List<AIController> summons = new List<AIController>();

    private NetworkAnimator networkAnim;

    private bool canRegenStamina;
    private const float staminaRegenDelay = 2.5f;

    public bool innocent = false;
    public bool dissapear = false;
    public virtual void Start()
    {
        TryGetComponent<NetworkAnimator>(out networkAnim);

        if(Gamemanager.newGame && gameObject.layer == LayerMask.NameToLayer("Player") && GetComponentInParent<PlayerController>())
        {
            PlayerPrefs.SetFloat("Damage", 0);
            PlayerPrefs.SetFloat("Health", maxHealth);
            PlayerPrefs.SetFloat("Stamina", maxStamina);

            currentStamina = PlayerPrefs.GetFloat("Stamina");
            health = PlayerPrefs.GetFloat("Health");

            PlayerPrefs.Save();
        }
        else if(!PlayerPrefs.HasKey("Stamina") && gameObject.layer == LayerMask.NameToLayer("Player") && GetComponentInParent<PlayerController>())
        {
            PlayerPrefs.SetFloat("Damage", 0);
            PlayerPrefs.SetFloat("Health", maxHealth);
            PlayerPrefs.SetFloat("Stamina", maxStamina);
            currentStamina = PlayerPrefs.GetFloat("Stamina");
            health = PlayerPrefs.GetFloat("Health");
            PlayerPrefs.Save();
        }
        else if(PlayerPrefs.HasKey("Stamina") && gameObject.layer == LayerMask.NameToLayer("Player") && GetComponentInParent<PlayerController>())
        {
            currentStamina = PlayerPrefs.GetFloat("Stamina");
            health = PlayerPrefs.GetFloat("Health");
        }
        else
        {
            currentStamina = maxStamina;
            health = maxHealth;
        }

        if(staminaBar != null)
        {
            staminaText = staminaBar.GetComponentInChildren<Text>();
            healthText = healthBar.GetComponentInChildren<Text>();

            staminaText.text = Mathf.RoundToInt(currentStamina) + "/" + PlayerPrefs.GetFloat("Stamina");
            healthText.text = Mathf.RoundToInt(health) + "/" + PlayerPrefs.GetFloat("Health");
        }
    }

    private void Update()
    {
        if(gameObject.layer == LayerMask.NameToLayer("Player") && GetComponentInParent<PlayerController>())
        {
            if (canRegenStamina && currentStamina < PlayerPrefs.GetFloat("Stamina"))
            {
                if (gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    currentStamina += PlayerPrefs.GetFloat("Stamina") * 0.15f * Time.deltaTime;
                }
                else
                {
                    currentStamina += maxStamina * 0.15f * Time.deltaTime;
                }

                if (staminaBar != null)
                {
                    staminaBar.value = currentStamina / PlayerPrefs.GetFloat("Stamina");
                    staminaText.text = Mathf.RoundToInt(currentStamina) + "/" + PlayerPrefs.GetFloat("Stamina");
                }
            }
        }
        else
        {
            if (canRegenStamina && currentStamina < maxStamina)
            {
                if (gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    currentStamina += PlayerPrefs.GetFloat("Stamina") * 0.15f * Time.deltaTime;
                }
                else
                {
                    currentStamina += maxStamina * 0.15f * Time.deltaTime;
                }

                if (staminaBar != null)
                {
                    staminaBar.value = currentStamina / PlayerPrefs.GetFloat("Stamina");
                    staminaText.text = Mathf.RoundToInt(currentStamina) + "/" + PlayerPrefs.GetFloat("Stamina");
                }
            }
        }

    }

    //Add Armor amount to stats and scale it with rarity
    public void AddArmor(int amount, Rarity rarity)
    {
        if(rarity == Rarity.Common)
        {
            armor += amount * 1f;
        }
        else if(rarity == Rarity.Uncommon)
        {
            armor += amount * 1.25f;
        }
        else if (rarity == Rarity.Rare)
        {
            armor += amount * 1.5f;
        }
        else if (rarity == Rarity.Legendary)
        {
            armor += amount * 2f;
        }

        if(armorText != null)
            armorText.text = "Armor: " + armor;
    }

    //Add weapons damage to stats and scale it with rarity
    public void AddDamage(int amount, Rarity rarity)
    {
        if (rarity == Rarity.Common)
        {
            damage += amount * 1f;
        }
        else if (rarity == Rarity.Uncommon)
        {
            damage += amount * 1.25f;
        }
        else if (rarity == Rarity.Rare)
        {
            damage += amount * 1.5f;
        }
        else if (rarity == Rarity.Legendary)
        {
            damage += amount * 2f;
        }

        if(damageText != null)
        {
            damageText.text = "Damage: " + (damage + PlayerPrefs.GetFloat("Damage"));
        }
    }

    public void UpdateValues()
    {
        healthText.text = Mathf.RoundToInt(health) + "/" + PlayerPrefs.GetFloat("Health");
        staminaText.text = Mathf.RoundToInt(currentStamina) + "/" + PlayerPrefs.GetFloat("Stamina");
    }

    //Checks if enough stamina for given float
    public bool EnoughStaminaFor(float staminaUsage)
    {
        if(staminaUsage >= currentStamina)
        {
            return false;
        }

        return true;
    }

    //Drains given amount of stamina
    public void DrainStamina(float staminaUsage)
    {
        currentStamina -= staminaUsage;

        if (staminaBar != null)
        {
            staminaBar.value = currentStamina / PlayerPrefs.GetFloat("Stamina");
            staminaText.text = Mathf.RoundToInt(currentStamina) + "/" + PlayerPrefs.GetFloat("Stamina");
        }

        StopAllCoroutines();
        StartCoroutine(StartStaminaTimer());
    }

    //Waits stamina regendelay then allows stamina regen
    public IEnumerator StartStaminaTimer()
    {
        canRegenStamina = false;
        yield return new WaitForSeconds(staminaRegenDelay);
        canRegenStamina = true;
    }

    //Takes damage Add armor functionality
    public void TakeDamage(float dmg, Transform source)
    {
        if(chosen)
        {
            source.GetComponent<Stats>().TakeDamage(dmg, transform);
            if(gameObject.name == "Fini")
            {
                DialogueHandler.instance.DisplayText("Fini", "Hehheh no i dont think sou YOU cant hurt me..");
            }

            return;
        }

        //Checks if alive so doesnt continue
        if (!alive) return;
        
        if(summons.Count > 0)
        {
            foreach(AIController i in summons)
            {
                i.Agro(source);
            }
        }

        if(innocent)
        {
            DialogueHandler.instance.DisplayText(name, "HELP!!");
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, 40, Vector3.one);
            if(hits != null)
            {
                foreach(RaycastHit hit in hits)
                {
                    if (hit.collider.GetComponentInParent<Stats>() == this) continue;
                    AIController temp = hit.collider.GetComponentInParent<AIController>();
                    if(temp != null)
                    {
                        hit.collider.GetComponentInParent<AIController>().Agro(source);
                    }

                }
            }
        }
        //If damage is less than 0 that means heal
        if(dmg < 0)
        {
            if(gameObject.layer != LayerMask.NameToLayer("Player"))
            {
                if (health - dmg > maxHealth)
                {
                    health = maxHealth;
                }
                else
                {
                    health -= dmg;
                }
            }
            else
            {
                if (health - dmg > PlayerPrefs.GetFloat("Health"))
                {
                    health = PlayerPrefs.GetFloat("Health");
                }
                else
                {
                    health -= dmg;
                }
            }


            if (healthBar != null)
            {
                healthBar.value = health / PlayerPrefs.GetFloat("Health");
                healthText.text = Mathf.RoundToInt(health) + "/" + PlayerPrefs.GetFloat("Health");
            }
            return;
        }

        //if theres too much armor limit damage to 1
        if(dmg - armor <= 0)
        {
            health -= 1;
        }
        else //Else just remove it
        {
            health -= (dmg - armor);
        }

        //Makes sure melee collider is off
        MeleeCollisionDetector meCoDe = GetComponentInChildren<MeleeCollisionDetector>();
        if(meCoDe)
        {
            meCoDe.StopCollider();
        }
        //Allows attack and block
        TryGetComponent<MeleeWeapons>(out MeleeWeapons we);
        if (we != null)
        {
            we.AllowAttack();
            we.AllowBlock();
        }

        //Change healthbar value if one is present
        if (healthBar != null)
        {
            healthBar.value = health / PlayerPrefs.GetFloat("Health");
            healthText.text = Mathf.RoundToInt(health) + "/" + PlayerPrefs.GetFloat("Health");
        }

        //Play hit animator
        if(TryGetComponent<Animator>(out Animator anim))
        {
            anim.Play("hit");
        }

        //Checks if ai and makes source a target
        if(TryGetComponent<AIController>(out AIController aiCont))
        {
            if (aiCont.transform.GetComponent<Stats>())
            {
                if(source.gameObject.layer != gameObject.layer) //also checks not same faction
                {
                    aiCont.Agro(source);
                    //StopAllCoroutines();
                    //StartCoroutine(RemoveEnemy(source.gameObject.layer));
                }
            }
        }

        //If dies
        if (health <= 0)
        {


            ContainerUnit unit;
            if(aiCont != null) //If ai dies try see if it was player and proceed quest
            {

                if (source.root.TryGetComponent<PlayerController>(out _))
                {
                    aiCont.SendMessage("Dead", transform); //Send message to all so they remove this from target

                    source.root.GetComponentInChildren<PlayerQuestHandler>().TryProceedingQuest(aiCont.info);
                }

                if (dissapear)
                {
                    Destroy(gameObject);
                    return;
                }

                unit = gameObject.AddComponent<ContainerUnit>();
                unit.SetRandomItems(aiCont.info);
                if(aiCont.aiRange != null)
                {
                    unit.AddItem(aiCont.aiRange.currentBow);
                }
                else if(aiCont.aiMelee != null)
                {
                    unit.AddItem(aiCont.aiMelee.currentMelee);
                }
                


                //If in squad add dead amount so it can be cleared
                transform.root.TryGetComponent<SquadClearHandler>(out SquadClearHandler scl);
                if (scl != null)
                {
                    scl.dead++;
                }

                //Delete talkable
                if (TryGetComponent<Talkable>(out Talkable a))
                {
                    Destroy(a);
                }

                //Destroy aicontroller
                Destroy(aiCont);
            }
            else if(TryGetComponent<PlayerController>(out PlayerController controller)) //Else if its a player
            {
                Destroy(controller);
            }

            if (networkAnim != null) Destroy(networkAnim);

            alive = false;
            Destroy(anim);
            GetComponent<RagdollController>().EnableRagdoll();
        }
    }

    //Could be used to remove enemy after a delay from the faction
    private const int enemyRemoveTime = 200;
    private IEnumerator RemoveEnemy(int layer)
    {
        yield return new WaitForSeconds(enemyRemoveTime);
        faction.RemoveEnemy(layer);
    }
}
