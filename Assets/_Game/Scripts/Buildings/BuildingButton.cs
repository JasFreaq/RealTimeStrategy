using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Building _building;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private LayerMask _floorMask = new LayerMask();

    private Camera _mainCamera;
    private RTSPlayer _player;
    private GameObject _buildingPreviewInstance;
    private Renderer[] _buildingRendererInstances;

    private void Start()
    {
        _mainCamera = Camera.main;

        _iconImage.sprite = _building.Icon;
        _priceText.text = _building.Price.ToString();

        _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
    }

    private void Update()
    {
        UpdateBuildingPreview();
    }

    private void UpdateBuildingPreview()
    {
        if (_buildingPreviewInstance)
        {
            Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _floorMask))
            {
                _buildingPreviewInstance.transform.position = hit.point;

                if (!_buildingPreviewInstance.activeSelf)
                {
                    _buildingPreviewInstance.SetActive(true);
                }

                Color color = _player.CanPlaceBuilding(_building.ID, hit.point) ? Color.green : Color.red;
                foreach (Renderer buildingRendererInstance in _buildingRendererInstances)
                {
                    buildingRendererInstance.material.color = color;
                }
            }
            else
            {
                _buildingPreviewInstance.SetActive(false);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && _building.Price <= _player.Resources)
        {
            _buildingPreviewInstance = Instantiate(_building.BuildingPreview);
            _buildingRendererInstances = _buildingPreviewInstance.GetComponentsInChildren<Renderer>();

            _buildingPreviewInstance.SetActive(false);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_player && _buildingPreviewInstance)
        {
            Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _floorMask) &&
                _player.CanPlaceBuilding(_building.ID, hit.point))
            {
                _player.CmdTryPlaceBuilding(_building.ID, hit.point);
            }

            Destroy(_buildingPreviewInstance);
        }
    }
}
