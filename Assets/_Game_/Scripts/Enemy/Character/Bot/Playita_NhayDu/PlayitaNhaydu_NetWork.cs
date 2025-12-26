using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayitaNhaydu_NetWork : CharacterNetwork
{
    [Header("reference - PlayitaNhaydu_NetWork")]
    [SerializeField] private PlayitaNhaydu playitaNhaydu;

    public override void OnInit()
    {
        base.OnInit();
        if (botIdentity.AssignedPath.PointChindCanMove.Count <= 0)
        {
#if UNITY_EDITOR
            Debug.LogError("Thiếu PointChindCanMove cho "+gameObject.name);
#endif
            return;
        }
        playitaNhaydu.SetupPointSpawnInfantry(botIdentity.AssignedPath.PointChindCanMove[0]);
        TF.LookAt(new Vector3(mainCameraTranform.position.x, TF.position.y, mainCameraTranform.position.z ));
    }

    public override void BotDead()
    {
        base.BotDead();
        playitaNhaydu.OnDead();
        BotAudio.PlayAudio(GameConstants.AudioType.BotDeath);
    }
}