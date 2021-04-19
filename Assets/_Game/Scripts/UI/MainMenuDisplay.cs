using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuDisplay : MonoBehaviour
{
    [SerializeField] private GameObject _landingPanel = null;

    [Header("Enter Address Menu")]
    [SerializeField] private GameObject _enterAddressPanel = null;
    [SerializeField] private TMP_InputField _addressInput = null;
    [SerializeField] private Button _joinButton = null;

    [Header("Lobby Menu")]
    [SerializeField] private GameObject _lobbyPanel = null;
    [SerializeField] private Button _startButton = null;
    [SerializeField] private TextMeshProUGUI[] _playerNameTexts = new TextMeshProUGUI[4];

    private RTSSteamworksHandler _steamworksHandler = null;

    private void Awake()
    {
        _steamworksHandler = GetComponent<RTSSteamworksHandler>();
    }

    private void OnEnable()
    {
        RTSNetworkManager.ClientRegisterOnConnect(HandleClientConnected);
        RTSNetworkManager.ClientRegisterOnDisconnect(HandleClientDisconnected);

        RTSPlayer.ClientRegisterOnInfoUpdate(HandleClientInfoUpdate);
        RTSPlayer.AuthorityRegisterOnPartyUpdate(HandleAuthorityPartyUpdate);
    }

    private void OnDisable()
    {
        RTSNetworkManager.ClientDeregisterOnConnect(HandleClientConnected);
        RTSNetworkManager.ClientDeregisterOnDisconnect(HandleClientDisconnected);

        RTSPlayer.ClientDeregisterOnInfoUpdate(HandleClientInfoUpdate);
        RTSPlayer.AuthorityDeregisterOnPartyUpdate(HandleAuthorityPartyUpdate);
    }

    public void HostLobby()
    {
        _landingPanel.SetActive(false);
        
        if (_steamworksHandler && _steamworksHandler.UseSteam)
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
        }
        else
        {
            NetworkManager.singleton.StartHost();
        }
    }

    public void JoinLobby()
    {
        string address = _addressInput.text;

        NetworkManager.singleton.networkAddress = address;
        NetworkManager.singleton.StartClient();

        _joinButton.interactable = false;
    }

    public void StartGame()
    {
        NetworkClient.connection.identity.GetComponent<RTSPlayer>().CmdStartGame();
    }

    private void HandleClientConnected()
    {
        _joinButton.interactable = true;

        _enterAddressPanel.SetActive(false);
        _landingPanel.SetActive(false);

        _lobbyPanel.SetActive(true);
    }
    
    private void HandleClientDisconnected()
    {
        _joinButton.interactable = true;
    }

    void HandleClientInfoUpdate()
    {
        List<RTSPlayer> players = ((RTSNetworkManager) NetworkManager.singleton).Players;

        for (int i = 0, n = players.Count; i < n; i++)
        {
            _playerNameTexts[i].text = $"{players[i].DisplayName}";
        }
        
        for (int i = players.Count, n = _playerNameTexts.Length; i < n; i++)
        {
            _playerNameTexts[i].text = $"Waiting for Player...";
        }

        _startButton.interactable = (players.Count >= 2);
    }

    void HandleAuthorityPartyUpdate(bool isOwner)
    {
        _startButton.gameObject.SetActive(isOwner);
    }

    public void LeaveLobby()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
            SceneManager.LoadScene(0);
        }
    }
}
