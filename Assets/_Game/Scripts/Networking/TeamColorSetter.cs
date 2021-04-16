using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class TeamColorSetter : NetworkBehaviour
{
    [System.Serializable]
    struct RenderSetting
    {
        public Renderer renderer;
        public int materialIndex;
    }

    [SerializeField] private RenderSetting[] _renderSettings = new RenderSetting[0];

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
        foreach (RenderSetting renderSetting in _renderSettings)
        {
            if (renderSetting.materialIndex == 0)
            {
                renderSetting.renderer.material.color = _teamColor;
            }
            else if (renderSetting.materialIndex > 0 &&
                     renderSetting.materialIndex < renderSetting.renderer.materials.Length) 
            {
                renderSetting.renderer.materials[renderSetting.materialIndex].color = _teamColor;
            }
        }
    }

    #endregion
}
