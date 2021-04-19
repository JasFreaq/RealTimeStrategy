using System;
using System.Collections.Generic;
using Cinemachine;
using Mirror;
using UnityEngine;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private Building[] _buildings = new Building[0];
    [SerializeField] private float _buildingRangeLimit = 5f;
    [SerializeField] private LayerMask _buildingBlockLayer = new LayerMask();

    [SyncVar(hook = nameof(ClientHandleNameUpdate))]
    private string _displayName;
    [SyncVar(hook = nameof(ClientHandleResourceUpdate))] 
    private int _resources = 500;
    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerUpdate))] 
    private bool _partyOwner = false;

    private Action<int> _clientOnResourceUpdate;

    private static Action ClientOnInfoUpdate;
    private static Action<bool> AuthorityOnPartyOwnerUpdate;

    private Color _teamColor = new Color();
    private List<UnitBehaviour> _ownedUnits = new List<UnitBehaviour>();
    private List<Building> _ownedBuildings = new List<Building>();

    public string DisplayName
    {
        get { return _displayName; }
        [Server] set { _displayName = value; }
    }

    public int Resources
    {
        get { return _resources; }
        [Server] set { _resources = value; }
    }

    public bool IsPartyOwner
    {
        get { return _partyOwner; }
        [Server] set { _partyOwner = value; }
    }

    public Color TeamColor
    {
        get { return _teamColor; }
        [Server] set { _teamColor = value; }
    }

    public IReadOnlyList<UnitBehaviour> OwnedUnits { get { return _ownedUnits; } }
    public IReadOnlyList<Building> OwnedBuildings { get { return _ownedBuildings; } }

    public Transform MinimapCameraTransform { get { return transform.GetChild(0); } }
    
    public bool CanPlaceBuilding(int buildingID, Vector3 position)
    {
        if (TryFindBuilding(buildingID, out Building building))
        {
            BoxCollider buildingCollider = building.GetComponent<BoxCollider>();
            if (!Physics.CheckBox(position + buildingCollider.center, buildingCollider.size / 2,
                Quaternion.identity, _buildingBlockLayer))
            {
                foreach (Building ownedBuilding in _ownedBuildings)
                {
                    if ((ownedBuilding.transform.position - position).sqrMagnitude <=
                        Mathf.Pow(_buildingRangeLimit, 2))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool TryFindBuilding(int ID, out Building building)
    {
        building = null;

        foreach (Building buildingType in _buildings)
        {
            if (buildingType.ID == ID)
            {
                building = buildingType;
                return true;
            }
        }

        return false;
    }

    #region Server

    public override void OnStartServer()
    {
        DontDestroyOnLoad(gameObject);

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

    [Command]
    public void CmdStartGame()
    {
        if (_partyOwner)
        {
            ((RTSNetworkManager) NetworkManager.singleton).StartGame();
        }
    }

    [Command]
    public void CmdTryPlaceBuilding(int buildingID, Vector3 position)
    {
        if (TryFindBuilding(buildingID, out Building building))
        {
            GameObject buildingInstance = Instantiate(building, position, Quaternion.identity).gameObject;
            NetworkServer.Spawn(buildingInstance, connectionToClient);

            _resources -= building.Price;
        }
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

    public override void OnStartClient()
    {
        if (!NetworkServer.active)
        {
            DontDestroyOnLoad(gameObject);

            ((RTSNetworkManager) NetworkManager.singleton).Players.Add(this);
        }
    }

    public override void OnStopClient()
    {
        if (isClientOnly)
        {
            ((RTSNetworkManager)NetworkManager.singleton).Players.Remove(this);

            if (hasAuthority) 
            {
                UnitBehaviour.AuthorityDeregisterOnUnitSpawn(AuthorityHandleUnitSpawn);
                UnitBehaviour.AuthorityDeregisterOnUnitDespawn(AuthorityHandleUnitDespawn);

                Building.AuthorityDeregisterOnBuildingSpawn(AuthorityHandleBuildingSpawn);
                Building.AuthorityDeregisterOnBuildingDespawn(AuthorityHandleBuildingDespawn);
            }

            ClientOnInfoUpdate?.Invoke();
        }
    }

    private void ClientHandleNameUpdate(string oldName, string newName)
    {
        ClientOnInfoUpdate?.Invoke();
    }
    
    private void ClientHandleResourceUpdate(int oldResources, int newResources)
    {
        _clientOnResourceUpdate?.Invoke(newResources);
    }

    private void AuthorityHandlePartyOwnerUpdate(bool wasOwner, bool isOwner)
    {
        if (hasAuthority)
        {
            AuthorityOnPartyOwnerUpdate?.Invoke(isOwner);
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
    
    #region Observer Registration Functions

    public void ClientRegisterOnResourceUpdate(Action<int> action)
    {
        _clientOnResourceUpdate += action;
    }

    public void ClientDeregisterOnResourceUpdate(Action<int> action)
    {
        _clientOnResourceUpdate -= action;
    }

    public static void ClientRegisterOnInfoUpdate(Action action)
    {
        ClientOnInfoUpdate += action;
    }

    public static void ClientDeregisterOnInfoUpdate(Action action)
    {
        ClientOnInfoUpdate -= action;
    }
    
    public static void AuthorityRegisterOnPartyUpdate(Action<bool> action)
    {
        AuthorityOnPartyOwnerUpdate += action;
    }

    public static void AuthorityDeregisterOnPartyUpdate(Action<bool> action)
    {
        AuthorityOnPartyOwnerUpdate -= action;
    }

    #endregion
}
