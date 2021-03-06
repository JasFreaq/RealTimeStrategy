using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Transform _mainCameraTransform = null;

    private void Start()
    {
        _mainCameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + _mainCameraTransform.rotation * Vector3.forward,
            _mainCameraTransform.rotation * Vector3.up);
    }
}
