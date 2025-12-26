using UnityEngine;
using static GameConstants;
public class EnemySpawner : MonoBehaviour
{
    public GameObject infantryPrefab;
    public GameObject tankPrefab;
    public int numberOfBotsToSpawn;
    void Start()
    {
        // Ví dụ: Spawn một làn sóng lính sau 2 giây
        Invoke("SpawnInfantryWave", 2f);
        
        // Ví dụ: Spawn một chiếc xe tăng sau 5 giây
        Invoke("SpawnTank", 5f);
    }

    void SpawnInfantryWave()
    {
        for (int i = 1; i <= numberOfBotsToSpawn; i++)
        {
            SpawnBot(infantryPrefab, BotMoveType.Infantry);
        }
    }

    void SpawnTank()
    {
        SpawnBot(tankPrefab, BotMoveType.Tank);
    }
    
    // Hàm chung để spawn bot
    void SpawnBot(GameObject botPrefab, BotMoveType pathMoveTypeForBotMove)
    {
        // 1. Hỏi PathManager một tuyến đường phù hợp
        PointGroup assignedRoute = PathManager.Instance.GetPath(pathMoveTypeForBotMove);

        // 2. Kiểm tra xem có tìm thấy đường không
        if (assignedRoute != null && assignedRoute.points.Count > 0)
        {
            // 3. Lấy điểm bắt đầu của tuyến đường đó
            Transform startPoint = assignedRoute.points[0];
            
            // 4. Tạo ra con bot tại vị trí đó
            GameObject newBot = Instantiate(botPrefab, startPoint.position, startPoint.rotation);
            
            newBot.GetComponent<BotPathMovement>().SetPath(assignedRoute);
        }
        else
        {
            Debug.LogError($"Không thể spawn bot vì không tìm thấy tuyến đường cho loại {pathMoveTypeForBotMove}");
        }
    }
}