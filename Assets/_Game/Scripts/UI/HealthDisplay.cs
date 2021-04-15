using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private GameObject _healthBarParent = null;
    [SerializeField] private Slider _healthBarSlider = null;

    private Health _health = null;
    
    private void Awake()
    {
        _health = GetComponent<Health>();
        _health.ClientRegisterOnHealthUpdate(HandleHealthBar);
    }

    private void OnMouseEnter()
    {
        _healthBarParent.SetActive(true);
    }

    private void OnMouseExit()
    {
        _healthBarParent.SetActive(false);
    }

    private void OnDestroy()
    {
        _health.ClientDeregisterOnHealthUpdate(HandleHealthBar);
    }

    private void HandleHealthBar(float fraction)
    {
        _healthBarSlider.value = fraction;
    }
}
