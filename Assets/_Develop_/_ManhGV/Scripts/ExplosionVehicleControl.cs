using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct ExplosionClass
{
    public Transform centerExplosion;
    public Transform originalParents;
    public Transform[] explosionObjects;
    
    [System.NonSerialized]
    public Vector3[] originalLocalPositions;
    [System.NonSerialized]
    public Quaternion[] originalLocalRotations;
}

public class ExplosionVehicleControl : MonoBehaviour
{
    [SerializeField] private Bot_Audio audio;
    
    [Header("Explosion List")]
    public List<ExplosionClass> explosionList = new List<ExplosionClass>();
    
    [Header("Particle Effects")]
    public ParticleSystem explosionParticle;
    
    [Header("Movement Settings")]
    public float explosionForce = 10f;
    public float explosionRadius = 5f;
    public float upwardForce = 5f;
    public float gravity = 9.81f;
    public float rotationSpeed = 360f;
    
    [Header("Ground Detection")]
    public LayerMask groundLayerMask = -1; // -1 = Everything
    public float groundCheckDistance = 100f;
    public float raycastDistance = 25f;
    public float fallbackGroundY = 0f; // Nếu không tìm thấy ground, rơi xuống Y=0
    
#if UNITY_EDITOR
    [Header("Controls - Editor Only")]
    public KeyCode explosionKey = KeyCode.Space;
    public KeyCode resetKey = KeyCode.R;
    
    [Header("Test Controls - Editor Only")]
    [Range(0, 10)]
    public int testExplosionIndex = 0;
#endif
    
    // Optimized data structures
    private bool[] explosionStates;
    private Vector3[] groundHitPoints;
    private bool[] groundCalculatedStates;
    
    // Pooled arrays - MỖI OBJECT HOÀN TOÀN ĐỘC LẬP
    private Vector3[][] currentVelocitiesList;
    private Vector3[][] angularVelocitiesList;
    private bool[][] hasLandedList; // Flag riêng cho từng object
    private Vector3[][] startPositionsList;
    private float[][] startTimesList;
    
    // Cache
    private int explosionCount;
    private float deltaTime;
    private readonly Vector3 gravityVector = Vector3.down;
    
    private WaitForEndOfFrame waitFrame;

    void Start()
    {
        waitFrame = new WaitForEndOfFrame();
        InitializeAllExplosions();
    }

    void Update()
    {
#if UNITY_EDITOR
        HandleEditorInput();
#endif
        UpdateAllExplosionMovements();
    }

#if UNITY_EDITOR
    void HandleEditorInput()
    {
        if (Input.GetKeyDown(explosionKey))
        {
            TriggerExplosion(testExplosionIndex);
        }
        
        if (Input.GetKeyDown(resetKey))
        {
            ResetExplosion(testExplosionIndex);
        }
    }
#endif

    void InitializeAllExplosions()
    {
        if (explosionList == null || explosionList.Count == 0)
        {
#if UNITY_EDITOR
            Debug.LogWarning("Explosion list is empty!");
#endif
            return;
        }

        explosionCount = explosionList.Count;
        
        explosionStates = new bool[explosionCount];
        groundHitPoints = new Vector3[explosionCount];
        groundCalculatedStates = new bool[explosionCount];
        
        currentVelocitiesList = new Vector3[explosionCount][];
        angularVelocitiesList = new Vector3[explosionCount][];
        hasLandedList = new bool[explosionCount][];
        startPositionsList = new Vector3[explosionCount][];
        startTimesList = new float[explosionCount][];

        for (int explosionIndex = 0; explosionIndex < explosionCount; explosionIndex++)
        {
            InitializeSingleExplosion(explosionIndex);
        }
    }

