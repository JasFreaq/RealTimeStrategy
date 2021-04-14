using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private float _chaseRange = 7.5f;

    private NavMeshAgent _agent = null;
    private TargetHandler _targetHandler = null;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _targetHandler = GetComponent<TargetHandler>();
    }

    #region Server

    public override void OnStartServer()
    {
        GameOverHandler.ServerRegisterOnGameOver(ServerHandleGameOver);
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerDeregisterOnGameOver(ServerHandleGameOver);
    }

    [ServerCallback]
    private void Update()
    {
        Targetable target = _targetHandler.Target;
        if (target)
        {
            if ((target.transform.position - transform.position).sqrMagnitude > Mathf.Pow(_chaseRange, 2)) 
            {
                _agent.SetDestination(target.transform.position);
            }
            else if (_agent.hasPath)
            {
                _agent.ResetPath();
            }
        }
        else
        {
            if (_agent.hasPath && _agent.remainingDistance <= _agent.stoppingDistance)
            {
                _agent.ResetPath();
            }
        }
    }

    [Command]
    public void CmdMove(Vector3 position)
    {
        _targetHandler.ClearTarget();

        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }
    }

    [Server]
    private void ServerHandleGameOver()
    {
        _agent.ResetPath();
    }

    #endregion
}
