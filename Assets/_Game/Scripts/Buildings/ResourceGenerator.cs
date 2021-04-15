using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ResourceGenerator : NetworkBehaviour
{
    [SerializeField] private int _resourcePerInterval = 10;
    [SerializeField] private float _interval = 2f;

    private Health _health;
    private RTSPlayer _player;

    private Coroutine _resourceGenerationRoutine = null;

    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    #region Server

    public override void OnStartServer()
    {
        _player = connectionToClient.identity.GetComponent<RTSPlayer>();

        _resourceGenerationRoutine = StartCoroutine(ResourceGenerationRoutine());

        _health.ServerRegisterOnDeath(ServerHandleDeath);
        GameOverHandler.ServerRegisterOnGameOver(ServerHandleGameOver);
    }

    public override void OnStopServer()
    {
        StopCoroutine(_resourceGenerationRoutine);

        _health.ServerDeregisterOnDeath(ServerHandleDeath);
        GameOverHandler.ServerDeregisterOnGameOver(ServerHandleGameOver);
    }

    private void ServerHandleDeath()
    {
        NetworkServer.Destroy(gameObject);
    }

    private void ServerHandleGameOver()
    {
        enabled = false;

        StopCoroutine(_resourceGenerationRoutine);
    }

    [Server]
    private IEnumerator ResourceGenerationRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_interval);
            _player.Resources += _resourcePerInterval;
        }
    }

    #endregion
}
