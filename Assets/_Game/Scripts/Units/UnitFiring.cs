using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitFiring : NetworkBehaviour
{
    [SerializeField] private GameObject _projectilePrefab = null;
    [SerializeField] private Transform _projectileSpawnPoint = null;
    [SerializeField] private float _fireRange = 8f;
    [SerializeField] private float _fireRate = 1f;
    [SerializeField] float _rotationSpeed = 90f;

    private TargetHandler _targetHandler = null;
    private float _lastFireTime = 0f;

    private void Awake()
    {
        _targetHandler = GetComponent<TargetHandler>();
    }

    [ServerCallback]
    private void Update()
    {
        Targetable target = _targetHandler.Target;
        if (target && CanFireAtTarget())
        {
            Quaternion lookQuat = Quaternion.LookRotation(target.transform.position - transform.position);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookQuat, _rotationSpeed * Time.deltaTime);

            if (Time.time > (1 / _fireRate) + _lastFireTime)
            {
                Quaternion projectileQuat = Quaternion.LookRotation(target.GetAimAtPoint().position - _projectileSpawnPoint.position);

                GameObject projectile = Instantiate(_projectilePrefab, _projectileSpawnPoint.position, projectileQuat);
                NetworkServer.Spawn(projectile, connectionToClient);

                _lastFireTime = Time.time;
            }
        }
    }

    private bool CanFireAtTarget()
    {
        return (_targetHandler.Target.transform.position - transform.position).sqrMagnitude <= Mathf.Pow(_fireRange, 2);
    }
}
