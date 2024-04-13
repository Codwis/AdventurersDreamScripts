using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class StepEvent : MonoBehaviour
{
    [Tooltip("Does it spawn something")] public GameObject[] prefabsToSpawn;
    [Tooltip("How much damage does it deal ps can also heal when negative")] public float damage;
    [Tooltip("Should triggering have sound")] public AudioClip audioToPlay;

    public bool playerOnly = false;
    public Vector3 locationToGo = Vector3.zero;
    AudioSource source;
    private void Start()
    {
        source = GetComponent<AudioSource>();
        source.volume = 0.2f;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(playerOnly)
        {
            if(!other.GetComponentInParent<PlayerController>())
            {
                return;
            }
        }
        if(audioToPlay != null)
        {
            source.PlayOneShot(audioToPlay);
        }
        if(prefabsToSpawn != null)
        {
            foreach(GameObject game in prefabsToSpawn)
            {
                Instantiate(game);
            }
        }
        if(locationToGo != Vector3.zero)
        {
            other.transform.root.GetComponent<PlayerController>().inputsOn = false;
            other.transform.root.GetComponent<PlayerController>().downVel = Vector3.zero;
            other.transform.root.position = locationToGo;
            other.transform.root.GetComponent<PlayerController>().downVel = Vector3.zero;
            StartCoroutine(resetMovement(other.transform.root.GetComponent<PlayerController>()));
        }
    }

    private IEnumerator resetMovement(PlayerController col)
    {
        yield return new WaitForSeconds(1);
        col.inputsOn = true;
    }
    private void OnTriggerStay(Collider other)
    {
        if (damage != 0)
        {
            other.TryGetComponent<Stats>(out Stats stats);
            if (stats)
            {
                stats.TakeDamage(damage, transform);
            }
        }
    }
}
public enum EventTypes { Spawn, Damage }
