using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameOverHandler : NetworkBehaviour
{
    private List<UnitBase> _bases = new List<UnitBase>();

    private static Action ServerOnGameOver;
    private static Action<string> ClientOnGameOver;

    #region Server

    public static void ServerRegisterOnGameOver(Action action)
    {
        ServerOnGameOver += action;
    }

    public static void ServerDeregisterOnGameOver(Action action)
    {
        ServerOnGameOver -= action;
    }

    public override void OnStartServer()
    {
        UnitBase.ServerRegisterOnBaseSpawn(ServerHandleBaseSpawn);
        UnitBase.ServerRegisterOnBaseDespawn(ServerHandleBaseDespawn);
    }

    public override void OnStopServer()
    {
        UnitBase.ServerDeregisterOnBaseSpawn(ServerHandleBaseSpawn);
        UnitBase.ServerDeregisterOnBaseDespawn(ServerHandleBaseDespawn);
    }

    [Server]
    private void ServerHandleBaseSpawn(UnitBase unitBase)
    {
        _bases.Add(unitBase);
    }

    [Server]
    private void ServerHandleBaseDespawn(UnitBase unitBase)
    {
        _bases.Remove(unitBase);

        if (_bases.Count == 1)
        {
            int playerID = _bases[0].connectionToClient.connectionId;
            RPCGameOver($"Player {playerID}");

            ServerOnGameOver?.Invoke();
        }
    }

    #endregion

    #region Client

    public static void ClientRegisterOnGameOver(Action<string> action)
    {
        ClientOnGameOver += action;
    }
    
    public static void ClientDeregisterOnGameOver(Action<string> action)
    {
        ClientOnGameOver -= action;
    }

    [ClientRpc]
    private void RPCGameOver(string winner)
    {
        ClientOnGameOver?.Invoke(winner);
    }

    #endregion
}
