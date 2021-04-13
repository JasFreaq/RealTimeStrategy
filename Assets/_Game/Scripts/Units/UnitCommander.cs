using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommander : MonoBehaviour
{
    [SerializeField] private LayerMask _layerMask = new LayerMask();
    [SerializeField] private UnitSelectionHandler _unitSelectionHandler = null;

    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _layerMask))
            {
                if (hit.transform.TryGetComponent<Targetable>(out Targetable target))
                {
                    if (!target.hasAuthority)
                    {
                        TryTarget(target);
                        return;
                    }
                }

                TryMove(hit.point);
            }
        }
    }

    private void TryMove(Vector3 position)
    {
        foreach (UnitBehaviour unit in _unitSelectionHandler.SelectedUnits)
        {
            unit.UnitMovement.CmdMove(position);
        }
    }
    
    private void TryTarget(Targetable target)
    {
        foreach (UnitBehaviour unit in _unitSelectionHandler.SelectedUnits)
        {
            unit.TargetHandler.CmdSetTarget(target);
        }
    }
}
