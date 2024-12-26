using UnityEngine;

public class HeadShotIcon : MonoBehaviour, IPoolObject
{
    public GameObject Prefab { get; set; }

    public void Init()
    {
        // Khởi tạo lại đối tượng khi nó được lấy từ pool
    }

    public void OnPushToPool()
    {
        // Reset lại đối tượng trước khi trả về pool
    }
}