    void InitializeSingleExplosion(int explosionIndex)
    {
        ExplosionClass explosion = explosionList[explosionIndex];
        
        explosionStates[explosionIndex] = false;
        groundHitPoints[explosionIndex] = new Vector3(0, fallbackGroundY, 0);
        groundCalculatedStates[explosionIndex] = false;
        
        if (explosion.explosionObjects != null && explosion.explosionObjects.Length > 0)
        {
            int objectCount = explosion.explosionObjects.Length;
            
            explosion.originalLocalPositions = new Vector3[objectCount];
            explosion.originalLocalRotations = new Quaternion[objectCount];
            
            // Tạo arrays riêng biệt cho từng explosion
            currentVelocitiesList[explosionIndex] = new Vector3[objectCount];
            angularVelocitiesList[explosionIndex] = new Vector3[objectCount];
            hasLandedList[explosionIndex] = new bool[objectCount];
            startPositionsList[explosionIndex] = new Vector3[objectCount];
            startTimesList[explosionIndex] = new float[objectCount];
            
            for (int i = 0; i < objectCount; i++)
            {
                if (explosion.explosionObjects[i] != null)
                {
                    explosion.originalLocalPositions[i] = explosion.explosionObjects[i].localPosition;
                    explosion.originalLocalRotations[i] = explosion.explosionObjects[i].localRotation;
                    
                    // Init riêng từng object
                    currentVelocitiesList[explosionIndex][i] = Vector3.zero;
                    angularVelocitiesList[explosionIndex][i] = Vector3.zero;
                    hasLandedList[explosionIndex][i] = false;
                    startPositionsList[explosionIndex][i] = Vector3.zero;
                    startTimesList[explosionIndex][i] = 0f;
                }
            }
            
            explosionList[explosionIndex] = explosion;
            
#if UNITY_EDITOR
            Debug.Log($"Initialized explosion {explosionIndex} with {objectCount} objects");
#endif
        }
        else
        {
            currentVelocitiesList[explosionIndex] = new Vector3[0];
            angularVelocitiesList[explosionIndex] = new Vector3[0];
            hasLandedList[explosionIndex] = new bool[0];
            startPositionsList[explosionIndex] = new Vector3[0];
            startTimesList[explosionIndex] = new float[0];
        }
    }

    public void TriggerExplosion(int explosionIndex)
    {
        if (!ValidateExplosionIndex(explosionIndex)) return;
        audio?.PlayAudio(GameConstants.AudioType.BotDeath);
        
        if (explosionStates[explosionIndex])
        {
#if UNITY_EDITOR
            Debug.LogWarning($"Explosion {explosionIndex} is already active!");
#endif
            return;
        }

        ExplosionClass explosion = explosionList[explosionIndex];

        if (!groundCalculatedStates[explosionIndex])
        {
            CalculateGroundHitPoint(explosionIndex);
        }

        if (explosion.centerExplosion == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"Center explosion not assigned for explosion {explosionIndex}!");
#endif
            return;
        }

        PlayExplosionParticle();

        explosionStates[explosionIndex] = true;
        Vector3 explosionCenter = explosion.centerExplosion.position;
        
        Vector3[] velocities = currentVelocitiesList[explosionIndex];
        Vector3[] angularVelocities = angularVelocitiesList[explosionIndex];
        bool[] hasLanded = hasLandedList[explosionIndex];
        Vector3[] startPositions = startPositionsList[explosionIndex];
        float[] startTimes = startTimesList[explosionIndex];

