using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class TargetHandler : NetworkBehaviour
{
    private Targetable _target = null;

    public Targetable Target { get { return _target; } }

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
}
