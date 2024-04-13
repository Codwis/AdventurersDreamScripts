using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

public class LobbyController : NetworkBehaviour
{
    public static string devId = "eN6Jjh8TZ9UxaQQexU0S3DJMlHM7";

    public string sceneToLoad = "Demo";
    public static PlayerSaver player;

    public static LobbyController hostControl;
    public static LobbySaver currentLobby;

    public LobbyList list;
    public Animator animator;

    [Header("Ui Objects")]
    public GameObject lobbiesObject;
    public GameObject currentLobbyObject;
    public Transform playersUiObj;

    [HideInInspector] public PlayerSpawnerManager spawnerManager;
    public LobbyPlayerManager lobbyManager;

    public Text lobbyCode;
    public Text maxPlayersText;

    [Header("Misc")]
    public ErrorHandlerMenu handler;
    public NetworkManager networkManager;
    public UnityTransport transport;

    private string lobbyName = "Lobby";
    private int maxPlayers = 1;
    private bool isPrivate = false;
    private string lobbyCodeWriteBox;

    private async void Start()
    {

        player.playerModel = false;
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            player.info = await AuthenticationService.Instance.GetPlayerInfoAsync();
        }
        catch(LobbyServiceException e)
        {
            handler.ShowError(e);
        }

        currentLobby.players = new List<ulong>();
        spawnerManager = GetComponent<PlayerSpawnerManager>();
        player.controller = this;
    }

    public async void FindServers()
    {
        list.ResetLobbies();
        QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync();

        foreach (Lobby lobby in response.Results)
        {
            if (lobby.IsPrivate) continue;
            list.AddLobby(lobby, this);
        }
    }

    public async void HostServer()
    {
        Allocation allocation = await Relay.Instance.CreateAllocationAsync(maxPlayers);
        DataObject data = new DataObject(DataObject.VisibilityOptions.Public, await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId));

        CreateLobbyOptions options = new CreateLobbyOptions();
        options.IsPrivate = isPrivate;
        options.Data = new Dictionary<string, DataObject>();
        options.Data.Add("allocationCode", data);
        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

        lobbyCode.text = lobby.LobbyCode;
        hostControl = this;

        networkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        transport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);
        networkManager.StartHost();
        networkManager.SetSingleton();

        PlayerJoined(lobby);
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {

    }

    public async void JoinWithCode()
    {
        try
        {
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCodeWriteBox);
            animator.SetTrigger("Join");

            lobbiesObject.SetActive(false);
            currentLobbyObject.SetActive(true);
            lobbyCode.text = lobby.LobbyCode;

            PlayerJoined(lobby);
        }
        catch (LobbyServiceException e)
        {
            handler.ShowError(e);
        }
    }

    public async void JoinWithLobby(Lobby lobbyIn)
    {
        try
        {
            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyIn.Id);
            animator.SetTrigger("Join");

            lobbiesObject.SetActive(false);
            currentLobbyObject.SetActive(true);
            lobbyCode.text = lobby.LobbyCode;

            PlayerJoined(lobby);
        }
        catch (LobbyServiceException e)
        {
            handler.ShowError(e);
        }
    }

    //When ever player joins set them up
    private async void PlayerJoined(Lobby lobby)
    {
        currentLobby.lobby = lobby;
        currentLobby.controller = this;
        player.player = currentLobby.lobby.Players[currentLobby.lobby.Players.Count - 1];

        //Starts clients for non hosts;
        if (player.info.Id != currentLobby.lobby.HostId)
        {
            DataObject dataObj = currentLobby.lobby.Data["allocationCode"];
            JoinAllocation allocation = await Relay.Instance.JoinAllocationAsync(dataObj.Value);
            transport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);
            networkManager.StartClient();
            transport.StartClient();
        }

        //Generate Random Id for player
        player.playerSpotInList = (int)networkManager.LocalClientId;
        StartCoroutine(SpawnInPlayer());
        StartCoroutine(LobbyTimer());
    }

    public async void LeaveLobby()
    {
        Destroy(PlayerSpawnerManager.userObject.gameObject);
        if(networkManager.IsHost)
        {
            await LobbyService.Instance.DeleteLobbyAsync(currentLobby.lobby.Id);
        }
        currentLobbyObject.SetActive(false);
        lobbiesObject.SetActive(true);
        animator.SetTrigger("Main");
    }

    private const float delay = 4;
    private IEnumerator SpawnInPlayer()
    {
        if(networkManager.IsHost)
        {
            yield return new WaitForSeconds(0);
            
        }
        else
        {
            yield return new WaitForSeconds(delay);
        }
        spawnerManager.SpawnPlayer();
    }

    //Ocassionally updates the lobby and sends a heartbeat to keep lobby alive
    public async void UpdateLobby()
    {
        int temp = currentLobby.lobby.Players.Count;
        currentLobby.lobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.lobby.Id);
        lobbyManager.SetLobby(currentLobby);
        lobbyManager.UpdatePlayers();

        if(currentLobby.lobby.HostId == player.info.Id)
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.lobby.Id);
        }
    }

    private IEnumerator LobbyTimer()
    {
        while(true)
        {
            UpdateLobby();
            yield return new WaitForSeconds(3.5f);
        }
    }

    //Loads a scene
    public void LoadScene()
    {
        networkManager.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
        networkManager.SceneManager.OnLoadComplete += SceneManager_OnLoadComplete;
    }

    //Gets called when scene is ready
    private void SceneManager_OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        //spawnerManager.SpawnPlayer();
    }

    public void Ready()
    {
        
        if(player.info.Id == currentLobby.lobby.HostId)
        {
            LoadScene();
        }
    }

    public PlayerSaver GetPlayerInfo()
    {
        return player;
    }
    public void PlayerModelCreated()
    {
        player.playerModel = true;
    }

    #region UI Functions 

    #region Host Functions
    public void LobbyNameChanged(string value)
    {
        lobbyName = value;
    }
    public void MaxPlayerChanged(float value)
    {
        maxPlayers = (int)value;
        maxPlayersText.text = "Max Players: " + value;
    }

    public void PrivateTrigger(bool on)
    {
        isPrivate = on;
    }

    #endregion

    public void ChangeCode(string code)
    {
        lobbyCodeWriteBox = code;
    }

    #endregion
}
public struct LobbySaver
{
    public Lobby lobby;
    public LobbyController controller;
    public List<ulong> players;
}

public struct PlayerSaver
{
    public Player player;
    public PlayerInfo info;
    public LobbyController controller;
    public int playerSpotInList;
    public bool playerModel;
}