using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerQuestHandler : MonoBehaviour
{
    // ADD COMMENTS
    public CanvasGroup questOfferGroup;
    public Text questNameText;
    public Button yesButton;
    public Button noButton;

    public List<string> questsDone;
    private QuestGiver currentGiver;
    private bool playerPressed = false;
    private bool playerResponse;

    public GameObject questPrefab;
    public List<QuestHandler> currentQuests = new List<QuestHandler>();
    private List<AIInfo> currentEnemiesNeeded = new List<AIInfo>();
    private List<Item> currentItemsNeeded = new List<Item>();
    private RectTransform rectTransform;

    public void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        yesButton.onClick.AddListener(() => PlayerResponse(true));
        noButton.onClick.AddListener(() => PlayerResponse(false));
    }

    public bool EarlyQuestStart(Quest newQuest, QuestGiver giver)
    {
        currentGiver = giver;

        if (!newQuest.deniable)
        {
            AddQuest(newQuest);
            return false;
        }
        StartCoroutine(ShowQuestOffer(newQuest));
        return true;
    }

    public void AddQuest(Quest newQuest)
    {
        GameObject temp = Instantiate(questPrefab, transform);
        QuestHandler handler = temp.GetComponent<QuestSetup>().Setup(newQuest);
        if(currentGiver != null)
        {
            currentGiver.SetHandler(handler);
        }
        currentQuests.Add(handler);

        if (newQuest.itemsNeeded.Count > 0)
        {
            foreach (ItemTask i in newQuest.itemsNeeded)
            {
                currentItemsNeeded.Add(i.taskItem);
            }
        }
        if (newQuest.enemiesKilledNeeded.Count > 0)
        {
            foreach (KillTask i in newQuest.enemiesKilledNeeded)
            {
                currentEnemiesNeeded.Add(i.enemyToKill);
            }
        }
    }

    private void PlayerResponse(bool response) //Sets which player selected
    {
        if(response)
        {
            GetComponentInParent<Animator>().Play("accept");
        }
        else
        {
            GetComponentInParent<Animator>().Play("decline");
        }
        playerResponse = response;
        playerPressed = true;
    }

    private IEnumerator ShowQuestOffer(Quest newQuest) //Shows the quest offer
    {
        //Displays the quests name and resets input
        questNameText.text = newQuest.questName;
        playerPressed = false;

        //Shows the Ui
        questOfferGroup.alpha = 1;
        questOfferGroup.interactable = true;
        questOfferGroup.blocksRaycasts = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;

        while (!playerPressed) //While player hasnt pressed do nothing
        {
            Cursor.visible = true;
            yield return null;
        }
        
        //hides again the cursor because the other ui is automatically hidden
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Inventory.instance.AllowUi(true);

        //also hides the quest offer
        questOfferGroup.alpha = 0;
        questOfferGroup.interactable = false;
        questOfferGroup.blocksRaycasts = false;

        //Tells the giver it has been answered and can continue
        currentGiver.Answered(playerResponse);
        if (playerResponse) //if answer was yes then add the new quest
        {
            AddQuest(newQuest);
        }
    }

    public void TryProceedingQuest(AIInfo info = null, Item item = null, Quest questToComplete = null)
    {
        if(info != null)
        {
            if (currentEnemiesNeeded.Contains(info))
            {
                for (int i = 0; i < currentQuests.Count; i++)
                {
                    for (int ii = 0; ii < currentQuests[i].quest.enemiesKilledNeeded.Count; ii++)
                    {
                        if (currentQuests[i].quest.enemiesKilledNeeded[ii].enemyToKill == info)
                        {
                            if (currentQuests[i].ProgressEnemy(info))
                            {
                                if(currentEnemiesNeeded.Count > 1)
                                    i--;
                                currentEnemiesNeeded.Remove(info);
                                break;
                            }
                        }
                    }
                }
            }
        }

        if(item != null)
        {
            if (currentItemsNeeded.Contains(item))
            {
                for(int i = 0; i < currentQuests.Count; i++)
                {
                    for(int ii = 0; ii < currentQuests[i].quest.itemsNeeded.Count; ii++)
                    {
                        if (currentQuests[i].quest.itemsNeeded[ii].taskItem == item)
                        {
                            currentQuests[i].ProgressItem(item);
                        }
                    }
                }
            }
        }

        if(questToComplete != null)
        {
            questsDone.Add(questToComplete.questName);
            CompleteAmbush(questToComplete);
        }
    }

    public void CompleteAmbush(Quest questComplete)
    {
        for(int i = 0; i < currentQuests.Count; i++)
        {
            if (currentQuests[i].quest == questComplete)
            {
                Destroy(currentQuests[i].gameObject);
                currentQuests.RemoveAt(i);
                return;
            }
        }
    }
    public bool TryCompletingQuest(QuestHandler handler)
    {
        if(handler != null)
        {
            if (handler.CheckQuestDone())
            {
                questsDone.Add(handler.quest.questName);

                Destroy(handler.gameObject);
                currentQuests.Remove(handler);

                return true;
            }
        }
        return false;
    }
    

    private Vector3 offset;
    public void GetPointerDown() //When pointer clicks down it gets offset to the mouse
    {
        offset = rectTransform.position - Input.mousePosition;
    }
    public void DragBag() //Moves the quest menu when gameObject is dragged around
    {
        //Calculates the proper spot and wont snap onto the mouse because offset
        Vector3 pos = Input.mousePosition - rectTransform.position + offset;
        rectTransform.position += pos;

        //Clamps UI withing the screen
        rectTransform.position = new Vector3(Mathf.Clamp(rectTransform.position.x, 0, Screen.width), Mathf.Clamp(rectTransform.position.y, rectTransform.rect.height / 6, Screen.height + rectTransform.rect.height / 6));
    }

    public void LoadData(QuestData data)
    {
        if (data == null) return;

        if(data.questsDone != null)
        {
            foreach (string st in data.questsDone)
            {
                questsDone.Add(st);
            }
        }

    }
    public bool HasDoneQuest(string questName)
    {
        if(questsDone.Contains(questName))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
