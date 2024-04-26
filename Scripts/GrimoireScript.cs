using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GrimoireScript : MonoBehaviour
{
    public Grimoire currentGrimoire;
    public Spell currentSpellSelected;

    public SkinnedMeshRenderer skinnedRenderer;
    public Transform grimoire;
    public MouseInputs[] currentInputs;

    private int count = 0;

    private void Update()
    {
        if (currentGrimoire == null || currentGrimoire.spells == null) return;

        if (Input.GetKeyDown((KeyCode)PlayerPrefs.GetInt("Grimoire")))
        {
            if (currentSpellSelected == null)
            {
                if (currentGrimoire.spells[0] != null)
                {
                    currentSpellSelected = currentGrimoire.spells[0];
                }
            }


            if (currentSpellSelected != null)
            {
                currentInputs = new MouseInputs[currentSpellSelected.mouseMovements.Length];
            }
            else
            {
                currentInputs = new MouseInputs[5];
            }

            StopAllCoroutines();
            StartCoroutine(OpenCloseBook(true));
            count = 0;

        }
        else if(Input.GetKeyUp((KeyCode)PlayerPrefs.GetInt("Grimoire")))
        {
            StopAllCoroutines();
            StartCoroutine(OpenCloseBook(false));
        }

        if (currentGrimoire.elementTypes == null) return;
        if (currentGrimoire.elementTypes.Count == 0) return;

        if (Input.GetKey((KeyCode)PlayerPrefs.GetInt("Grimoire")))
        {
            //Add animation to split 
            if (count >= currentInputs.Length) 
            {
                count = 0;
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                currentInputs[count] = MouseInputs.Up;
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                currentInputs[count] = MouseInputs.Right;
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                currentInputs[count] = MouseInputs.Down;
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                currentInputs[count] = MouseInputs.Left;

            }
            else
            {
                return;
            }
            CheckForSpell();

            if(spelled)
            {
                spelled = false;
                return;
            }
            count++;
        }
    }

    bool first = true;
    int openNum = 0;
    private IEnumerator OpenCloseBook(bool open)
    {
        if(!Gamemanager.instance.globalAudio.isPlaying)
        {
            Gamemanager.instance.globalAudio.PlayOneShot(currentGrimoire.openSound);
        }

        if(open)
        {
            while(openNum < 100)
            {
                openNum += 5;
                skinnedRenderer.SetBlendShapeWeight(0, openNum);
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            while (openNum > 0)
            {
                openNum -= 5;
                skinnedRenderer.SetBlendShapeWeight(0, openNum);
                yield return new WaitForEndOfFrame();
            }
        }
    }

    bool spelled = false;
    private void CheckForSpell()
    {
        if (count < currentSpellSelected.mouseMovements.Length - 1) return;

        for (int i = 0; i < currentInputs.Length; i++)
        {
            if (currentInputs[i] != currentSpellSelected.mouseMovements[i])
            {
                return;
            }
        }

        
        if(currentSpellSelected.spellPrefab != null)
        {
            Instantiate(currentSpellSelected.spellPrefab, grimoire.position, transform.rotation);
        }
        else
        {
            GameObject temp = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Sphere), grimoire.position, transform.rotation);
            temp.AddComponent<SphereCollider>();
            temp.AddComponent<Rigidbody>().AddForce(transform.forward * 5, ForceMode.Impulse);
        }

        spelled = true;
        count = 0;
        return;
    }

    public void Equip(Grimoire grim, SkinnedMeshRenderer meshRenderer)
    {
        skinnedRenderer = meshRenderer;
        currentGrimoire = grim;

        if(first && Gamemanager.newGame)
        {
            currentGrimoire.elementTypes = new List<ElementTypes>();
            first = false;
        }
        else
        {
            foreach (ElementTypes elementType in currentGrimoire.elementTypes)
            {
                GrimoireGpxHandler t = grimoire.GetComponentInChildren<GrimoireGpxHandler>();
                t.ActivateElement(elementType, skinnedRenderer);
            }
        }


    }
    public void UnEquip()
    {
        currentGrimoire = null;
    }

    
}
public enum MouseInputs { Up,Right,Down,Left }
