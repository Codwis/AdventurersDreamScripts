using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.UI;

public class RagdollController : MonoBehaviour
{
    public Transform head;
    public GameObject respawnButton;
    public void EnableRagdoll()
    {
        Rigidbody[] rigidBodies = GetComponentsInChildren<Rigidbody>();
        for(int i = 0; i < rigidBodies.Length; i++)
        {
            rigidBodies[i].useGravity = true;
            rigidBodies[i].isKinematic = false;
            rigidBodies[i].freezeRotation = false;
            rigidBodies[i].constraints = RigidbodyConstraints.None;
            rigidBodies[i].gameObject.layer = LayerMask.NameToLayer("Dead");
        }
        
        
        if (respawnButton != null)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            respawnButton.SetActive(true);
            GetComponentInChildren<DialogueHandler>().DisplayText("????", DialogueType.PositiveDeath);
        }

        Destroy(head.GetComponent<AIHeadController>());
    }
}
