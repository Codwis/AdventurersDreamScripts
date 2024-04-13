using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    public GameObject playerUi;
    public GameObject playerPrefab;
    public Vector3 spawnLoc;
    void Start()
    {
        transform.position = spawnLoc;
    }
    public void RealSetup()
    {
        if (!LobbyController.currentLobby.controller.GetPlayerInfo().playerModel)
        {
            GetComponentInChildren<AudioListener>().enabled = true;
            LobbyController.currentLobby.controller.PlayerModelCreated();
            playerUi.SetActive(true);

            var cams = GetComponentsInChildren<Camera>();
            foreach (Camera cam in cams) cam.enabled = true;
            GetComponent<PlayerController>().enabled = true;
            GetComponent<Animator>().enabled = true;
            GetComponent<CharacterController>().enabled = true;
            GetComponent<Rigidbody>().isKinematic = false;
            GetComponent<PlayerController>().inputsOn = true;
            gameObject.layer = LayerMask.NameToLayer("Player");
            
        }
    }
}
