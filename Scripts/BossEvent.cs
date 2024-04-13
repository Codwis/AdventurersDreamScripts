using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEvent : MonoBehaviour
{
    [Tooltip("Prefabs that should activate like lights or fires")] public GameObject[] prefabsToActivate;
    public AIController aiController;
    [Tooltip("Should triggering have sound")] public AudioClip audioToPlay;

    public AudioClip clipToPlayEachPrefab;

    private Transform player;
    private bool activate = false;
    private void OnTriggerEnter(Collider other)
    {
        if(!activate)
        {
            activate = true;
            player = other.transform.root;
            StartCoroutine(wait());
        }
    }

    private IEnumerator wait()
    {
        Gamemanager.instance.globalAudio.PlayOneShot(audioToPlay);

        foreach(GameObject obj in prefabsToActivate)
        {
            yield return new WaitForSeconds(0.25f);
            Gamemanager.instance.globalAudio.PlayOneShot(clipToPlayEachPrefab);
            obj.SetActive(true);
        }

        activateBoss();
    }
    
    private void activateBoss()
    {
        aiController.Agro(player);
        Destroy(gameObject);
    }
}
