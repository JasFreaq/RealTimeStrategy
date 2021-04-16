using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MinimapHandler : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private float _mapScale = 20f;
    [SerializeField] private float _offset = -7.5f;
    
    private RectTransform _minimapRect = null;

    private Transform _playerCamTransform = null;

    private void Awake()
    {
        _minimapRect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (!_playerCamTransform && NetworkClient.connection != null)
        {
            RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
            _playerCamTransform = player.MinimapCameraTransform;
        }
    }

    private void MoveCamera()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_minimapRect,
            mousePos, null, out Vector2 localPos))
        {
            Vector2 alpha = new Vector2((localPos.x - _minimapRect.rect.x) / _minimapRect.rect.width,
                (localPos.y - _minimapRect.rect.y) / _minimapRect.rect.height);

            Vector3 newPos = new Vector3(Mathf.Lerp(-_mapScale, _mapScale, alpha.x),
                _playerCamTransform.position.y, Mathf.Lerp(-_mapScale, _mapScale, alpha.y));

            _playerCamTransform.position = newPos + new Vector3(0, 0, _offset);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MoveCamera();
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveCamera();
    }
}
