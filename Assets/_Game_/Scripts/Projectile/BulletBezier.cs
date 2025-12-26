using System;
using UnityEngine;
using Assets._Develop_.ThanhNT.Scripts.Observer;
public class BulletBezier : GameUnit<GameConstants.ProjectileEnemy>,ITakeDamage
{
    public bool DespawnEqualPool = true;
    public float StrengSnakeCam = .025f;
    [Header("Bullet Settings")] 
    [SerializeField]
    private GameObject modelBullet;
    [SerializeField] private AudioSource audioSource;
    [SerializeField]
    private ParticleSystem bulletEffect;
    // [SerializeField]
    // private GameObject detectTarget;
    [SerializeField]
    private int    damage = 10;
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int  _currentHealth;
    private                 bool isDead;
    // Flag để cho projectile biết rocket đã chết vì bị player bắn hạ
    public bool DeadByDamage => isDead;
    
    [Header("Test Settings")]
    public bool testMode = true;
    public Vector3 testEndOffset = new Vector3(5, 0, 5);
    public float testFlightTime = 1.5f;
    public float testCurveHeight = 2f;
    
    [Header("Random Offset Settings")]
    public bool useRandomEndOffset = true;
    public Vector3 randomOffsetRange = new Vector3(2f, 0f, 0f);

    private Vector3 startPoint, controlPoint, endPoint;
    private float   duration,   height;
    private float   elapsed;
    private bool    isActive;

    [SerializeField] private AudioSource audioSourcefire;
    
    private void OnEnable()
    {
        if (GameController.Instance.CurrentGameState != GameConstants.GameState.InGame)
        {
            audioSourcefire.enabled = false;
            audioSource.enabled = false;
        }
        //detectTarget.SetActive(true);
        modelBullet.SetActive(true);
        _currentHealth = maxHealth;
        isDead        = false;
    }

    
    public void Init(Vector3 start, Vector3 end, float flightTime, float curveHeight)
    {
        startPoint = start;
        endPoint = end;
        duration = flightTime;
        height = curveHeight;
        elapsed = 0f;
        isActive = true;

        // Apply random offset to end point if enabled
        if (useRandomEndOffset)
        {
            Vector3 randomOffset = new Vector3(
                UnityEngine.Random.Range(-randomOffsetRange.x, randomOffsetRange.x),
                UnityEngine.Random.Range(-randomOffsetRange.y, randomOffsetRange.y),
                UnityEngine.Random.Range(-randomOffsetRange.z, randomOffsetRange.z)
            );
            endPoint += randomOffset;
        }

        controlPoint = (start + endPoint) / 2f;
        controlPoint.y += height;
        
    }

    void Update()
    {
#if UNITY_EDITOR
        if (testMode && !Application.isPlaying)
            return;
#endif
        if (!isActive) return;
        if (isDead)
        {
            OnArrive();
        }
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        Vector3 pos = Bezier(startPoint, controlPoint, endPoint, t);
        TF.position = pos;

        if (t < 1f)
        {
            Vector3 nextPos = Bezier(startPoint, controlPoint, endPoint, t + 0.01f);
            TF.forward = (nextPos - pos).normalized;
        }
        else
        {
            //EventManager.Invoke(EventName.OnTakeDamagePlayer, botNetwork.BotConfigSO.damage);
            //EffectUI.Instance.Play();
            OnArrive();
        }
    }

    private void OnArrive()
    {
        isActive = false;
        modelBullet.SetActive(false);
       // detectTarget.SetActive(false);
        bulletEffect.Play();
        audioSource.Play();
        if (!isDead)
        {
            EventManager.Instance?.Publish(new PlayerHealthChangedEvent(damage: damage, state: "OnlyDamage"));
            EventManager.Instance?.Publish(new CamShakeEvent(new CamShakeData{duration = .3f,strength = StrengSnakeCam,vibrato = 15,randomness = 45}));
        }
        Invoke(nameof(OnDespawn), 1f);
    }
    
    public void ForceDestroy()
    {
        if (!isActive) return;
        isActive = false;
        //gameObject.SetActive(false);
    }

