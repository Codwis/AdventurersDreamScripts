using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Multiplayer;
public class AdventureNetworkManager : NetworkManager
{

    private void OnConnectedToServer()
    {
        Debug.Log("Yey it works");
    }
}