        for (int i = 0; i < explosion.explosionObjects.Length; i++)
        {
            if (explosion.explosionObjects[i] == null) continue;
            
            Transform obj = explosion.explosionObjects[i];
            
            obj.SetParent(null);
            
            startPositions[i] = obj.position;
            startTimes[i] = Time.time;
            
            Vector3 objPos = obj.position;
            Vector3 direction = (objPos - explosionCenter);
            float distance = direction.magnitude;
            
            if (distance > 0.001f)
            {
                direction = direction / distance;
            }
            else
            {
                direction = Random.onUnitSphere;
            }
            
            float forceMultiplier = Mathf.Max(0f, 1f - (distance / explosionRadius));
            
            float randomForceMult = Random.Range(0.8f, 1.2f);
            Vector3 explosionVelocity = direction * (explosionForce * forceMultiplier * randomForceMult);
            explosionVelocity.y += upwardForce * Random.Range(0.7f, 1.3f);
            
            velocities[i] = explosionVelocity;
            
            angularVelocities[i] = new Vector3(
                Random.Range(-rotationSpeed, rotationSpeed),
                Random.Range(-rotationSpeed, rotationSpeed),
                Random.Range(-rotationSpeed, rotationSpeed)
            );
            
            hasLanded[i] = false; // Reset flag cho object này
        }

#if UNITY_EDITOR
        Debug.Log($"✓ Explosion {explosionIndex} triggered with {explosion.explosionObjects.Length} objects");
#endif
    }

    void UpdateAllExplosionMovements()
    {
        deltaTime = Time.deltaTime;
        if (deltaTime <= 0f) return;
        
        for (int explosionIndex = 0; explosionIndex < explosionCount; explosionIndex++)
        {
            if (!explosionStates[explosionIndex]) continue;
            
            UpdateSingleExplosionMovement(explosionIndex);
        }
    }

    void UpdateSingleExplosionMovement(int explosionIndex)
    {
        ExplosionClass explosion = explosionList[explosionIndex];
        Vector3[] velocities = currentVelocitiesList[explosionIndex];
        Vector3[] angularVelocities = angularVelocitiesList[explosionIndex];
        bool[] hasLanded = hasLandedList[explosionIndex];
        float[] startTimes = startTimesList[explosionIndex];
        float groundY = groundHitPoints[explosionIndex].y;
        
        float gravityDelta = gravity * deltaTime;
        
        // *** LOOP QUA TỪNG OBJECT - HOÀN TOÀN ĐỘC LẬP ***
        for (int i = 0; i < explosion.explosionObjects.Length; i++)
        {
            if (explosion.explosionObjects[i] == null) continue;
            
            // *** QUAN TRỌNG: CHỈ BỎ QUA OBJECT NÀY NẾU NÓ ĐÃ LANDED ***
            if (hasLanded[i])
            {
                continue; // Object này đã dừng, KHÔNG ẢNH HƯỞNG objects khác
            }
            
            Transform obj = explosion.explosionObjects[i];
            float timeElapsed = Time.time - startTimes[i];
            
            // Áp dụng gravity - tăng dần theo thời gian
            float gravityMultiplier = Mathf.Lerp(2f, 5f, timeElapsed / 5f);
            velocities[i].y -= gravityDelta * gravityMultiplier;
            
            // Clamp velocity
            velocities[i].y = Mathf.Max(velocities[i].y, -40f);
            
            // Di chuyển object
            Vector3 oldPos = obj.position;
            Vector3 newPos = oldPos + velocities[i] * deltaTime;
            obj.position = newPos;
            
            // Xoay object
            angularVelocities[i] *= 0.995f;
            obj.Rotate(angularVelocities[i] * deltaTime, Space.Self);
            
            // *** KIỂM TRA LANDING - 4 METHODS ĐỘC LẬP ***
            bool shouldLandThisObject = false;
            float landingY = fallbackGroundY; // Default fallback
            
            // Method 1: Raycast xuống dưới từ vị trí cũ
            if (Physics.Raycast(oldPos, Vector3.down, out RaycastHit hit1, raycastDistance, groundLayerMask))
            {
                if (hit1.distance <= 1f && velocities[i].y <= 0)
                {
                    shouldLandThisObject = true;
                    landingY = hit1.point.y;
#if UNITY_EDITOR
                    Debug.Log($"[{explosionIndex}][{i}] Landed via Method 1 at Y={landingY:F2}");
#endif
                }
            }
            
            // Method 2: Raycast từ vị trí mới
            if (!shouldLandThisObject && Physics.Raycast(newPos, Vector3.down, out RaycastHit hit2, raycastDistance, groundLayerMask))
            {
                if (hit2.distance <= 1f && velocities[i].y <= 0)
                {
                    shouldLandThisObject = true;
                    landingY = hit2.point.y;
#if UNITY_EDITOR
                    Debug.Log($"[{explosionIndex}][{i}] Landed via Method 2 at Y={landingY:F2}");
#endif
                }
            }
            
            // Method 3: Raycast từ trên xuống
            if (!shouldLandThisObject && Physics.Raycast(newPos + Vector3.up * 5f, Vector3.down, out RaycastHit hit3, 10f, groundLayerMask))
            {
                if (newPos.y <= hit3.point.y + 0.5f && velocities[i].y <= 0)
                {
                    shouldLandThisObject = true;
                    landingY = hit3.point.y;
#if UNITY_EDITOR
                    Debug.Log($"[{explosionIndex}][{i}] Landed via Method 3 at Y={landingY:F2}");
#endif
                }
            }
            
            // Method 4: Fallback - Rơi xuống groundY hoặc Y=0
            if (!shouldLandThisObject)
            {
                float targetGroundY = groundY != 0 ? groundY : fallbackGroundY;
                
                if (newPos.y <= targetGroundY && velocities[i].y <= 0)
                {
                    shouldLandThisObject = true;
                    landingY = targetGroundY;
#if UNITY_EDITOR
                    Debug.Log($"[{explosionIndex}][{i}] Landed via Method 4 (Fallback) at Y={landingY:F2}");
#endif
                }
            }
            
            // Method 5: Safety timeout
            if (!shouldLandThisObject && timeElapsed > 15f)
            {
                shouldLandThisObject = true;
                landingY = fallbackGroundY;
#if UNITY_EDITOR
                Debug.LogWarning($"[{explosionIndex}][{i}] FORCE LANDED after timeout at Y={landingY:F2}");
#endif
            }
            
            // *** XỬ LÝ LANDING CHO OBJECT NÀY (KHÔNG ẢNH HƯỞNG OBJECTS KHÁC) ***
            if (shouldLandThisObject)
            {
                Vector3 landingPosition = obj.position;
                landingPosition.y = landingY;
                obj.position = landingPosition;
                
                // Tính bounce
                float bounceStrength = Mathf.Abs(velocities[i].y) * 0.3f;
                
                if (bounceStrength > 0.5f)
                {
                    // Còn bounce cao
                    velocities[i].y = bounceStrength;
                    velocities[i].x *= 0.75f;
                    velocities[i].z *= 0.75f;
                    angularVelocities[i] *= 0.85f;
                }
                else
                {
                    // Dừng hẳn - CHỈ OBJECT NÀY
                    velocities[i] = Vector3.zero;
                    angularVelocities[i] = Vector3.zero;
                    hasLanded[i] = true;
                    
#if UNITY_EDITOR
                    Debug.Log($"✓✓✓ [{explosionIndex}][{i}] FULLY LANDED at Y={landingY:F2} ✓✓✓");
#endif
                }
            }
            
            // *** FORCE RƠI XUỐNG 0 NẾU VẪN ĐANG TREO TRÊN KHÔNG ***
            if (!hasLanded[i] && obj.position.y > fallbackGroundY + 0.1f)
            {
                // Đảm bảo object luôn có velocity rơi xuống
                if (velocities[i].y > -1f)
                {
                    velocities[i].y = -5f;
                }
            }
        }
    }

    public void ResetExplosion(int explosionIndex)
    {
        if (!ValidateExplosionIndex(explosionIndex)) return;
        
        if (!explosionStates[explosionIndex])
        {
#if UNITY_EDITOR
            Debug.LogWarning($"Explosion {explosionIndex} is not active!");
#endif
            return;
        }

        ExplosionClass explosion = explosionList[explosionIndex];
        Vector3[] velocities = currentVelocitiesList[explosionIndex];
        Vector3[] angularVelocities = angularVelocitiesList[explosionIndex];
        bool[] hasLanded = hasLandedList[explosionIndex];
        float[] startTimes = startTimesList[explosionIndex];

        for (int i = 0; i < explosion.explosionObjects.Length; i++)
        {
            if (explosion.explosionObjects[i] == null) continue;
            
            Transform obj = explosion.explosionObjects[i];
            
            if (explosion.originalParents != null)
            {
                obj.SetParent(explosion.originalParents);
            }
            
            obj.localPosition = explosion.originalLocalPositions[i];
            obj.localRotation = explosion.originalLocalRotations[i];
            
            // Reset từng object riêng biệt
            velocities[i] = Vector3.zero;
            angularVelocities[i] = Vector3.zero;
            hasLanded[i] = false;
            startTimes[i] = 0f;
        }

        explosionStates[explosionIndex] = false;
        groundCalculatedStates[explosionIndex] = false;
        
#if UNITY_EDITOR
        Debug.Log($"✓ Explosion {explosionIndex} reset");
#endif
    }

    public void ResetAllExplosions()
    {
        for (int i = 0; i < explosionCount; i++)
        {
            if (explosionStates[i])
            {
                ResetExplosion(i);
            }
        }
#if UNITY_EDITOR
        Debug.Log("✓ All explosions reset!");
#endif
    }

    void CalculateGroundHitPoint(int explosionIndex)
    {
        ExplosionClass explosion = explosionList[explosionIndex];
        if (explosion.centerExplosion == null) return;

        Vector3 rayStart = explosion.centerExplosion.position;
        float maxDistance = groundCheckDistance * 3f;
        
        // Raycast từ nhiều điểm
        Vector3[] rayStartPositions = new Vector3[]
        {
            rayStart,
            rayStart + Vector3.left * 10f,
            rayStart + Vector3.right * 10f,
            rayStart + Vector3.forward * 10f,
            rayStart + Vector3.back * 10f,
            rayStart + new Vector3(5f, 0, 5f),
            rayStart + new Vector3(-5f, 0, -5f)
        };
        
        float lowestY = float.MaxValue;
        bool foundGround = false;
        
        foreach (var startPos in rayStartPositions)
        {
            if (Physics.Raycast(startPos, Vector3.down, out RaycastHit hit, maxDistance, groundLayerMask))
            {
                if (hit.point.y < lowestY)
                {
                    lowestY = hit.point.y;
                    groundHitPoints[explosionIndex] = hit.point;
                    foundGround = true;
                }
            }
        }
        
        if (foundGround)
        {
            groundCalculatedStates[explosionIndex] = true;
#if UNITY_EDITOR
            Debug.Log($"✓ Ground found at Y={lowestY:F2} for explosion {explosionIndex}");
#endif
        }
        else
        {
            // Fallback: Dùng Y = fallbackGroundY (0)
            groundHitPoints[explosionIndex] = new Vector3(rayStart.x, fallbackGroundY, rayStart.z);
            groundCalculatedStates[explosionIndex] = true;
#if UNITY_EDITOR
            Debug.LogWarning($"✗ NO GROUND FOUND for explosion {explosionIndex}! Using fallback Y={fallbackGroundY}");
#endif
        }
    }

    void PlayExplosionParticle()
    {
        if (explosionParticle != null)
        {
            explosionParticle.Play();
        }
    }

    bool ValidateExplosionIndex(int explosionIndex)
    {
        if (explosionIndex < 0 || explosionIndex >= explosionCount)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"Invalid explosion index: {explosionIndex}");
