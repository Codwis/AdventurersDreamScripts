using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestGiver : Talkable
{
    [Tooltip("Quest they give to the player")]
    public Quest questToGive;
    [Tooltip("Quest marker make sure layer is quest")]
    public MeshRenderer questMarker;

    private bool questGiven = false;
    private bool questCompleted = false;

    private bool questAnswered = true;
    private DialogueHandler dialogueHandler;

    private QuestHandler questHandler;
    private Inventory inv;

    private bool noSpace = false;

    public override void Interact(Transform source) //When player interacts with this source is player
    {
        if(controller.currentAgro != null)
        {
            Debug.Log("uhoh?1");
            return;
        }
        if(stats.faction.CheckIfEnemy(source.gameObject.layer))
        {
            return;
        }

        PlayerQuestHandler playQuestHandle = source.GetComponentInChildren<PlayerQuestHandler>();

        if(playQuestHandle.HasDoneQuest(questToGive.questName) && !questGiven)
        {
            if(questToGive.nextStep != null)
            {
                questToGive = questToGive.nextStep;
                questGiven = false;
                questCompleted = false;
                questAnswered = true;
            }
            return;
        }

        if (questGiven && questCompleted) //If quest has been given and completed do normal dialogue
        {
            base.Interact(source);
        }

        //If there is currently a question on players screen
        if (questAnswered)
        {
            
            if (questGiven && !questCompleted) //If quest is currently active
            {
                if (!TryCompletingQuest(source.GetComponentInChildren<PlayerQuestHandler>())) //Tries to complete the quest
                {
                    if (!noSpace) //If theres space and didnt complete the quest show this text
                    {
                        noSpace = false;
                        //Will say quest no done dialogue if quest didnt complete
                        source.GetComponentInChildren<DialogueHandler>().DisplayText(name, questToGive.questNotDoneDialogue);
                    }
                }
            }
            else if (!questGiven) //Quest not been given yet
            {
                //reset values
                questCompleted = false;
                
                if(questToGive.deniable)
                {
                    questAnswered = false;
                }

                //Gets the inventory and disables the equipment ui
                inv = source.GetComponentInChildren<Inventory>();
                inv.AllowUi(false);

                //Gets the dialogue handler and displays text
                dialogueHandler = source.GetComponentInChildren<DialogueHandler>();
                dialogueHandler.DisplayText(name, questToGive.questDialogue,GetComponent<Animator>(), this);
                questGiven = true;
            }
        }

        controller.SetLookOfInterest(source.GetComponent<PlayerController>().cam.transform);
    }



    private bool TryCompletingQuest(PlayerQuestHandler handler) //Tries to complete the quest
    {
        //Tests if there is room in the inventory
        if (!handler.transform.root.GetComponentInChildren<Inventory>().CanFitQuestRewards(questToGive))
        {
            handler.transform.root.GetComponentInChildren<DialogueHandler>().DisplayText(name, "Make some room for the rewards");
            noSpace = true;
            return false;
        }

        if (handler.TryCompletingQuest(questHandler)) //this mostly checks if the quest is ready to turn in
        {
            //If ready then set all the variables and show text
            questCompleted = true;
            if(textToSay != null)
            {
                textToSay = new Dialogue[1];
                textToSay[0].text = questToGive.afterQuestText;
            }

            handler.transform.root.GetComponent<PlayerController>().Save();

            handler.transform.root.GetComponentInChildren<DialogueHandler>().DisplayText(name, questToGive.questCompleteDialogue);

            //If there are rewards give them to the player
            if(questToGive.rewardItems != null)
            {
                foreach (RewardItem item in questToGive.rewardItems)
                {
                    handler.transform.root.GetComponentInChildren<Inventory>().AddItem(item.item, item.amount);
                }
            }

            if(questToGive.thingsToSpawnAfterCompletion != null)
            {
                foreach(GameObject obj in questToGive.thingsToSpawnAfterCompletion)
                {
                    Instantiate(obj);
                }
            }
            
            //If the quest has a next step
            if(questToGive.nextStep != null)
            {
                //resets the given status and sets the next quest ready to be given
                Material mat = questMarker.material;
                mat.color = Color.yellow;
                questGiven = false;
                questCompleted = false;
                questToGive = questToGive.nextStep;
                questAnswered = true;
            }
            else
            {
                questAnswered = true;
            }
            return true;
        }
        return false;
    }

    public void Answered(bool answer) //When QuestOffer gets answered
    {
        
        //allows continuation of dialogue
        questAnswered = true;
        dialogueHandler.AllowContinue();
        dialogueHandler = null;

        //Allows opening of ui
        inv.AllowUi(true);
        inv = null;

        //Resets the questgiven if player declined
        if (!answer)
        {
            questGiven = false;
        }
        else
        {
            Material mat = questMarker.material;
            mat.color = Color.white;
        }
    }

    //Sets the handler gets called from playerquest
    public void SetHandler(QuestHandler handler)
    {
        questHandler = handler;
    }
}
