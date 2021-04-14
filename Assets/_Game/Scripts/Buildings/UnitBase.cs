using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitBase : NetworkBehaviour
{
    private Health _health = null;
    
    private static Action<int> ServerOnPlayerDeath;
    private static Action<UnitBase> ServerOnBaseSpawned;
    private static Action<UnitBase> ServerOnBaseDespawned;

    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    #region Server

    public static void ServerRegisterOnPlayerDeath(Action<int> action)
    {
        ServerOnPlayerDeath += action;
    }
    
    public static void ServerRegisterOnBaseSpawn(Action<UnitBase> action)
    {
        ServerOnBaseSpawned += action;
    }

    public static void ServerRegisterOnBaseDespawn(Action<UnitBase> action)
    {
        ServerOnBaseDespawned += action;
    }

    public static void ServerDeregisterOnPlayerDeath(Action<int> action)
    {
        ServerOnPlayerDeath -= action;
    }
    
    public static void ServerDeregisterOnBaseSpawn(Action<UnitBase> action)
    {
        ServerOnBaseSpawned -= action;
    }

    public static void ServerDeregisterOnBaseDespawn(Action<UnitBase> action)
    {
        ServerOnBaseDespawned -= action;
    }

    public override void OnStartServer()
    {
        _health.ServerRegisterOnDeath(ServerHandleDeath);

        ServerOnBaseSpawned?.Invoke(this);
    }

    public override void OnStopServer()
    {
        _health.ServerDeregisterOnDeath(ServerHandleDeath);

        ServerOnBaseDespawned?.Invoke(this);
    }

    [Server]
    private void ServerHandleDeath()
    {
        ServerOnPlayerDeath?.Invoke(connectionToClient.connectionId);

        NetworkServer.Destroy(gameObject);
    }

    #endregion
}