    Vector3 Bezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        return Mathf.Pow(1 - t, 2) * p0 +
               2 * (1 - t) * t * p1 +
               Mathf.Pow(t, 2) * p2;
    }
    private void OnDespawn()
    {
        //Debug.Log("Damage To Player Is: " + damage);
        if(DespawnEqualPool)
            SimplePool<GameConstants.ProjectileEnemy>.Despawn(this);
        else
            gameObject.SetActive(false);
    }
    
    public void OnTakeDamage(DamageInfo damageInfo)
    {
        _currentHealth -= damageInfo.damage;
        if (_currentHealth <= 0 && !isDead)
        {
            _currentHealth = 0; 
            isDead = true;
        }
    }
    
    public Transform GetTransformThis()
    {
        return transform;
    }

    public Transform GetTransformCenter()
    {
        return transform;
    }
    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (!testMode) return;

        // Nếu đang hoạt động hoặc đã khởi tạo
        Vector3 p0 = startPoint;
        Vector3 p1 = controlPoint;
        Vector3 p2 = endPoint;

        // Nếu chưa khởi tạo => vẽ demo từ vị trí hiện tại
        if (!isActive)
        {
            p0 = transform.position;
            p2 = transform.position + testEndOffset;
            
            // Apply random offset to end point for gizmo visualization if enabled
            if (useRandomEndOffset)
            {
                // For visualization purposes, we'll use a fixed offset within the range
                Vector3 randomOffset = new Vector3(
                    randomOffsetRange.x * 0.5f,
                    randomOffsetRange.y * 0.5f,
                    randomOffsetRange.z * 0.5f
                );
                p2 += randomOffset;
            }
            
            p1 = (p0 + p2) / 2f + Vector3.up * testCurveHeight;
        }

        Gizmos.color = Color.yellow;
        Vector3 prev = p0;
        for (float t = 0; t <= 1f; t += 0.05f)
        {
            Vector3 point = Bezier(p0, p1, p2, t);
            Gizmos.DrawLine(prev, point);
            prev = point;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(p2, 0.2f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(p0, 0.2f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(p1, 0.15f);
#endif
    }


#if UNITY_EDITOR
    // ================= INSPECTOR BUTTONS ===================
    [UnityEditor.CustomEditor(typeof(BulletBezier))]
    public class BulletBezierEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            BulletBezier bullet = (BulletBezier)target;

            UnityEditor.EditorGUILayout.Space();
            UnityEditor.EditorGUILayout.LabelField("Test Tools", UnityEditor.EditorStyles.boldLabel);

            if (UnityEditor.EditorApplication.isPlaying)
            {
                if (GUILayout.Button("▶ Test Fire (Play Mode)"))
                {
                    bullet.TestFire();
                }

                if (GUILayout.Button("✖ Force Destroy"))
                {
                    bullet.ForceDestroy();
                }
            }
            else
            {
                if (GUILayout.Button("▶ Simulate Test (Editor Only)"))
                {
                    bullet.TestFireEditor();
                }

                if (GUILayout.Button("🔄 Reset Position"))
                {
                    bullet.ResetEditorPosition();
                }
            }
        }
    }

    public void TestFire()
    {
        Vector3 start = transform.position;
        Vector3 end = transform.position + testEndOffset;
        
        // Apply random offset to end point if enabled
        if (useRandomEndOffset)
        {
            Vector3 randomOffset = new Vector3(
                UnityEngine.Random.Range(-randomOffsetRange.x, randomOffsetRange.x),
                UnityEngine.Random.Range(-randomOffsetRange.y, randomOffsetRange.y),
                UnityEngine.Random.Range(-randomOffsetRange.z, randomOffsetRange.z)
            );
            end += randomOffset;
        }
        
        Init(start, end, testFlightTime, testCurveHeight);
    }

    public void TestFireEditor()
    {
        UnityEditor.EditorApplication.isPlaying = true;
        Debug.Log("Switching to Play Mode for TestFire");
    }

    public void ResetEditorPosition()
    {
        TF.position = Vector3.zero;
        TF.rotation = Quaternion.identity;
        Debug.Log("Position reset to (0,0,0)");
    }
#endif



}
