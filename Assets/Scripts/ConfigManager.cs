using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameConstants;

public class ConfigManager : MonoBehaviour
{
    [SerializeField] private StepData[] _stepDatas ;
    public static ConfigManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    public StepData GetStepData(int step)
    {
        return _stepDatas[step];
    }
    public int GetStepCount()
    {
        return _stepDatas.Length;
    }
}
[System.Serializable]
public class StepData
{
    public int botQuantity;
    public BotType Botype;
    public int GetTotalBot()
    {
        return 0;
    }
}
