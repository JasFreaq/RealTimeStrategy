using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RTSNetworkManager : NetworkManager
{
    [Header("RTS")]
    [SerializeField] private GameObject _unitBasePrefab = null;
    [SerializeField] private GameObject _gameOverHandlerPrefab = null;

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
        player.TeamColor = new Color(Random.value, Random.value, Random.value);

        GameObject unitSpawnerInstance = Instantiate(_unitBasePrefab, 
            conn.identity.transform.position, conn.identity.transform.rotation);
        NetworkServer.Spawn(unitSpawnerInstance, conn);

        player.transform.position = Vector3.zero;
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (SceneManager.GetActiveScene().name.StartsWith("Map"))
        {
            GameObject gameOverHandler = Instantiate(_gameOverHandlerPrefab);

            NetworkServer.Spawn(gameOverHandler);
        }
    }
}
