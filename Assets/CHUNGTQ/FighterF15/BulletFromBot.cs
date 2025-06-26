using System;
using UnityEngine;

public class BulletFromBot : MonoBehaviour, IPoolObject
{
    [SerializeField] private float damage;
    private Vector3 direction;
    [SerializeField] private BotConfigSO botConfigSO;
    [SerializeField] private Transform target;

    [Tooltip("Khoảng cách tối đa viên đạn có thể di chuyển trước khi tự hủy")]
    [SerializeField] private float maxDistance = 2000f;
    
    [Tooltip("Khoảng cách để tính là hit player")]
    [SerializeField] private float hitDistance = 10f; // Có thể điều chỉnh giá trị này

    private Vector3 initialPosition;
    private bool hasHit = false;
    public GameObject Prefab { get; set; }
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitSound;
    [Header("Collision Settings")]
    [SerializeField] private LayerMask playerLayer; // Set this in inspector to player layer
    [SerializeField] private bool destroyOnHit = true;
    [SerializeField] private float minTimeBetweenSounds = 0.1f; // Tránh spam sound
    
    private float lastSoundTime;

    private void Start()
    {
        audioSource.clip = hitSound; // Gán AudioClip vào AudioSource
        audioSource.volume = 0.009f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }

    private void HandleCollision(GameObject hitObject)
    {
        // Kiểm tra xem object va chạm có thuộc player layer không
        if (((1 << hitObject.layer) & playerLayer) != 0)
        {
            // Kiểm tra thời gian giữa các lần phát sound
            if (Time.time - lastSoundTime >= minTimeBetweenSounds)
            {
                PlayHitSound();
                lastSoundTime = Time.time;
            }

            if (destroyOnHit)
            {
                ReturnToPool();
                // Nếu muốn đạn biến mất sau khi va chạm
                
            }
        }
    }

    private void PlayHitSound()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("Missing AudioClip or AudioSource reference!");
        }
    }
    public void Init()
    {
        target = LocalPlayer.Instance.GetTranformPlayer();
    }

    public void Initialize(float damage, Vector3 direction)
    {
        this.damage = damage;
        this.direction = direction.normalized;
        initialPosition = transform.position;
        hasHit = false;
    }

    private void OnEnable()
    {
        this.Initialize(damage, direction);
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (hasHit || target == null) return;

        // Di chuyển viên đạn
        transform.position += direction * (botConfigSO.rocketSpeed * Time.deltaTime);

        // Check hit player
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget < hitDistance)
        {
            hasHit = true;
            //EventManager.Invoke(EventName.OnTakeDamagePlayer, damage);
#if UNITY_EDITOR
            Debug.Log($"Hit player! Distance: {distanceToTarget}, Damage: {damage}");
#endif        
        }

        // Check max distance
        if ((transform.position - initialPosition).sqrMagnitude > maxDistance * maxDistance)        
        {
#if UNITY_EDITOR            
            Debug.Log("Bullet exceeded max distance, returning to pool.");
#endif  
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        ObjectPool.Instance.PushToPool(this, gameObject);
    }

    public void OnPushToPool()
    {
        hasHit = false;
    }
}