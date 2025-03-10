using System;
using System.Collections.Generic;
using UnityEngine;
using static GameConstants;

public class PathManager : MonoBehaviour
{
    [SerializeField] public List<WayPointlist> Listwaypoint;
    public static PathManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    // public WayPoint GetWayPoint(BotType botType)
    // {
    //     var wayPointList = Listwaypoint.Find(list => list.botType == botType);
    //     if (wayPointList != null)
    //     {
    //         var paths = wayPointList._wayPointlist;
    //         var availablePaths = paths.FindAll(x => x.IsUse == false);
    //         if (availablePaths.Count == 0)
    //             throw new Exception("No available paths for bot type: " + botType);
    //
    //         int randomIndex = UnityEngine.Random.Range(0, availablePaths.Count);
    //         availablePaths[randomIndex].IsUse = true;
    //         return availablePaths[randomIndex];
    //     }
    //     throw new Exception("No paths found for bot type: " + botType);
    // }
    public WayPoint GetWayPoint(BotType botType)
    {
        var wayPointList = Listwaypoint.Find(list => list.botType == botType);
        if (wayPointList != null)
        {
            var paths = wayPointList._wayPointlist;
        
            // Ưu tiên waypoint chưa được sử dụng
            var availablePaths = paths.FindAll(x => !x.IsUse);
            if (availablePaths.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, availablePaths.Count);
                availablePaths[randomIndex].IsUse = true;
                return availablePaths[randomIndex];
            }

            // Nếu tất cả waypoint đã bị dùng, cho phép dùng lại một waypoint ngẫu nhiên
            Debug.LogWarning($"All paths used for bot type {botType}. Sharing waypoints.");
            int sharedIndex = UnityEngine.Random.Range(0, paths.Count);
            return paths[sharedIndex]; // Không đổi trạng thái IsUse để tránh khóa cứng waypoint
        }
    
        throw new Exception("No paths found for bot type: " + botType);
    }

    public void ResetPath()
    {
        foreach (var wayPointList in Listwaypoint)
        {
            Reset(wayPointList._wayPointlist);
        }
    }

    private void Reset(List<WayPoint> paths)
    {
        foreach (var path in paths)
        {
            path.IsUse = false;
        }
    }
}

[Serializable]
public class WayPointlist
{
    public List<WayPoint> _wayPointlist;
    public BotType botType;
    public GameObject SamplePrefab;
    public int WayPointTotal;
    public List<listLimitWayPoint> listLimitWayPoint;
}

[Serializable]
public class listLimitWayPoint
{
    public List<LimitWayPoint> LimitWayPoint;
}

[Serializable]
public class LimitWayPoint
{
    public List<Transform> Limited;
}

[Serializable]
public class WayPoint
{
    public bool IsUse;
    public List<Transform> WayPoints = new List<Transform>();
    public List<Transform> AttackWayPoints = new List<Transform>();
}
