using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RTSPlayer : NetworkBehaviour
{
    private List<UnitBehaviour> _ownedUnits = new List<UnitBehaviour>();
    private List<Building> _ownedBuildings = new List<Building>();

    public IReadOnlyList<UnitBehaviour> OwnedUnits { get { return _ownedUnits; }}
    public IReadOnlyList<Building> OwnedBuildings { get { return _ownedBuildings; }}

    #region Server

    public override void OnStartServer()
    {
        UnitBehaviour.ServerRegisterOnUnitSpawn(ServerHandleUnitSpawn);
        UnitBehaviour.ServerRegisterOnUnitDespawn(ServerHandleUnitDespawn);

        Building.ServerRegisterOnBuildingSpawn(ServerHandleBuildingSpawn);
        Building.ServerRegisterOnBuildingDespawn(ServerHandleBuildingDespawn);
    }

    public override void OnStopServer()
    {
        UnitBehaviour.ServerDeregisterOnUnitSpawn(ServerHandleUnitSpawn);
        UnitBehaviour.ServerDeregisterOnUnitDespawn(ServerHandleUnitDespawn);

        Building.ServerDeregisterOnBuildingSpawn(ServerHandleBuildingSpawn);
        Building.ServerDeregisterOnBuildingDespawn(ServerHandleBuildingDespawn);
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
    
    private void ServerHandleBuildingSpawn(Building building)
    {
        if (building.connectionToClient.connectionId == connectionToClient.connectionId)
        {
            _ownedBuildings.Add(building);
        }
    }

    private void ServerHandleBuildingDespawn(Building building)
    {
        if (building.connectionToClient.connectionId == connectionToClient.connectionId)
        {
            _ownedBuildings.Remove(building);
        }
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        if (!NetworkServer.active)
        {
            UnitBehaviour.AuthorityRegisterOnUnitSpawn(AuthorityHandleUnitSpawn);
            UnitBehaviour.AuthorityRegisterOnUnitDespawn(AuthorityHandleUnitDespawn);

            Building.AuthorityRegisterOnBuildingSpawn(AuthorityHandleBuildingSpawn);
            Building.AuthorityRegisterOnBuildingDespawn(AuthorityHandleBuildingDespawn);
        }
    }

    public override void OnStopClient()
    {
        if (isClientOnly && hasAuthority)
        {
            UnitBehaviour.AuthorityDeregisterOnUnitSpawn(AuthorityHandleUnitSpawn);
            UnitBehaviour.AuthorityDeregisterOnUnitDespawn(AuthorityHandleUnitDespawn);

            Building.AuthorityDeregisterOnBuildingSpawn(AuthorityHandleBuildingSpawn);
            Building.AuthorityDeregisterOnBuildingDespawn(AuthorityHandleBuildingDespawn);
        }
    }

    private void AuthorityHandleUnitSpawn(UnitBehaviour unit)
    {
        _ownedUnits.Add(unit);
    }

    private void AuthorityHandleUnitDespawn(UnitBehaviour unit)
    {
        _ownedUnits.Remove(unit);
    }
    
    private void AuthorityHandleBuildingSpawn(Building building)
    {
        _ownedBuildings.Add(building);
    }

    private void AuthorityHandleBuildingDespawn(Building building)
    {
        _ownedBuildings.Remove(building);
    }

    #endregion
}
