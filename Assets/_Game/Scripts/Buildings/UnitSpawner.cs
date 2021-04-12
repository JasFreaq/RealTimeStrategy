using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject _unitPrefab = null;
    [SerializeField] private Transform _unitSpawnPoint = null;

    #region Server

    [Command]
    private void CmdSpawnUnit()
    {
        GameObject unitInstance = Instantiate(_unitPrefab, _unitSpawnPoint.position, _unitSpawnPoint.rotation);

        NetworkServer.Spawn(unitInstance, connectionToClient);
    }

    #endregion

    #region Client

    public void OnPointerClick(PointerEventData eventData)
    {
        if (hasAuthority && eventData.button == PointerEventData.InputButton.Left)
        {
            CmdSpawnUnit();
        }
    }

    #endregion
}
