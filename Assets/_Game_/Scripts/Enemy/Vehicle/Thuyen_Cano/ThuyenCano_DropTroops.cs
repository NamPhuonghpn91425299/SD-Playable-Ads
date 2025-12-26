using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class ThuyenCano_DropTroops : StateBase
{
    [SerializeField] private BotDefinition[] troopSpawns;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private List<EnemyBase> enemyBases = new List<EnemyBase>();
    private Coroutine coroutineDead;
    public override void EnterState()
    {
        foreach (EnemyBase VARIABLE in enemyBases)
        {
            VARIABLE.TF.parent = null;
            if(VARIABLE.stateController.canDead)
                VARIABLE.stateController.CallEndStart();
        }
        enemyBases.Clear();
        coroutineDead = StartCoroutine(IDMoveDown());
        botContext.stateController.ChangeState(GameConstants.EnemyState.Dead);
    }

    private void OnEnable()
    {
        coroutineDead = null;
    }

    private IEnumerator IDMoveDown(bool wait = true)
    {
        if(wait)
            yield return HelperCoroutine.GetWait(2.5f);
        float timer = 2;
        while (true)
        {
            timer -= Time.deltaTime;
            TF.position += Vector3.down * 1f * Time.deltaTime;
            if (timer < 0)
            {
                botContext.botNetwork.OnDespawn(0f);
                break;
            }
            yield return null;
        }
    }

    public override void UpdateState()
    {
        
    }

    public override void ExitState()
    {
        if(coroutineDead != null)
        {
            StopCoroutine(coroutineDead);
            coroutineDead = null;
        }

        if (enemyBases.Count > 0)
            foreach (EnemyBase VARIABLE in enemyBases)
                VARIABLE.OnTakeDamage(new DamageInfo{damage = 100,damageType = DamageType.Explosion,posExplosion = TF.position});
    }

    private bool dontRemove;
    
    public void InitSpawnBot()
    {
        enemyBases.Clear();
        dontRemove = false;
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
            enemyBaseNew.ACBotDead += BotDead;
            //enemyBaseNew.gameObject.name = i.ToString();
            enemyBaseNew.stateController.SetupStartState(1);
            enemyBaseNew.stateController.OnInit(GameConstants.EnemyState.Start);
            enemyBases.Add(enemyBaseNew);
            
            void BotDead(bool obj)
            {
                enemyBaseNew.ACBotDead -= BotDead;
                if(dontRemove)
                    return;
                try
                {
                    enemyBases.Remove(enemyBaseNew);
                    if (enemyBases.Count <= 0 && coroutineDead !=null)
                    {
                        coroutineDead = StartCoroutine(IDMoveDown(false));
                    }
                }
                catch (Exception e)
                {
                    // Debug.LogError("Lỗi remove bot lính: " + e.Message);
                }
            }
        }
    }
    
    public void CallbotEqualsNull_IfCanoDead()
    {
        //Debug.LogError("Đang lỗi null phần này");
        dontRemove = true;
        foreach (EnemyBase VARIABLE in enemyBases)
        {
            print(VARIABLE.gameObject.name);
            if(VARIABLE.IsDead||VARIABLE.IsDeadExplosion)
                break;
            VARIABLE.OnTakeDamage(new DamageInfo { damage = 1000, damageType = DamageType.Normal });
        }
        enemyBases.Clear();
    }
}