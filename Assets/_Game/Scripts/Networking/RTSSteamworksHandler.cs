using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Steamworks;
using UnityEngine;

public class RTSSteamworksHandler : MonoBehaviour
{
    [SerializeField] private bool _useSteam = false;
    [SerializeField] private GameObject _landingPanel = null;

    private const string PCH_KEY = "HostAddress";

    protected Callback<LobbyCreated_t> _onLobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> _onGameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> _onLobbyEnter;

    public bool UseSteam { get { return _useSteam; } }

    private void Start()
    {
        _onLobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreate);
        _onGameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequest);
        _onLobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
    }

    private void OnLobbyCreate(LobbyCreated_t callback)
    {
        if (callback.m_eResult == EResult.k_EResultOK)
        {
            NetworkManager.singleton.StartHost();

            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), PCH_KEY,
                SteamUser.GetSteamID().ToString());
        }
        else
        {
            _landingPanel.SetActive(true);
        }
    }

    private void OnGameLobbyJoinRequest(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEnter(LobbyEnter_t callback)
    {
        if (!NetworkServer.active)
        {
            string address = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), PCH_KEY);

            NetworkManager.singleton.networkAddress = address;
            NetworkManager.singleton.StartClient();

            _landingPanel.SetActive(false);
        }
    }
}
