using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyItem : MonoBehaviour
{
    public Text lobbyNameText;
    public Text playerCountText;

    private Lobby lobby;
    private LobbyController controller;

    public void SetVariables(Lobby thisLobby, in LobbyController lobController)
    {
        lobby = thisLobby;

        lobbyNameText.text = lobby.Name;
        playerCountText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;

        controller = lobController;
    }
    public void JoinLobby()
    {
        controller.JoinWithLobby(lobby);
    }
}
