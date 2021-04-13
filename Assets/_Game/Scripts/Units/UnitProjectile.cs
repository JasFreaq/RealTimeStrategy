using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class UnitProjectile : NetworkBehaviour
{
    [SerializeField] private float _launchForce = 500f;
    [SerializeField] private float _damage = 20f;
    [SerializeField] private float _lifetime = 7.5f;

    private Rigidbody _rigidbody = null;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        _rigidbody.AddForce(transform.forward * _launchForce);
    }

    public override void OnStartServer()
    {
        StartCoroutine(DestroySelfRoutine());
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<NetworkIdentity>(out NetworkIdentity identity))
        {
            if (identity.connectionToClient != connectionToClient)
            {
                if (other.TryGetComponent<Health>(out Health health))
                {
                    health.DealDamage(_damage);
                    NetworkServer.Destroy(gameObject);
                }
            }
        }
    }

    [Server]
    private IEnumerator DestroySelfRoutine()
    {
        yield return new WaitForSeconds(_lifetime);
        NetworkServer.Destroy(gameObject);
    }
}
