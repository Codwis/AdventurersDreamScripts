using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class LobbyList : MonoBehaviour
{
    public GameObject lobbyPrefab;
    private List<LobbyItem> lobbies = new List<LobbyItem>();

    public void AddLobby(Lobby lobby, in LobbyController lobController)
    {
        LobbyItem item = Instantiate(lobbyPrefab, transform).GetComponent<LobbyItem>();

        lobbies.Add(item);
        item.SetVariables(lobby, lobController);
    }

    public void ResetLobbies()
    {
        for(int i = 0; i < lobbies.Count; i++)
        {
            Destroy(lobbies[0].gameObject);
        }
    }
}
