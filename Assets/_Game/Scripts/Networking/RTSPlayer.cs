using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RTSPlayer : NetworkBehaviour
{
    private List<UnitBehaviour> _ownedUnits = new List<UnitBehaviour>();

    public IReadOnlyList<UnitBehaviour> OwnedUnits { get { return _ownedUnits; }}

    #region Server

    public override void OnStartServer()
    {
        UnitBehaviour.ServerRegisterOnUnitSpawn(ServerHandleUnitSpawn);
        UnitBehaviour.ServerRegisterOnUnitDespawn(ServerHandleUnitDespawn);
    }

    public override void OnStopServer()
    {
        UnitBehaviour.ServerDeregisterOnUnitSpawn(ServerHandleUnitSpawn);
        UnitBehaviour.ServerDeregisterOnUnitDespawn(ServerHandleUnitDespawn);
    }

    private void ServerHandleUnitSpawn(UnitBehaviour unit)
    {
        if (unit.connectionToClient.connectionId == connectionToClient.connectionId)
        {
            _ownedUnits.Add(unit);
        }
    }

    private void ServerHandleUnitDespawn(UnitBehaviour unit)
    {
        if (unit.connectionToClient.connectionId == connectionToClient.connectionId)
        {
            _ownedUnits.Remove(unit);
        }
    }

    #endregion

    #region Client

    public override void OnStartClient()
    {
        if (isClientOnly)
        {
            UnitBehaviour.AuthorityRegisterOnUnitSpawn(AuthorityHandleUnitSpawn);
            UnitBehaviour.AuthorityRegisterOnUnitDespawn(AuthorityHandleUnitDespawn);
        }
    }

    public override void OnStopClient()
    {
        if (isClientOnly)
        {
            UnitBehaviour.AuthorityDeregisterOnUnitSpawn(AuthorityHandleUnitSpawn);
            UnitBehaviour.AuthorityDeregisterOnUnitDespawn(AuthorityHandleUnitDespawn);
        }
    }

    private void AuthorityHandleUnitSpawn(UnitBehaviour unit)
    {
        if (hasAuthority) 
            _ownedUnits.Add(unit);
    }

    private void AuthorityHandleUnitDespawn(UnitBehaviour unit)
    {
        if (hasAuthority)
            _ownedUnits.Remove(unit);
    }

    #endregion
}
