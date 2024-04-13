using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Multiplayer;
using Unity.Netcode;
using Unity.VisualScripting;

public class PlayerSpawnerManager : MonoBehaviour
{
    public static NetworkObject userObject;
    public GameObject playerPrefab;
    public Vector3 playerSpawnSpot;

    private LobbyController controller;
    public void SpawnPlayer()
    {
        //GameObject te = Instantiate(playerPrefab, playerSpawnSpot, playerPrefab.transform.rotation);

        controller = LobbyController.currentLobby.controller;
        controller.UpdateLobby();
        //userObject = controller.networkManager.SpawnManager.GetLocalPlayerObject();
        userObject = controller.networkManager.SpawnManager.GetClientOwnedObjects(controller.networkManager.LocalClientId)[0];
        userObject.GetComponent<PlayerSetup>().RealSetup();
    }
}
