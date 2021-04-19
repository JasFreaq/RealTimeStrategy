using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class ResourceDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _resourcesText = null;

    private RTSPlayer _player = null;

    private void Start()
    {
        _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        ClientHandleResourceUpdate(_player.Resources);
        _player.ClientRegisterOnResourceUpdate(ClientHandleResourceUpdate);
    }

    private void OnDestroy()
    {
        _player.ClientDeregisterOnResourceUpdate(ClientHandleResourceUpdate);
    }

    private void ClientHandleResourceUpdate(int resourcesValue)
    {
        _resourcesText.text = $"Resources: {resourcesValue}";
    }
}
