using System;
using System.Collections.Generic;
using static GameConstants;
using UnityEngine;
using Random = UnityEngine.Random;

public class Helli_MH6_DropTroops : StateBase
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private BotDefinition[] troopSpawns;
    private List<StateControllerBase> stateControllerS = new List<StateControllerBase>();
    private float timer = 2;

    public override void EnterState()
    {
        timer = 2;
        foreach (StateControllerBase VARIABLE in stateControllerS)
        {
            VARIABLE.transform.parent = null;
            if(VARIABLE.canDead)
                VARIABLE.CallEndStart();
        }
        stateControllerS.Clear();
    }   

    public override void UpdateState()
    {
        timer -= Time.deltaTime;
        if(timer <= 0)
            botContext.stateController.ChangeState((EnemyState.Move));
    }

    public override void ExitState()
    {
        
    }
    
    /// <summary>
    /// Spawn bots ngồi trên máy bay 
    /// </summary>
    public void SpawnBots()
    {
        stateControllerS.Clear();
        //Spawn troops when the state is enabled
        List<PointGroup> pointGroups = botContext.botIdentity.AssignedPath.PointChindCanMove;
#if UNITY_EDITOR
        if(pointGroups.Count < spawnPoints.Length)
        {
            Debug.LogError("Số lượng điểm nhóm ít hơn hoặc bằng số điểm spawn, có thể không đủ điểm để spawn tất cả các quân lính.");
            return;
        }
#endif
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            // EnemyBase enemyBaseNew = SimplePool<BotType>.Spawn<EnemyBase>(troopSpawnTypes[Random.Range(0,troopSpawnTypes.Length)],spawnPoints[i].position, spawnPoints[i].rotation,spawnPoints[i]);
            EnemyBase enemyBaseNew = BotSpawnManager.Instance.ExecuteSpawnOrder(troopSpawns[Random.Range(0, troopSpawns.Length)], spawnPoints[i],pointGroups[i],true);
            enemyBaseNew.OnInit();
            enemyBaseNew.gameObject.name = i.ToString();
            enemyBaseNew.stateController.SetupStartState(0);
            enemyBaseNew.stateController.OnInit(GameConstants.EnemyState.Start);
            stateControllerS.Add(enemyBaseNew.stateController);
        }
    }
    
    public void CallbotEqualsNull_IfCanoDead()
    {
        foreach (StateControllerBase VARIABLE in stateControllerS)
        {
            VARIABLE.transform.parent = null;
            VARIABLE.ChangeState(EnemyState.DeadExplosionHelicopter);
        }
        stateControllerS.Clear();
    }
}