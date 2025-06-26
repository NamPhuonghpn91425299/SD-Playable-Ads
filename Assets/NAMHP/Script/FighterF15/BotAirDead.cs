using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotAirDead : MonoBehaviour,IPoolObject
{
    [Header("Falling Parameters")]
    [SerializeField] private float minFallSpeed = 3f;
    [SerializeField] private float maxFallSpeed = 6f;
    [SerializeField] private float minBackwardDrift = 8f;
    [SerializeField] private float maxBackwardDrift = 12f;
    [SerializeField] private float minRotationSpeed = 60f;
    [SerializeField] private float maxRotationSpeed = 120f;

    [Header("Falling Variations")]
    [SerializeField] private AnimationCurve[] fallPathCurves;
    [SerializeField] private float minFallDuration = 2.5f;
    [SerializeField] private float maxFallDuration = 4f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 10f;

    [Header("Effects")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private GameObject Step2;
    [SerializeField] private GameObject _model;

    // Private variables
    private float fallSpeed;
    private float backwardDrift;
    private float rotationSpeed;
    private float fallDuration;
    private AnimationCurve fallPathCurve;

    private bool isFalling;
    private float elapsedTime;
    private Vector3 initialPosition;
    private Vector3 fallDirection;
    private Transform cachedTransform;
    private bool isDead;

    public GameObject Prefab { get; set; }

    void Awake()
    {
        isDead = false;
        cachedTransform = transform;
        RandomizeFallingParameters();
    }

    void RandomizeFallingParameters()
    {
        // Randomize all falling parameters
        fallSpeed = Random.Range(minFallSpeed, maxFallSpeed);
        backwardDrift = Random.Range(minBackwardDrift, maxBackwardDrift);
        rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
        fallDuration = Random.Range(minFallDuration, maxFallDuration);

        // Select random fall path curve
        fallPathCurve = fallPathCurves.Length > 0
            ? fallPathCurves[Random.Range(0, fallPathCurves.Length)]
            : AnimationCurve.Linear(0, 0, 1, 1);

        PrepareFallDirection();
    }

    void PrepareFallDirection()
    {
        // Hướng lùi về sau
        Vector3 backwardVector = -cachedTransform.forward * backwardDrift;

        // Hướng xuống dưới
        Vector3 downVector = Vector3.down * fallSpeed;

        // Kết hợp hai hướng
        Vector3 horizontalRandomVector = new Vector3(
            Random.Range(-backwardDrift * 0.5f, backwardDrift * 0.5f),
            0,
            Random.Range(-backwardDrift * 0.5f, backwardDrift * 0.5f)
        );

        fallDirection = (backwardVector + downVector + horizontalRandomVector).normalized;

    }
    public void OnBotDead()
    {
        if (isFalling) return;
        isFalling = true;
        elapsedTime = 0f;
        initialPosition = cachedTransform.position;
    }
    void Update()
    {
        // Kiểm tra va chạm mặt đất
        if (IsGroundReached() || elapsedTime >= fallDuration && !isDead)
        {
            _model.SetActive(false);
           StartCoroutine(Explode());
           isDead = true;
        }
        else if (!isDead)
        {
            AirFailling();
        }
    }

    private void AirFailling()
    {
        if (!isFalling) return;

        elapsedTime += Time.deltaTime;

        // Quỹ đạo rơi kết hợp trọng lực và chuyển động về sau
        float normalizedTime = elapsedTime / fallDuration;
        float verticalDisplacement = fallPathCurve.Evaluate(normalizedTime) * fallSpeed;
        float horizontalDisplacement = elapsedTime * backwardDrift;

        // Tính toán vị trí mới
        Vector3 newPosition = initialPosition
            - Vector3.up * verticalDisplacement  // Rơi xuống
            - transform.forward * horizontalDisplacement;  // Lùi về sau

        transform.position = newPosition;

        // Xoay máy bay
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime, Space.Self);
    }
    bool IsGroundReached()
    {
        return Physics.Raycast(
                                cachedTransform.position,
                                Vector3.down,
                                groundCheckDistance,
                                groundLayer
        );
    }

    public void SpawnExplode()
    {
        StartCoroutine(Explode());
    }
    private IEnumerator  Explode()
    {
        if (explosionEffect != null)
        {
            var exploision = ObjectPool.Instance.PopFromPool(explosionEffect,instantiateIfNone:true);
            exploision.transform.SetPositionAndRotation(transform.position,Quaternion.identity);
        }

        Step2.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        ObjectPool.Instance.PushToPool(this, gameObject);
    }

# if UNITY_EDITOR
    // Optional debug visualization
    void OnDrawGizmos()
    {
        if (!isFalling) return;

        // Draw fall direction
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(initialPosition, fallDirection * 5f);

        // Draw ground check ray
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);
    }

#endif
    public void Init()
    {

    }

    public void OnPushToPool()
    {

    }
}
