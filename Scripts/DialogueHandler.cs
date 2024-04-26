using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

public class DialogueHandler : MonoBehaviour
{

    [Header("Text Boxes")]

    [Tooltip("Where is talkers name written")]public Text talkerNameBox;
    [Tooltip("Main dialogue box where text shows")] public Text dialogueBox;
    private CanvasGroup dialogueBoxGroup;

    [Header("Misc")]

    [Tooltip("Things to say when npc has no quests")] public DialogueData dialogueData;
    private Dictionary<DialogueType, string[]> basicDialogues = new Dictionary<DialogueType, string[]>();

    private bool talking = false;
    private Dialogue[] currentText;
    private int currentIndex = 0;

    private bool canContinue = true;
    private bool quest = false;

    private QuestGiver questGiver;
    private Animator talkAnimator;

    public static DialogueHandler instance;
    private void Awake()
    {
        if (instance == null) instance = this;
    }
    private void Start()
    {
        //Loads up dialogue and gets canvas group
        LoadDialogue();
        dialogueBoxGroup = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        float input = Input.GetAxisRaw("Attack");
        //If player presses mousebutton or E during dialogue it shows next line
        if(input != 0 && currentText != null && talking && canContinue)
        {
            NextLine();
        }
    }

    //Is someone talking to the player
    public bool IsTalking()
    {
        return talking;
    }

    //Just loads up dialogue from data to a dictionary
    private void LoadDialogue()
    {
        foreach(DialogueEntry entry in dialogueData.entries)
        {
            basicDialogues.Add(entry.dialogueType, entry.dialogueText);
        }
    }

    //If there is specific dialogue needed to play
    public void DisplayText(string talkerName, Dialogue[] text, Animator animator = null, QuestGiver giver = null)
    {
        talkAnimator = animator;
        if (talking) return;

        if (giver != null)
        {
            questGiver = giver;
            quest = true;
        }

        if (text[0].customName != "")
        {
            talkerNameBox.text = text[0].customName;
        }
        else
        {
            talkerNameBox.text = talkerName;
        }

        //sets variables and shows dialogue box
        talking = true;

        currentIndex = 0;
        currentText = text;

        dialogueBoxGroup.alpha = 1;
        dialogueBoxGroup.interactable = true;
        dialogueBoxGroup.blocksRaycasts = true;

        NextLine(true);
    }

    //If there is specific dialogue needed to play
    public void DisplayText(string talkerName, string text, QuestGiver giver = null)
    {
        if (talking) return;

        if(giver != null)
        {
            questGiver = giver;
            quest = true;
        }

        talkerNameBox.text = talkerName;

        dialogueBoxGroup.alpha = 1;
        dialogueBoxGroup.interactable = true;
        dialogueBoxGroup.blocksRaycasts = true;

        talking = true;

        currentIndex = 0;
        currentText = new Dialogue[1];
        currentText[0].text = text;
        NextLine(true);
    }

