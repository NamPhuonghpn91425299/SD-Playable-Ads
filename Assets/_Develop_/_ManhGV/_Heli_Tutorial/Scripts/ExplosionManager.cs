using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dữ liệu của mỗi mảnh vỡ
/// </summary>
public class FragmentData
{
    public Transform transform;
    public Vector3 velocity;
    public Vector3 rotationVelocity;
    public float elapsedTime;
    public Vector3 initialLocalPosition;
    public Quaternion initialLocalRotation;
    public bool isStopped = false;
}

/// <summary>
/// Dữ liệu của một vụ nổ
/// </summary>
[System.Serializable]
public class ExplosionData
{
    public Transform grandParent;           // Cha gốc (cấp 1)
    public Transform explosionParent;       // Parent chứa các object (cấp 2)
    public Transform explosionCenter;       // Tâm vụ nổ (Transform)
    public List<Transform> fragments;       // List các object con (cấp 3)
    
    // Runtime data (không cần set từ ngoài)
    [System.NonSerialized]
    public List<FragmentData> fragmentsData;
    [System.NonSerialized]
    public bool isExploding = false;
    [System.NonSerialized]
    public Vector3 parentInitialLocalPosition;
}

/// <summary>
/// Quản lý các vụ nổ
/// </summary>
public class ExplosionManager : MonoBehaviour
{
    [Header("Shared Parameters - Tham số dùng chung")]
    public float gravity = -9.8f;           // Trọng lực rơi
    public float upwardForce = 5f;          // Độ nổ bay cao lên
    public float explosionRadius = 8f;      // Độ bung (lực ngang)
    public float rotationSpeed = 360f;      // Tốc độ xoay
    public float minY = -10f;               // Vị trí Y tối thiểu
    public LayerMask groundLayer;           // Layer của mặt đất
    public float raycastDistance = 1f;      // Khoảng cách raycast

    [Header("Explosion List")]
    public List<ExplosionData> explosions = new List<ExplosionData>();

    /// <summary>
    /// Khởi tạo tất cả explosions đã có trong list (gọi trong Start hoặc Awake)
    /// </summary>
    public void InitializeExplosions()
    {
        for (int i = 0; i < explosions.Count; i++)
        {
            InitializeExplosion(i);
        }
        Debug.Log($"Initialized {explosions.Count} explosions");
    }

    /// <summary>
    /// Khởi tạo một explosion theo index
    /// </summary>
    public void InitializeExplosion(int index)
    {
        if (index < 0 || index >= explosions.Count)
        {
            Debug.LogError($"Invalid explosion index: {index}");
            return;
        }

        ExplosionData data = explosions[index];

        if (data.explosionParent == null)
        {
            Debug.LogError($"Explosion {index}: explosionParent is null!");
            return;
        }

        if (data.explosionCenter == null)
        {
            Debug.LogError($"Explosion {index}: explosionCenter is null!");
            return;
        }

        if (data.fragments == null || data.fragments.Count == 0)
        {
            Debug.LogError($"Explosion {index}: fragments list is empty!");
            return;
        }

        // Khởi tạo fragmentsData
        data.fragmentsData = new List<FragmentData>();
        data.parentInitialLocalPosition = data.explosionParent.localPosition;

        foreach (Transform fragment in data.fragments)
        {
            if (fragment == null) continue;

            FragmentData fragData = new FragmentData
            {
                transform = fragment,
                velocity = Vector3.zero,
                rotationVelocity = Vector3.zero,
                elapsedTime = 0f,
                initialLocalPosition = fragment.localPosition,
                initialLocalRotation = fragment.localRotation,
                isStopped = false
            };

            data.fragmentsData.Add(fragData);
        }

        data.isExploding = false;
        Debug.Log($"Initialized explosion at index {index}");
    }
    
    /// <summary>
    /// Thêm vụ nổ mới vào list
    /// </summary>
    public void AddExplosion(ExplosionData data)
    {
        if (data == null)
        {
            Debug.LogError("ExplosionData is null!");
            return;
        }

        if (data.explosionParent == null)
        {
            Debug.LogError("explosionParent is null!");
            return;
        }

        if (data.explosionCenter == null)
        {
            Debug.LogError("explosionCenter is null!");
            return;
        }

        if (data.fragments == null || data.fragments.Count == 0)
        {
            Debug.LogError("fragments list is empty!");
            return;
        }

        // Khởi tạo fragmentsData
        data.fragmentsData = new List<FragmentData>();
        data.parentInitialLocalPosition = data.explosionParent.localPosition;

        foreach (Transform fragment in data.fragments)
        {
            if (fragment == null) continue;

            FragmentData fragData = new FragmentData
            {
                transform = fragment,
                velocity = Vector3.zero,
                rotationVelocity = Vector3.zero,
                elapsedTime = 0f,
                initialLocalPosition = fragment.localPosition,
                initialLocalRotation = fragment.localRotation,
                isStopped = false
            };

            data.fragmentsData.Add(fragData);
        }

        explosions.Add(data);
        Debug.Log($"Added explosion at index {explosions.Count - 1}");
    }

