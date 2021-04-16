using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour
{
    [SerializeField] private Transform _playerCameraTransform = null;
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _screenBorderThickness = 10f;
    [SerializeField] private Vector2 _minScreenLimits;
    [SerializeField] private Vector2 _maxScreenLimits;

    private Controls _controls;
    private Vector2 _input;

    public override void OnStartAuthority()
    {
        _playerCameraTransform.gameObject.SetActive(true);

        _controls = new Controls();

        _controls.Player.MoveCamera.performed += SetPreviousInput;
        _controls.Player.MoveCamera.canceled += SetPreviousInput;

        _controls.Enable();
    }

    [ClientCallback]
    private void Update()
    {
        if (hasAuthority && Application.isFocused)
        {
            UpdateCameraPosition();
        }
    }

    private void UpdateCameraPosition()
    {
        Vector3 newPos = _playerCameraTransform.position;

        if (_input == Vector2.zero)
        {
            Vector3 cursorMovement = Vector3.zero;
            Vector2 cursorPosition = Mouse.current.position.ReadValue();

            if (cursorPosition.x >= Screen.width - _screenBorderThickness)
            {
                cursorMovement.x++;
            }
            else if (cursorPosition.x <= _screenBorderThickness)
            {
                cursorMovement.x--;
            }
            
            if (cursorPosition.y >= Screen.height - _screenBorderThickness)
            {
                cursorMovement.z++;
            }
            else if (cursorPosition.y <= _screenBorderThickness)
            {
                cursorMovement.z--;
            }

            newPos += cursorMovement.normalized * _speed * Time.deltaTime;
        }
        else
        {
            newPos += new Vector3(_input.x, 0f, _input.y) * _speed * Time.deltaTime;
        }

        newPos.x = Mathf.Clamp(newPos.x, _minScreenLimits.x, _maxScreenLimits.x);
        newPos.z = Mathf.Clamp(newPos.z, _minScreenLimits.y, _maxScreenLimits.y);

        _playerCameraTransform.position = newPos;
    }

    private void SetPreviousInput(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
    }
}
