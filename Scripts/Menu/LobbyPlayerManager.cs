using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyPlayerManager : MonoBehaviour
{
    public GameObject playerBannerPrefab;

    private List<GameObject> playerBanners = new List<GameObject>();
    private LobbySaver currentLobby;

    public void UpdatePlayers()
    {
        int dif = currentLobby.lobby.Players.Count - playerBanners.Count;
        if (dif > 0)
        {
            for(int i = 0; i < dif; i++)
            {
                GameObject temp = Instantiate(playerBannerPrefab, transform);
                playerBanners.Add(temp);
            }
        }
        else if(dif < 0)
        {
            for (int i = 0; i < dif; i++)
            {
                Destroy(playerBanners[0]);
            }
        }

        for(int i = 0; i < playerBanners.Count; i++)
        {
            string customName;
            customName = currentLobby.lobby.Players[i].Id;
            playerBanners[i].GetComponent<PlayerBanner>().SetPlayer(currentLobby.lobby.Players[i], currentLobby.controller);
        }
    }

    public void SetLobby(in LobbySaver saver)
    {
        currentLobby = saver;
    }

}
