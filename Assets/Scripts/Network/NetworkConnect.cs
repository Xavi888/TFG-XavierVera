using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using TMPro;

public class NetworkConnect : NetworkBehaviour
{
    public int maxPlayers = 2;
    public UnityTransport transport;
    public GameObject createPanelCode;
    public GameObject joinPanelCode;

    private Lobby currentLobby;
    private float refreshLobbyTimer;

    private async void Awake()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void OnCreate()
    {
        string gameMode = GameManager.Instance.GetGameMode();

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        transport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

        CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
        {
            IsPrivate = false,
            Data = new Dictionary<string, DataObject>
            {
                { "joinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) },
                { "gameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode, DataObject.IndexOptions.S1) }
            }
        };

        currentLobby = await LobbyService.Instance.CreateLobbyAsync(gameMode + " Lobby", maxPlayers, lobbyOptions);
        
        NetworkManager.Singleton.StartHost();

        //createPanelCode.GetComponentInChildren<TextMeshProUGUI>().text = joinCode;
        //createPanelCode.transform.parent.gameObject.SetActive(true);
    }

    public async void OnJoin()
    {
        //if (joinPanelCode.GetComponentInChildren<TextMeshProUGUI>().text != "")
        //{
        string gameMode = GameManager.Instance.GetGameMode();

        QuickJoinLobbyOptions quickJoinOptions = new QuickJoinLobbyOptions
        {
            Filter = new List<QueryFilter>
        {
            new QueryFilter(
                field: QueryFilter.FieldOptions.S1,
                op: QueryFilter.OpOptions.EQ,
                value: gameMode
            )
        }
        };

        currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinOptions);
        string relayJoinCode = currentLobby.Data["joinCode"].Value;

        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

        transport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);

        NetworkManager.Singleton.StartClient();
        //}
    }

    private void Update()
    {
        if (refreshLobbyTimer >= 15f)
        {
            refreshLobbyTimer = 0f;
            if (currentLobby != null && currentLobby.HostId == AuthenticationService.Instance.PlayerId)
            {
                LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            }
        }
        refreshLobbyTimer += Time.deltaTime;
    }
}
