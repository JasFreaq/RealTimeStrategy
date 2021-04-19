using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class RTSNetworkManager : NetworkManager
{
    [Header("RTS")]
    [SerializeField] private GameObject _unitBasePrefab = null;
    [SerializeField] private GameObject _gameOverHandlerPrefab = null;

    private static Action ClientOnConnected;
    private static Action ClientOnDisconnected;

    private bool _gameInProgress = false;
    private List<RTSPlayer> _players = new List<RTSPlayer>();

    public List<RTSPlayer> Players { get { return _players; } }

    public static void ClientRegisterOnConnect(Action action)
    {
        ClientOnConnected += action;
    }

    public static void ClientDeregisterOnConnect(Action action)
    {
        ClientOnConnected -= action;
    }

    public static void ClientRegisterOnDisconnect(Action action)
    {
        ClientOnDisconnected += action;
    }

    public static void ClientDeregisterOnDisconnect(Action action)
    {
        ClientOnDisconnected -= action;
    }

    #region Server

    public override void OnServerConnect(NetworkConnection conn)
    {
        if (_gameInProgress)
        {
            conn.Disconnect();
        }
    }

    public void StartGame()
    {
        if (_players.Count >= 2)
        {
            _gameInProgress = true;
            ServerChangeScene("Map_01");
        }
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
        _players.Add(player);

        player.IsPartyOwner = (_players.Count == 1);
        player.DisplayName = $"Player {_players.Count}";
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (SceneManager.GetActiveScene().name.StartsWith("Map"))
        {
            GameObject gameOverHandler = Instantiate(_gameOverHandlerPrefab);
            NetworkServer.Spawn(gameOverHandler);

            foreach (RTSPlayer player in _players)
            {
                player.TeamColor = new Color(Random.value, Random.value, Random.value);

                GameObject unitBaseInstance = Instantiate(_unitBasePrefab,
                    player.transform.position, player.transform.rotation);
                NetworkServer.Spawn(unitBaseInstance, player.connectionToClient);

                player.transform.position = Vector3.zero;
            }
        }
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
        _players.Remove(player);

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        _players.Clear();
        _gameInProgress = false;
    }

    #endregion

    #region Client

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        ClientOnConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        ClientOnDisconnected?.Invoke();
    }

    public override void OnStopClient()
    {
        
    }

    #endregion
}
