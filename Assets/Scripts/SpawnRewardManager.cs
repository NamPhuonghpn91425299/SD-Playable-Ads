using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameConstants;

public class SpawnRewardManager : MonoBehaviour
{
    public static SpawnRewardManager Instance;

    [SerializeField] public List<SpawnRewardList> spawnRewardList;

    private void Awake()
    {
        
        Instance = this;    
    }

    // public SpawnRewardPoint GetSpawnPoint(RewardType rewardType)
    // {
    //     var wayPointList = spawnRewardList.Find(list => list.rewardType == rewardType);
    //     if (wayPointList != null)
    //     {
    //         var paths = wayPointList.spawnPointPoint;
    //         var availablePaths = paths.FindAll(x => x.IsUse == false);
    //         if (availablePaths.Count == 0)
    //         {
    //             // throw new Exception("No available paths for bot type: " + rewardType);
    //             return paths[UnityEngine.Random.Range(0, paths.Count)];
    //         }
    //
    //         int randomIndex = UnityEngine.Random.Range(0, availablePaths.Count);
    //         availablePaths[randomIndex].IsUse = true;
    //         return availablePaths[randomIndex];
    //     }
    //     throw new Exception("No paths found for bot type: " + rewardType);
    // }
    public SpawnRewardPoint GetSpawnPoint(RewardType rewardType)
    {
        var wayPointList = spawnRewardList.Find(list => list.rewardType == rewardType);
        if (wayPointList != null)
        {
            var paths = wayPointList.spawnPointPoint;

            if (paths == null || paths.Count == 0)
            {
                throw new Exception("No paths available for reward type: " + rewardType);
            }

            var availablePaths = paths.FindAll(x => x.IsUse == false);
            if (availablePaths.Count == 0)
            {
                // Không có đường khả dụng, chọn ngẫu nhiên trong toàn bộ danh sách paths
                return paths[UnityEngine.Random.Range(0, paths.Count)];
            }

            int randomIndex = UnityEngine.Random.Range(0, availablePaths.Count);
            availablePaths[randomIndex].IsUse = true;
            return availablePaths[randomIndex];
        }

        throw new Exception("No paths found for reward type: " + rewardType);
    }

    public void ResetPoint()
    {
        foreach (var wayPointList in spawnRewardList)
        {
            Reset(wayPointList.spawnPointPoint);
        }
    }

    private void Reset(List<SpawnRewardPoint> paths)
    {
        foreach (var path in paths)
        {
            path.IsUse = false;
        }
    }
}

[Serializable]
public class SpawnRewardList
{

    public List<SpawnRewardPoint> spawnPointPoint = new List<SpawnRewardPoint>();
    public RewardType rewardType;
}

[Serializable]
public class SpawnRewardPoint
{
    public bool IsUse;
    public List<Transform> SpawnPoint = new List<Transform>();
}
