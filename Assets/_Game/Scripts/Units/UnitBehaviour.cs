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

    public UnitMovement UnitMovement
    {
        get
        {
            return _unitMovementComp;
        }
    }

    private void Awake()
    {
        _unitMovementComp = GetComponent<UnitMovement>();
    }

    #region Client

    [Client]
    public void Select()
    {
        if (hasAuthority)
        {
            _onSelected?.Invoke();
            print("Banana");
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
