using System;
using System.Collections;
using UnityEngine;
using static AircraftY801StateMachine;

public class AircraftY801SpwanState : BaseState<AirForceState>
{
    [SerializeField] private BotConfigSO AircraftConfig;
    [SerializeField] private BotNetwork botNetwork;
    [SerializeField] private Transform spwanPos;
    [SerializeField] private ModeSpawn modeSpawn;
    [SerializeField] private int botsPerRow;
    [SerializeField] private float space;
    [SerializeField] private float delayspawn;// thời gian delay spawn giữa các bot, dùng cho SpawnOneByOne
    private WayPoint _path;
    private float _speed;
    private CarryAttributes[] botCarry;
    private bool isSpwanDone;

    public override void EnterState()
    {
        Init();
        isSpwanDone = false;
        botCarry = new CarryAttributes[AircraftConfig.carryAttributes.Length];
        for (int i = 0; i < AircraftConfig.carryAttributes.Length; i++)
        {
            botCarry[i] = new CarryAttributes
            {
                botConfig = AircraftConfig.carryAttributes[i].botConfig,
                Quantity = AircraftConfig.carryAttributes[i].Quantity
            };
        }
        StartCoroutine(SpwanAction());
    }

    private void Init()
    {
        _path = botNetwork.Path;
        _speed = AircraftConfig.moveSpeed;
    }

    IEnumerator SpwanAction()
    {
        yield return StartCoroutine(SpwanBot(modeSpawn));
        yield return new WaitUntil(() => isSpwanDone == true);
    }

    IEnumerator SpwanBot(ModeSpawn modeSpawn)
    {
        if (botCarry.Length > 0)
        {
            switch (modeSpawn)
            {
                case ModeSpawn.SpawnFullBot:
                    for (int i = 0; i < botCarry[0].Quantity; i++)
                    {
                        int row = i / botsPerRow;
                        int col = i % botsPerRow;
                        Vector3 positionOffset = new Vector3(col * space, 0, row * -space);
                        //Instantiate(botCarry[0].botConfig.Model, spwanPos.position, Quaternion.identity);
                        SpwanCarry(botCarry[0].botConfig.Model, spwanPos.position - positionOffset);
                        yield return null;
                    }
                    isSpwanDone = true;
                    break;
                case ModeSpawn.SpawnOneByOne:
                    for (int i = 0; i < botCarry[0].Quantity; i++)
                    {
                        SpwanCarry(botCarry[0].botConfig.Model, spwanPos.position);
                        yield return new WaitForSeconds(delayspawn);
                    }
                    isSpwanDone = true;
                    break;
            }
        }
    }

    public override void UpdateState()
    {
        if (!isSpwanDone)
        {
            transform.Translate(Vector3.forward * _speed * Time.deltaTime);
        }
    }

    public override void ExitState()
    {
        isSpwanDone = false; // Reset lại isSpwanDone khi rời khỏi trạng thái
    }

    void SpwanCarry(GameObject model, Vector3 offset)
    {
        Instantiate(model, offset, Quaternion.identity);
    }

    public override AirForceState GetNextState()
    {
        if (botNetwork.IsDead)
        {
            return AirForceState.Dead;
        }
        else
        {
            if (isSpwanDone)
            {
                return AirForceState.MoveBehindPos;
            }
            return StateKey;
        }
    }
}

public enum ModeSpawn
{
    SpawnFullBot,
    SpawnOneByOne
}
