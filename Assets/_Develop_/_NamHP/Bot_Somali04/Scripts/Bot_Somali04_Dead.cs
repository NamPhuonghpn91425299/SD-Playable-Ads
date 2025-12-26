using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameConstants;
public class Bot_Somali04_Dead : Default_Dead
{

    public override void EnterState()
    {
        botContext.ChangeAnimAndType(HashDeadExplosion, 3);
        botContext.audioPlayable.PlayAudio(GameConstants.AudioType.BotDeath);
    }
    
}
