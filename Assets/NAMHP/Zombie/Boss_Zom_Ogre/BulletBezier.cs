using System;
using UnityEngine;

public class BulletBezier : MonoBehaviour,IPoolObject
{
    [Header("Bullet Settings")] 
    [SerializeField]
    private GameObject modelBullet;
    [SerializeField] private AudioSource audioSource;
    [SerializeField]
    private ParticleSystem bulletEffect;
    [SerializeField]
    private GameObject detectTarget;
    [SerializeField]
    private BotNetwork botNetwork;
    
    [Header("Test Settings")]
    public bool testMode = true;
    public Vector3 testEndOffset = new Vector3(5, 0, 5);
    public float testFlightTime = 1.5f;
    public float testCurveHeight = 2f;

    private Vector3 startPoint, controlPoint, endPoint;
    private float duration, height;
    private float elapsed;
    private bool isActive;

    private void OnEnable()
    {
        detectTarget.SetActive(true);
        modelBullet.SetActive(true);
    }

    public void Init(Vector3 start, Vector3 end, float flightTime, float curveHeight)
    {
        startPoint = start;
        endPoint = end;
        duration = flightTime;
        height = curveHeight;
        elapsed = 0f;
        isActive = true;

        controlPoint = (start + end) / 2f;
        controlPoint.y += height;
        
    }

    void Update()
    {
#if UNITY_EDITOR
        if (testMode && !Application.isPlaying)
            return;
#endif
        if (!isActive) return;
        if (botNetwork.IsDead)
        {
            OnArrive();
        }
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        Vector3 pos = Bezier(startPoint, controlPoint, endPoint, t);
        transform.position = pos;

        if (t < 1f)
        {
            Vector3 nextPos = Bezier(startPoint, controlPoint, endPoint, t + 0.01f);
            transform.forward = (nextPos - pos).normalized;
        }
        else
        {
            EventManager.Invoke(EventName.OnTakeDamagePlayer, botNetwork.BotConfigSO.damage);
            EffectUI.Instance.Play();
            OnArrive();
        }
    }

    private void OnArrive()
    {
        isActive = false;
        modelBullet.SetActive(false);
        detectTarget.SetActive(false);
        bulletEffect.Play();
        audioSource.Play();
        Invoke(nameof(ReturnToPool), 3f);
    }

    private void ReturnToPool()
    {
        ObjectPool.Instance.PushToPool(this, gameObject);
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
        Init(start, end, testFlightTime, testCurveHeight);
    }

    public void TestFireEditor()
    {
        UnityEditor.EditorApplication.isPlaying = true;
        Debug.Log("Switching to Play Mode for TestFire");
    }

    public void ResetEditorPosition()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        Debug.Log("Position reset to (0,0,0)");
    }
#endif
    public GameObject Prefab { get; set; }
    public void Init()
    {
        bulletEffect.Stop();
        modelBullet.SetActive(true);
    }

    public void OnPushToPool()
    {
        
    }
}
