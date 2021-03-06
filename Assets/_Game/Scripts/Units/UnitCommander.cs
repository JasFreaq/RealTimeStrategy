using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommander : MonoBehaviour
{
    [SerializeField] private UnitSelectionHandler _unitSelectionHandler = null;
    [SerializeField] private LayerMask _layerMask = new LayerMask();

    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;

        GameOverHandler.ClientRegisterOnGameOver(ClientHandleGameOver);
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

    private void OnDestroy()
    {
        GameOverHandler.ClientDeregisterOnGameOver(ClientHandleGameOver);
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

    private void ClientHandleGameOver(string winnerName)
    {
        enabled = false;
    }
}
