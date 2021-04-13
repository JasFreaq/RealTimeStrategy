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
    }

    public override void OnStopServer()
    {
        ServerOnUnitDespawn?.Invoke(this);
    }
    
    #endregion

    #region Client

    public override void OnStartClient()
    {
        if (isClientOnly && hasAuthority)
        {
            AuthorityOnUnitSpawn?.Invoke(this);
        }
    }

    public override void OnStopClient()
    {
        if (isClientOnly && hasAuthority)
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
