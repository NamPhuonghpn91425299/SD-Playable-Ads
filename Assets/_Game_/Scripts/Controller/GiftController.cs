using System.Collections;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;
using static GameConstants;

public class GiftController : MonoBehaviour, IObserver<GameStateChangedEvent>
{
    [SerializeField] private GiftData[] giftDatas;
    void Start()
    {
        EventManager.Instance?.Subscribe<GameStateChangedEvent>(this);
    }

    void OnDisable()
    {
        EventManager.Instance?.Unsubscribe<GameStateChangedEvent>(this);
    }

    public void OnNotify(GameStateChangedEvent data)
    {
        if(data.NewState == GameState.InGame)
            StartCoroutine(SpawnGifts());
    }

    private IEnumerator SpawnGifts()
    {
        foreach (var giftData in giftDatas)
        {
            if(!giftData.CanSpawn)
                continue;
            
            yield return HelperCoroutine.GetWait(giftData.timerDelay);
            HealthObject<Gift> healthObject = SimplePool<Gift>.Spawn<HealthObject<Gift>>(giftData.giftType,giftData.spawnPoint.position, Quaternion.identity);
            healthObject.OnInit();
        }
    }
}

[System.Serializable]
public struct GiftData
{
    public bool CanSpawn;
    public Gift giftType;
    public float timerDelay;
    public Transform spawnPoint;
}
