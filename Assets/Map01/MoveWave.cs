using System.Collections;
using System.Collections.Generic;
using _Game.THIN.Scripts.Utility;
using UnityEngine;

/// <summary>
/// chỉnh sóng di chuyển trong scene 
/// </summary>
public class MoveWave : MonoBehaviour
{
    #region ====Properties====
    [Header("Wave Renderer")]
    [SerializeField] Renderer wave;
    [Header("Scale Value")]
    [SerializeField] Vector2 tiling=new Vector2(20,20);
    [Header("Move Speed")]
    [SerializeField] float waveSpeed;
    #endregion

    #region ====Unity Core====
    private void OnEnable()
    {
        if (!wave)
            wave = GetComponent<Renderer>();
        
        if (!wave || !wave.material || !wave.material.HasProperty(ShaderIDLib.MainTex))
        {
            enabled = false;
            return;
        }
        wave.material.SetTextureScale(ShaderIDLib.MainTex, tiling);
    }

    private void Update()
    {
        wave.material.SetTextureOffset(ShaderIDLib.MainTex, new Vector2(0, Time.time * waveSpeed));
    }
    #endregion

}
