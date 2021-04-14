using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class GameOverDisplay : MonoBehaviour
{
    [SerializeField] private GameObject _gameOverDisplayParent = null;
    [SerializeField] private TextMeshProUGUI _winnerNameText = null;

    void Start()
    {
        _gameOverDisplayParent.SetActive(false);

        GameOverHandler.ClientRegisterOnGameOver(ClientHandleGameOver);
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientDeregisterOnGameOver(ClientHandleGameOver);
    }

    private void ClientHandleGameOver(string winner)
    {
        _winnerNameText.text = $"{winner} has Won!";

        _gameOverDisplayParent.SetActive(true);
    }

    public void LeaveGame()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
        }
    }
}
