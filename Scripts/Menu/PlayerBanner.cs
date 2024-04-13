using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBanner : MonoBehaviour
{
    public Toggle ready;
    public Text playerName;
    private bool shown = false;
    public void SetPlayer(Player player, in LobbyController controller)
    {
        string customName = player.Id;

        if(player.Id == LobbyController.devId)
        {
            customName = "Codwis";
            if(!shown)
            {
                controller.handler.ShowText("The Creator Has Shown Up");
                shown = true;
            }
        }
        playerName.text = customName;
    }
    public void Ready()
    {
        ready.isOn = !ready.isOn;
    }
}
