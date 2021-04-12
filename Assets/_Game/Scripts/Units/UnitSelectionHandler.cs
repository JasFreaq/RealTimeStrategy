using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionHandler : MonoBehaviour
{
    [SerializeField] private LayerMask _layerMask = new LayerMask();

    private Camera _mainCamera;
    private List<UnitBehaviour> _selectedUnits = new List<UnitBehaviour>();

    public IReadOnlyList<UnitBehaviour> SelectedUnits
    {
        get
        {
            return _selectedUnits;
        }
    }

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleSelection();
    }

    public void HandleSelection()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            foreach (UnitBehaviour selectedUnit in _selectedUnits)
            {
                selectedUnit.Deselect();
            }

            _selectedUnits.Clear();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _layerMask))
            {
                UnitBehaviour unitBehaviour = hit.transform.GetComponent<UnitBehaviour>();
                if (unitBehaviour && unitBehaviour.hasAuthority)
                {
                    _selectedUnits.Add(unitBehaviour);

                    foreach (UnitBehaviour selectedUnit in _selectedUnits)
                    {
                        selectedUnit.Select();
                        print("Pop");
                    }
                }
            }
        }
    }
}
