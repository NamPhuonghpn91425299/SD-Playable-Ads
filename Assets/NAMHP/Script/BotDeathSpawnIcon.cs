using UnityEngine;

public class BotDeathSpawnIcon : MonoBehaviour
{
    public GameObject iconDeathPrefab;
    public BotNetwork botNetwork;
    public Transform botTransform; // Transform của bot để lấy vị trí

    private void OnEnable()
    {
        botNetwork.OnBotDead += OnBotDead;
    }
    
    private void OnDisable()
    {
        botNetwork.OnBotDead -= OnBotDead;
    }

    private void OnBotDead()
    {
        // Lấy vị trí của bot khi chết
        Vector3 spawnPosition = botTransform.position;

        // Spawn icon từ Object Pool hoặc Instantiate nếu cần
        GameObject icon = ObjectPool.Instance.PopFromPool(iconDeathPrefab, instantiateIfNone: true);
        icon.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
        icon.SetActive(true);

        // Gọi hiệu ứng bay lên + scale rồi biến mất
        icon.GetComponent<IconEffect>().StartEffect();
    }
}