#endif
            return false;
        }
        return true;
    }

    public bool IsExplosionComplete(int explosionIndex)
    {
        if (!ValidateExplosionIndex(explosionIndex) || !explosionStates[explosionIndex])
            return true;
        
        bool[] hasLanded = hasLandedList[explosionIndex];
        ExplosionClass explosion = explosionList[explosionIndex];
        
        for (int i = 0; i < explosion.explosionObjects.Length; i++)
        {
            if (explosion.explosionObjects[i] != null && !hasLanded[i])
            {
                return false;
            }
        }
        
        return true;
    }

    public void TriggerExplosionByIndex(int index)
    {
        TriggerExplosion(index);
    }

    public void ResetExplosionByIndex(int index)
    {
        ResetExplosion(index);
    }

    public bool IsExplosionActive(int index)
    {
        return ValidateExplosionIndex(index) && explosionStates[index];
    }

    public int GetExplosionCount()
    {
        return explosionCount;
    }

#if UNITY_EDITOR
    void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 13;
        labelStyle.normal.textColor = Color.white;
        
        GUILayout.Label("═══ EXPLOSION DEBUG ═══", labelStyle);
        
        for (int explosionIndex = 0; explosionIndex < explosionCount; explosionIndex++)
        {
            if (explosionStates[explosionIndex])
            {
                bool[] hasLanded = hasLandedList[explosionIndex];
                ExplosionClass explosion = explosionList[explosionIndex];
                
                int totalObjects = explosion.explosionObjects?.Length ?? 0;
                int landedObjects = 0;
                int fallingObjects = 0;
                
                for (int i = 0; i < totalObjects; i++)
                {
                    if (hasLanded[i]) 
                        landedObjects++;
                    else
                        fallingObjects++;
                }
                
                bool isComplete = IsExplosionComplete(explosionIndex);
                labelStyle.normal.textColor = isComplete ? Color.green : Color.yellow;
                
                string status = isComplete ? "✓ COMPLETE" : $"⚠ FALLING: {fallingObjects}";
                GUILayout.Label($"Explosion {explosionIndex}: {landedObjects}/{totalObjects} landed - {status}", labelStyle);
            }
        }
        GUILayout.EndArea();
    }

    void OnDrawGizmosSelected()
    {
        if (explosionList == null || explosionList.Count == 0) return;
        
        for (int explosionIndex = 0; explosionIndex < explosionList.Count; explosionIndex++)
        {
            ExplosionClass explosion = explosionList[explosionIndex];
            if (explosion.centerExplosion == null) continue;
            
            Gizmos.color = Color.HSVToRGB((float)explosionIndex / explosionList.Count, 1f, 1f);
            Gizmos.DrawWireSphere(explosion.centerExplosion.position, explosionRadius);
            
            Gizmos.color = Color.yellow;
            Vector3 rayStart = explosion.centerExplosion.position;
            Gizmos.DrawLine(rayStart, rayStart + Vector3.down * groundCheckDistance);
            
            if (Application.isPlaying && explosionIndex < groundCalculatedStates.Length && 
                groundCalculatedStates[explosionIndex])
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundHitPoints[explosionIndex], 1f);
                Gizmos.DrawLine(explosion.centerExplosion.position, groundHitPoints[explosionIndex]);
            }
        }
    }
#endif
}