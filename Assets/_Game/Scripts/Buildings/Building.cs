using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Building : NetworkBehaviour
{
    [SerializeField] private Sprite _icon = null;
    [SerializeField] private int _iD = -1;
    [SerializeField] private int _price = 100;

    private static Action<Building> ServerOnBuildingSpawn;
    private static Action<Building> ServerOnBuildingDespawn;

    private static Action<Building> AuthorityOnBuildingSpawn;
    private static Action<Building> AuthorityOnBuildingDespawn;

    public Sprite Icon { get { return _icon; } }
    public int ID { get { return _iD; } }
    public int Price { get { return _price; } }

    public static void ServerRegisterOnBuildingSpawn(Action<Building> action)
    {
        ServerOnBuildingSpawn += action;
    }

    public static void ServerRegisterOnBuildingDespawn(Action<Building> action)
    {
        ServerOnBuildingDespawn += action;
    }

    public static void ServerDeregisterOnBuildingSpawn(Action<Building> action)
    {
        ServerOnBuildingSpawn -= action;
    }

    public static void ServerDeregisterOnBuildingDespawn(Action<Building> action)
    {
        ServerOnBuildingDespawn -= action;
    }
    
    public static void AuthorityRegisterOnBuildingSpawn(Action<Building> action)
    {
        AuthorityOnBuildingSpawn += action;
    }

    public static void AuthorityRegisterOnBuildingDespawn(Action<Building> action)
    {
        AuthorityOnBuildingDespawn += action;
    }

    public static void AuthorityDeregisterOnBuildingSpawn(Action<Building> action)
    {
        AuthorityOnBuildingSpawn -= action;
    }

    public static void AuthorityDeregisterOnBuildingDespawn(Action<Building> action)
    {
        AuthorityOnBuildingDespawn -= action;
    }

    #region Server

    public override void OnStartServer()
    {
        ServerOnBuildingSpawn?.Invoke(this);
    }

    public override void OnStopServer()
    {
        ServerOnBuildingDespawn?.Invoke(this);
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        AuthorityOnBuildingSpawn?.Invoke(this);
    }

    public override void OnStopClient()
    {
        if (hasAuthority)
        {
            AuthorityOnBuildingDespawn?.Invoke(this);
        }
    }

    #endregion
}
