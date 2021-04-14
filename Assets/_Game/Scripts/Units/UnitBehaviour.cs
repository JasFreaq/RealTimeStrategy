using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class UnitBehaviour : NetworkBehaviour
{
    [SerializeField] private UnityEvent _onSelected = null;
    [SerializeField] private UnityEvent _onDeselected = null;
    
    private UnitMovement _unitMovementComp;
    private TargetHandler _targetHandler;
    private Health _health;
    
    private static Action<UnitBehaviour> ServerOnUnitSpawn;
    private static Action<UnitBehaviour> ServerOnUnitDespawn;
    
    private static Action<UnitBehaviour> AuthorityOnUnitSpawn;
    private static Action<UnitBehaviour> AuthorityOnUnitDespawn;

    public UnitMovement UnitMovement
    {
        get
        {
            return _unitMovementComp;
        }
    }

    public TargetHandler TargetHandler
    {
        get
        {
            return _targetHandler;
        }
    }

    private void Awake()
    {
        _unitMovementComp = GetComponent<UnitMovement>();
        _targetHandler = GetComponent<TargetHandler>();
        _health = GetComponent<Health>();
    }

    public static void ServerRegisterOnUnitSpawn(Action<UnitBehaviour> action)
    {
        ServerOnUnitSpawn += action;
    }

    public static void ServerRegisterOnUnitDespawn(Action<UnitBehaviour> action)
    {
        ServerOnUnitDespawn += action;
    }

    public static void ServerDeregisterOnUnitSpawn(Action<UnitBehaviour> action)
    {
        ServerOnUnitSpawn -= action;
    }

    public static void ServerDeregisterOnUnitDespawn(Action<UnitBehaviour> action)
    {
        ServerOnUnitDespawn -= action;
    }
    
    public static void AuthorityRegisterOnUnitSpawn(Action<UnitBehaviour> action)
    {
        AuthorityOnUnitSpawn += action;
    }

    public static void AuthorityRegisterOnUnitDespawn(Action<UnitBehaviour> action)
    {
        AuthorityOnUnitDespawn += action;
    }

    public static void AuthorityDeregisterOnUnitSpawn(Action<UnitBehaviour> action)
    {
        AuthorityOnUnitSpawn -= action;
    }

    public static void AuthorityDeregisterOnUnitDespawn(Action<UnitBehaviour> action)
    {
        AuthorityOnUnitDespawn -= action;
    }

    #region Server

    public override void OnStartServer()
    {
        ServerOnUnitSpawn?.Invoke(this);
        _health.ServerRegisterOnDeath(ServerHandleDeath);
    }

    public override void OnStopServer()
    {
        ServerOnUnitDespawn?.Invoke(this);
        _health.ServerDeregisterOnDeath(ServerHandleDeath);
    }

    [Server]
    private void ServerHandleDeath()
    {
        NetworkServer.Destroy(gameObject);
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        AuthorityOnUnitSpawn?.Invoke(this);
    }

    public override void OnStopClient()
    {
        if (hasAuthority)
        {
            AuthorityOnUnitDespawn?.Invoke(this);
        }
    }

    [Client]
    public void Select()
    {
        if (hasAuthority)
        {
            _onSelected?.Invoke();
        }
    }

    [Client]
    public void Deselect()
    {
        if (hasAuthority)
        {
            _onDeselected?.Invoke();
        }
    }

    #endregion
}
