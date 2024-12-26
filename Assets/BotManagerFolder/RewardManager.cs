using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    private static RewardManager _instance;
    [SerializeField] private List<RewardNetwork> _rewardNetworks = new List<RewardNetwork>();
    [SerializeField] private int totalRewardsOnMap;
    [SerializeField] private int totalRewardsOnTurn;

    public int TotalRewardsOnMap
    {
        get { return totalRewardsOnMap; }
        set { totalRewardsOnMap = value; }
    }

    public int TotalRewardsOnTurn
    {
        get { return totalRewardsOnTurn; }
        set { totalRewardsOnTurn = value; }
    }

    public static RewardManager Instance => _instance;
    public List<RewardNetwork> RewardNetworks => _rewardNetworks;

    private void Awake()
    {
        _instance = this;
    }

    public void SpawnReward(RewardNetwork rewardPrefab, Vector3 spawnPosition, SpawnRewardPoint path)
    {
        RewardNetwork newReward = Instantiate(rewardPrefab, spawnPosition, Quaternion.identity);
        newReward.SetPath(path);
        newReward.OnRewardCollected += OnRewardCollected;

        newReward.gameObject.SetActive(true);
        _rewardNetworks.Add(newReward);
    }

    private void OnRewardCollected()
    {
        totalRewardsOnMap--;
    }

    // Thêm hàm này để xóa tất cả các phần thưởng khi lượt kết thúc
    public void ClearAllRewards()
    {
        foreach (var reward in _rewardNetworks)
        {
            if (reward.IsCollected)
            {
                Destroy(reward.gameObject);
            }

        }
        _rewardNetworks.Clear();
    }
}
