using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmPlot : MonoBehaviour
{
    public GameObject youngPlant;
    public GameObject grownPlant;
    public bool readyToHarvest = false;
    public bool growing = false;

    public ToolTypes toolTypeRequired = ToolTypes.sack;

    private const float growTime = 140;
    private GameObject current;
    public float maxYoungSize;

    public IEnumerator ProgressGrow()
    {
        float currentSecond = 0;
        while(currentSecond < growTime)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(maxYoungSize, maxYoungSize, maxYoungSize), 1 * Time.deltaTime);
            yield return new WaitForSeconds(1);
            currentSecond++;
        }
        readyToHarvest = true;
        growing = false;
        Destroy(current);
        current = Instantiate(grownPlant, transform.position, transform.rotation, transform);
    }
    public void Water()
    {

    }
    public void Harvest()
    {
        Destroy(current);
        growing = false;
        readyToHarvest = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Farmer worker = other.GetComponentInParent<Farmer>();
        if (worker != null)
        {
            if (worker.currentTool == null)
            {
                if(worker.permamentTool == null)
                {
                    return;
                }
                else if (worker.permamentTool.toolType != toolTypeRequired) return;
            }
            else if (worker.currentTool.toolType != toolTypeRequired)
            {
                if (worker.permamentTool == null)
                {
                    return;
                }
                else if (worker.permamentTool.toolType != toolTypeRequired) return;
            }

            if (worker.currentPlot == this)
            {
                if (readyToHarvest)
                {
                    worker.GetComponent<AIController>().AllowMovement();
                    worker.GetComponent<Animator>().SetTrigger("Plow");
                    worker.GetComponent<Animator>().SetFloat("MoveSpeed", 0);

                    Destroy(current);
                    transform.localScale = Vector3.one;
                    toolTypeRequired = ToolTypes.sack;
                    readyToHarvest = false;
                }
                else if (!growing)
                {
                    worker.GetComponent<AIController>().AllowMovement();
                    worker.GetComponent<Animator>().SetTrigger("Plant");
                    worker.GetComponent<Animator>().SetFloat("MoveSpeed", 0);
                    growing = true;
                    toolTypeRequired = ToolTypes.scythe;
                    StartCoroutine(ProgressGrow());

                    worker.currentTool = null;
                    current = Instantiate(youngPlant, transform.position, transform.rotation, transform);
                }
            }
        }
    }


}
