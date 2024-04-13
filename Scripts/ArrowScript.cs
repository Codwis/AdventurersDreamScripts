using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ArrowScript : MonoBehaviour
{
    [NonSerialized] public Transform source;
    [NonSerialized] public AudioSource audioSource;

    public AudioClip arrowHitFlesh;
    public AudioClip arrowHit;
    public AudioClip arrowHitNoPenetrable;

    private float damage;
    private Rigidbody arrowRb;
    private GameObject parentObject;
    private Quaternion prevRot;

    private bool collided = false;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        arrowRb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        //Rotates the arrow so it faces where its going
        if(!collided && arrowRb.velocity.magnitude != 0)
        {
            transform.rotation = Quaternion.LookRotation(arrowRb.velocity);
            prevRot = transform.rotation;
        }
    }

    //Changes arrows damage
    public void ChangeDamage(float dmg)
    {
        damage = dmg;
    }

    //automatically removes the arrow
    private IEnumerator RemoveArrow()
    {
        yield return new WaitForSeconds(20);
        Destroy(parentObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.GetComponentInParent<RangedScript>() == source.GetComponent<RangedScript>())
        {
            return;
        }
        bool played = false;

        if (!collided)
        {
            if(collision.gameObject.TryGetComponent<BlockCollisionDetector>(out _)) // if it hits a blocking thing dont attach
            {
                StartCoroutine(RemoveArrow());
                collided = true;
                return;
            }
            Stats stats = collision.gameObject.GetComponentInParent<Stats>();
            if(stats != null)
            {
                //add armor to this
                stats.TakeDamage(damage, source);
                if(arrowHitFlesh != null && !played)
                {
                    played = true;
                    audioSource.PlayOneShot(arrowHitFlesh);
                }
            }

            //If collided object can be penetrated
            //Makes the arrow stick to the object
            if (!collision.collider.CompareTag("Unpenetrable"))
            {
                collided = true;
                arrowRb.isKinematic = true;


                parentObject = new GameObject("ParentObject");
                parentObject.transform.SetParent(collision.transform);
                transform.SetParent(parentObject.transform, true);

                transform.rotation = prevRot;

                if(arrowHitNoPenetrable != null && !played)
                {
                    played = true;
                    audioSource.PlayOneShot(arrowHitNoPenetrable);
                }

                Destroy(GetComponent<Collider>());
                Destroy(arrowRb);
            }
            else
            {

                if (arrowHit != null && !played)
                {
                    audioSource.PlayOneShot(arrowHit);
                }
                StartCoroutine(RemoveArrow());
                Destroy(this);
            }

            //Starts automatic arrow removal
            StartCoroutine(RemoveArrow());
        }
    }
}
