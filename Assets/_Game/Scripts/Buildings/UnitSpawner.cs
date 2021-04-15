using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private UnitBehaviour _unitPrefab = null;
    [SerializeField] private Transform _unitSpawnPoint = null;
    [SerializeField] private TextMeshProUGUI _remainingUnitsText = null;
    [SerializeField] private Image _unitProgressImage = null;
    [SerializeField] private int _unitQueueLimit = 5;
    [SerializeField] private float _unitSpawnDuration = 5f;
    [SerializeField] private float _spawnMoveRange = 7.5f;

    [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdate))] 
    private int _queuedUnits = 0;
    [SyncVar(hook = nameof(ClientHandleUnitTimerUpdate))] 
    private float _unitTimer;

    private float _progressImageVelocity;

    private Health _health;
    private RTSPlayer _player;

    private void Awake()
    {
        _health = GetComponent<Health>();

        _unitTimer = _unitSpawnDuration;
    }
    
    #region Server

    [ServerCallback]
    private void Update()
    {
        if (_queuedUnits > 0) 
        {
            _unitTimer -= Time.deltaTime;
            if (_unitTimer <= 0)
            {
                SpawnUnit();

                _queuedUnits--;
                _unitTimer += _unitSpawnDuration;
            }
        }
    }

    public override void OnStartServer()
    {
        _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        _health.ServerRegisterOnDeath(ServerHandleDeath);
    }

    public override void OnStopServer()
    {
        _health.ServerDeregisterOnDeath(ServerHandleDeath);
    }

    [Server]
    private void ServerHandleDeath()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Server]
    private void SpawnUnit()
    {
        GameObject unitInstance = Instantiate(_unitPrefab.gameObject, _unitSpawnPoint.position, _unitSpawnPoint.rotation);
        NetworkServer.Spawn(unitInstance, connectionToClient);

        Vector3 spawnOffset = Random.insideUnitCircle * _spawnMoveRange;
        spawnOffset.y = _unitSpawnPoint.position.y;

        UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();
        unitMovement.ServerMove(spawnOffset);

    }

    [Command]
    private void CmdAddToQueue()
    {
        if (_queuedUnits < _unitQueueLimit) 
        {
            _queuedUnits++;
            _player.Resources -= _unitPrefab.Price;
        }
    }

    #endregion

    #region Client

    public void OnPointerClick(PointerEventData eventData)
    {
        if (hasAuthority && eventData.button == PointerEventData.InputButton.Left
                         && _player.Resources >= _unitPrefab.Price) 
        {
            CmdAddToQueue();
        }
    }

    private void ClientHandleQueuedUnitsUpdate(int oldUnits, int newUnits)
    {
        _remainingUnitsText.text = newUnits.ToString();
    }
    
    private void ClientHandleUnitTimerUpdate(float oldTime, float newTime)
    {
        _unitProgressImage.fillAmount = newTime / _unitSpawnDuration;
        if (_queuedUnits == 0)
            _unitProgressImage.fillAmount = 0;
    }

    #endregion
}