    /// <summary>
    /// Kích hoạt vụ nổ theo index
    /// </summary>
    public void TriggerExplosion(int index)
    {
        if (index < 0 || index >= explosions.Count)
        {
            Debug.LogError($"Invalid explosion index: {index}");
            return;
        }

        ExplosionData explosion = explosions[index];

        if (explosion.isExploding)
        {
            Debug.LogWarning($"Explosion {index} is already exploding!");
            return;
        }

        // Tách explosionParent khỏi grandParent
        explosion.explosionParent.parent = null;

        // Tính toán velocity cho từng mảnh
        foreach (FragmentData fragData in explosion.fragmentsData)
        {
            if (fragData.transform == null) continue;

            // Tính hướng từ tâm nổ đến mảnh
            Vector3 direction = (fragData.transform.position - explosion.explosionCenter.position).normalized;

            // Velocity = hướng ngang * explosionRadius + lực bay lên
            fragData.velocity = direction * explosionRadius + Vector3.up * upwardForce;

            // Random rotation velocity
            fragData.rotationVelocity = Random.insideUnitSphere * rotationSpeed;

            // Reset các giá trị
            fragData.elapsedTime = 0f;
            fragData.isStopped = false;
        }

        explosion.isExploding = true;
        Debug.Log($"Triggered explosion {index}");
    }

    /// <summary>
    /// Kích hoạt tất cả vụ nổ
    /// </summary>
    public void TriggerAllExplosions()
    {
        for (int i = 0; i < explosions.Count; i++)
        {
            TriggerExplosion(i);
        }
    }

    /// <summary>
    /// Reset vụ nổ về trạng thái ban đầu
    /// </summary>
    public void ResetExplosion(int index)
    {
        if (index < 0 || index >= explosions.Count)
        {
            Debug.LogError($"Invalid explosion index: {index}");
            return;
        }

        ExplosionData explosion = explosions[index];

        // Gắn lại explosionParent vào grandParent
        if (explosion.grandParent != null)
        {
            explosion.explosionParent.parent = explosion.grandParent;
        }

        // Reset position của explosionParent
        explosion.explosionParent.localPosition = explosion.parentInitialLocalPosition;

        // Reset từng fragment
        foreach (FragmentData fragData in explosion.fragmentsData)
        {
            if (fragData.transform == null) continue;

            fragData.transform.localPosition = fragData.initialLocalPosition;
            fragData.transform.localRotation = fragData.initialLocalRotation;
            fragData.velocity = Vector3.zero;
            fragData.rotationVelocity = Vector3.zero;
            fragData.elapsedTime = 0f;
            fragData.isStopped = false;
        }

        explosion.isExploding = false;
        Debug.Log($"Reset explosion {index}");
    }

    /// <summary>
    /// Reset tất cả vụ nổ
    /// </summary>
    public void ResetAllExplosions()
    {
        for (int i = 0; i < explosions.Count; i++)
        {
            ResetExplosion(i);
        }
    }

    /// <summary>
    /// Dừng vụ nổ tại vị trí hiện tại
    /// </summary>
    public void StopExplosion(int index)
    {
        if (index < 0 || index >= explosions.Count)
        {
            Debug.LogError($"Invalid explosion index: {index}");
            return;
        }

        ExplosionData explosion = explosions[index];
        explosion.isExploding = false;

        // Dừng tất cả fragments
        foreach (FragmentData fragData in explosion.fragmentsData)
        {
            fragData.velocity = Vector3.zero;
            fragData.rotationVelocity = Vector3.zero;
            fragData.isStopped = true;
        }

        Debug.Log($"Stopped explosion {index}");
    }

    /// <summary>
    /// Dừng tất cả vụ nổ
    /// </summary>
    public void StopAllExplosions()
    {
        for (int i = 0; i < explosions.Count; i++)
        {
            StopExplosion(i);
        }
    }

    /// <summary>
    /// Update mỗi frame
    /// </summary>
    private void Update()
    {
        foreach (ExplosionData explosion in explosions)
        {
            if (!explosion.isExploding) continue;

            foreach (FragmentData fragData in explosion.fragmentsData)
            {
                if (fragData.transform == null || fragData.isStopped) continue;

                // Áp dụng trọng lực
                fragData.velocity.y += gravity * Time.deltaTime;

                // Di chuyển
                fragData.transform.position += fragData.velocity * Time.deltaTime;

                // Xoay
                fragData.transform.Rotate(fragData.rotationVelocity * Time.deltaTime, Space.World);

                // Tăng thời gian
                fragData.elapsedTime += Time.deltaTime;

                // Kiểm tra dừng
                bool shouldStop = false;
                Vector3 stopPosition = fragData.transform.position;

                // Kiểm tra minY
                if (fragData.transform.position.y <= minY)
                {
                    shouldStop = true;
                    stopPosition.y = minY;
                }

                // Kiểm tra raycast xuống mặt đất
                RaycastHit hit;
                if (Physics.Raycast(fragData.transform.position, Vector3.down, out hit, raycastDistance, groundLayer))
                {
                    shouldStop = true;
                    stopPosition = hit.point;
                }

                // Nếu cần dừng
                if (shouldStop)
                {
                    fragData.transform.position = stopPosition;
                    fragData.velocity = Vector3.zero;
                    fragData.rotationVelocity = Vector3.zero;
                    fragData.isStopped = true;
                }
            }
        }
    }

    // Vẽ Gizmos để debug
    private void OnDrawGizmos()
    {
        if (explosions == null) return;

        foreach (ExplosionData explosion in explosions)
        {
            if (explosion == null) continue;

            // Vẽ tâm nổ
            if (explosion.explosionCenter != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(explosion.explosionCenter.position, 0.3f);

                // Vẽ bán kính nổ
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(explosion.explosionCenter.position, explosionRadius);
            }
        }
    }
}