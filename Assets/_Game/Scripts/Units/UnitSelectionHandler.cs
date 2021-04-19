using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UnitSelectionHandler : MonoBehaviour
{
    [SerializeField] private RectTransform _selectionArea = null;
    [SerializeField] private LayerMask _layerMask = new LayerMask();

    private Camera _mainCamera;
    
    private RTSPlayer _player;
    
    private List<UnitBehaviour> _selectedUnits = new List<UnitBehaviour>();

    private Vector2 _selectionAreaStartPos;

    public IReadOnlyList<UnitBehaviour> SelectedUnits { get { return _selectedUnits; } }

    private void Start()
    {
        _mainCamera = Camera.main;
        _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        UnitBehaviour.AuthorityRegisterOnUnitDespawn(AuthorityHandleUnitDespawn);
        GameOverHandler.ClientRegisterOnGameOver(ClientHandleGameOver);
    }

    private void Update()
    {
        HandleSelection();
    }

    private void OnDestroy()
    {
        UnitBehaviour.AuthorityDeregisterOnUnitDespawn(AuthorityHandleUnitDespawn);
        GameOverHandler.ClientDeregisterOnGameOver(ClientHandleGameOver);
    }

    public void HandleSelection()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartSelectionArea();
        }
        else if (Mouse.current.leftButton.isPressed)
        {
            UpdateSelectionArea();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }
    }
    
    private void StartSelectionArea()
    {
        if (!Keyboard.current.leftShiftKey.isPressed) 
        {
            foreach (UnitBehaviour selectedUnit in _selectedUnits)
            {
                selectedUnit.Deselect();
            }

            _selectedUnits.Clear();
        }

        _selectionAreaStartPos = Mouse.current.position.ReadValue();
        _selectionArea.gameObject.SetActive(true);

        UpdateSelectionArea();
    }

    private void UpdateSelectionArea()
    {
        Vector2 currentMousePos = Mouse.current.position.ReadValue();

        float areaWidth = currentMousePos.x - _selectionAreaStartPos.x;
        float areaHeight = currentMousePos.y - _selectionAreaStartPos.y;

        _selectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        _selectionArea.anchoredPosition = _selectionAreaStartPos + new Vector2(areaWidth / 2, areaHeight / 2);
    }

    private void ClearSelectionArea()
    {
        _selectionArea.gameObject.SetActive(false);

        if (_selectionArea.sizeDelta.magnitude == 0) 
        {
            Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _layerMask))
            {
                UnitBehaviour unit = hit.transform.GetComponent<UnitBehaviour>();
                AddUnit(unit);
            }
        }
        else
        {
            Vector2 min = _selectionArea.anchoredPosition - (_selectionArea.sizeDelta / 2);
            Vector2 max = _selectionArea.anchoredPosition + (_selectionArea.sizeDelta / 2);

            foreach (UnitBehaviour unit in _player.OwnedUnits)
            {
                Vector2 pos = _mainCamera.WorldToScreenPoint(unit.transform.position);

                if (pos.x < max.x && pos.x > min.x && pos.y < max.y && pos.y > min.y)
                {
                    AddUnit(unit);
                }
            }
        }
    }

    private void AddUnit(UnitBehaviour unit)
    {
        if (unit && unit.hasAuthority && !_selectedUnits.Contains(unit))
        {
            _selectedUnits.Add(unit);
            unit.Select();
        }
    }

    private void AuthorityHandleUnitDespawn(UnitBehaviour unit)
    {
        _selectedUnits.Remove(unit);
    }

    private void ClientHandleGameOver(string winnerName)
    {
        enabled = false;
    }
}
