using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class TargetHandler : NetworkBehaviour
{
    private Targetable _target = null;

    public Targetable Target { get { return _target; } }

    public override void OnStartServer()
    {
        GameOverHandler.ServerRegisterOnGameOver(ServerHandleGameOver);
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerDeregisterOnGameOver(ServerHandleGameOver);
    }

    [Command]
    public void CmdSetTarget(Targetable target)
    {
        _target = target;
    }

    [Server]
    public void ClearTarget()
    {
        _target = null;
    }

    [Server]
    private void ServerHandleGameOver()
    {
        ClearTarget();
    }
}
