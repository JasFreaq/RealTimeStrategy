using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class TeamColorSetter : NetworkBehaviour
{
    [SerializeField] private Renderer[] _renderers = new Renderer[0];

    [SyncVar(hook = nameof(HandleTeamColorUpdate))] 
    private Color _teamColor;

    #region Server

    public override void OnStartServer()
    {
        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();
        _teamColor = player.TeamColor;
    }

    #endregion

    #region Client

    private void HandleTeamColorUpdate(Color oldColor, Color newColor)
    {
        foreach (Renderer thisRenderer in _renderers)
        {
            thisRenderer.material.color = _teamColor;
        }
    }

    #endregion
}
