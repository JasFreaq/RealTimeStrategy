using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SerializeField] private float _maxHealth = 100f;

    [SyncVar(hook = nameof(HandleHealthUpdate))] 
    private float _currentHealth;

    private Action _serverOnDeath;
    private Action<float> _clientOnHealthUpdate;

    #region Server

    public void ServerRegisterOnDeath(Action action)
    {
        _serverOnDeath += action;
    }

    public void ServerDeregisterOnDeath(Action action)
    {
        _serverOnDeath -= action;
    }
    
    public void ClientRegisterOnHealthUpdate(Action<float> action)
    {
        _clientOnHealthUpdate += action;
    }

    public void ClientDeregisterOnHealthUpdate(Action<float> action)
    {
        _clientOnHealthUpdate -= action;
    }

    public override void OnStartServer()
    {
        _currentHealth = _maxHealth;

        UnitBase.ServerRegisterOnPlayerDeath(ServerOnPlayerDeath);
    }

    public override void OnStopServer()
    {
        UnitBase.ServerDeregisterOnPlayerDeath(ServerOnPlayerDeath);
    }

    [Server]
    public void DealDamage(float damageAmount)
    {
        if (_currentHealth > 0)
        {
            _currentHealth = Mathf.Clamp(_currentHealth - damageAmount, 0f, _maxHealth);
            

            if (_currentHealth == 0)
            {
                _serverOnDeath?.Invoke();
            }
        }
    }

    [Server]
    private void ServerOnPlayerDeath(int ID)
    {
        if (connectionToClient.connectionId == ID)
        {
            NetworkServer.Destroy(gameObject);
        }
    }

    #endregion

    #region Client

    private void HandleHealthUpdate(float oldHealth, float newHealth)
    {
        _clientOnHealthUpdate?.Invoke(newHealth / _maxHealth);
    }

    #endregion
}