    //Writes the next line available
    private void NextLine(bool first = false)
    {
        if(first)
        {
            MeleeWeapons we = GetComponentInParent<MeleeWeapons>();
            if(we.currentMelee != null)
            {
                if(!we.handler.IsSheathed())
                {
                    we.anim.SetTrigger("PutAway");
                    we.anim.SetBool("Sheathed", true);
                }
            }
        }
        GetComponentInParent<MeleeWeapons>().canSwing = false;
        //empties box and tests if laast line
        dialogueBox.text = "";
        if (currentIndex > currentText.Length - 1)
        {
            GetComponentInParent<MeleeWeapons>().canSwing = true;
            talking = false;
            dialogueBoxGroup.alpha = 0;
            dialogueBoxGroup.interactable = false;
            dialogueBoxGroup.blocksRaycasts = false;
            if(questGiver != null)
            {
                PlayerQuestHandler handler;
                handler = transform.root.GetComponentInChildren<PlayerQuestHandler>();
                if(!handler.EarlyQuestStart(questGiver.questToGive, questGiver))
                {
                    Inventory.instance.AllowUi(true);
                }
                if(questGiver.TryGetComponent<AIController>(out AIController controller))
                {
                    controller.RemoveLookOfInterest();
                }

                talkAnimator = null;
                questGiver = null;
            }

            Cursor.visible = false;
            return;
        }

        if (currentText[currentIndex].customName != "")
        {
            if(currentText[currentIndex].customName != talkerNameBox.text)
            {
                talkerNameBox.text = currentText[currentIndex].customName;
            }
        }

        bool canDisplay = false;
        if (currentText[currentIndex].requireQuest)
        {
            PlayerQuestHandler questHand = Inventory.instance.GetComponentInChildren<PlayerQuestHandler>();
            foreach (QuestHandler handle in questHand.currentQuests)
            {
                if (handle.quest == currentText[currentIndex].questNeeded)
                {
                    canDisplay = true;
                    break;
                }
                canDisplay = false;
            }
            if (!canDisplay) currentIndex++;
        }

        if (currentText[currentIndex].thingToSummon != null)
        {
            GameObject obj = Instantiate(currentText[currentIndex].thingToSummon, null);

            PlayerQuestHandler handler = transform.root.GetComponentInChildren<PlayerQuestHandler>();
            if (obj.TryGetComponent<SquadClearHandler>(out SquadClearHandler cl))
            {
                if (cl.quest)
                {
                    cl.ambushQuest = handler.currentQuests[handler.currentQuests.Count - 1].quest;
                }
            }
        }

        if (currentText[currentIndex].itemToGive != null)
        {
            if (currentText[currentIndex].itemToGive.item != null)
            {
                Inventory inv = GetComponentInParent<Inventory>();
                inv.AddItem(currentText[currentIndex].itemToGive, 1);
            }
        }

        if (currentText[currentIndex].reactionName != null)
        {
            if (currentText[currentIndex].reactionName != "")
            {
                talkAnimator.Play(currentText[currentIndex].reactionName, talkAnimator.layerCount - 1);
            }
        }

        //Writes the current line
        canContinue = false;
        StartCoroutine(WriteText(currentText[currentIndex].text));
        currentIndex++;
    }

    //For when no specific dialogue
    public void DisplayText(string talkerName, DialogueType typeOfTalk)
    {
        if (talking) return;

        //sets variables and gets random dialogue
        talkerNameBox.text = talkerName;
        talking = true;
        string text = GetDialogue(typeOfTalk);

        dialogueBoxGroup.alpha = 1;
        dialogueBoxGroup.interactable = true;
        dialogueBoxGroup.blocksRaycasts = true;

        //If text has any stars replace them with talkers name
        if (text.Contains("*"))
        {
            string[] temp = text.Split("*");
            text = "";
            foreach (string str in temp)
            {
                text += str;
                if (str != temp[temp.Length - 1])
                    text += talkerName;
            }
        }

        currentIndex = 0;
        currentText = new Dialogue[1];
        currentText[0].text = text;

        NextLine(true);
    }

    //Gets called when player is given a choice
    public void AllowContinue()
    {
        quest = false;
        NextLine();
    }

    //Gets a random dialogue to use with the given type
    private string GetDialogue(DialogueType type)
    {
        if(basicDialogues.TryGetValue(type,out string[] values))
        {
            string final = values[UnityEngine.Random.Range(0, values.Length - 1)];
            return final;
        }
        else
        {
            return "...";
        }
    }

    //Writes the given text one character at a time
    private IEnumerator WriteText(string text)
    {
        char[] textSplit = text.ToCharArray();
        for(int i = 0; i < textSplit.Length; i++)
        {
            dialogueBox.text += textSplit[i];
            yield return new WaitForEndOfFrame(); //Adds small delay
        }
        canContinue = true;
    }
}

[Serializable]
public struct Dialogue
{
    public string text;
    public ItemInSlot itemToGive;
    public string reactionName;

    public string customName;
    public bool requireQuest;
    [Tooltip("if it needs quest to show this dialogue")] public Quest questNeeded;
    [SerializeField] public GameObject thingToSummon;
}
public enum DialogueType { Introductions, Greetings, SmallTalk, DeniedQuest, PositiveDeath }

