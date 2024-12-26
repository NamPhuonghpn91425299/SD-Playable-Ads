using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

public class aircraft_Y8_AirDefense : MonoBehaviour
{
    public BotNetwork botNetwork;
    [SerializeField] private GameObject Exlosion;

    [SerializeField] private bool isDead;
    [Header("Setup point ")]
    [SerializeField] private float dropSpeed;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Transform headAirPlane;

    private float currentAngle;
    private Vector3 landPos;
    private Vibration _playerVibration;
    [Header("Sound ")]
    [SerializeField] private AudioSource sound;
    [SerializeField] private F15TrackingMovement f15TrackingMovement;
    [SerializeField] private float dropPos;
    public List<FanDetector> fanDetectors;
    public Dictionary<string, FanDetector> hpBot = new Dictionary<string, FanDetector>();
    private int countDeadFan;
    private Transform myTrans;
    private bool isMoveDone;

    private bool WeaknessDestroyed => fanDetectors.All(e => e.IsDead);
    private FanDetector CanDestroyedWeakness
    {
        get
        {
            var data = fanDetectors.Where(e => !e.IsDead);

            return data.OrderBy(e => e.RemainHealth).FirstOrDefault();
        }
    }
    private void Awake()
    {
        f15TrackingMovement.enabled = false;
        myTrans = transform;
        hpBot = fanDetectors.ToHashSet().ToDictionary(e => e.name, e => e);
        
    }
    void OnEnable()
    {
        countDeadFan = (int)(botNetwork.BotConfigSO.health / botNetwork.BotConfigSO.WeaknessHealth);
        currentAngle = 0;
        isDead = false;
        botNetwork.OnBotDead += BotDead;
        botNetwork.OnWeaknessTakeDamage += OnWeaknessTakeDamage;
        botNetwork.OnHealthChanged += OnHeathChange;
        _playerVibration = LocalPlayer.Instance.GetComponent<Vibration>();

    }
    private void OnHeathChange(float obj)
    {
        var persentFan = botNetwork.BotConfigSO.WeaknessHealth / botNetwork.BotConfigSO.health;
        var persentDestroy = obj / persentFan;
        if (persentDestroy <= countDeadFan - 1)
        {
            countDeadFan--;
            CanDestroyedWeakness?.TryHandleDamage(9999);
        }
    
    }

    private void BotDead()
    {
            f15TrackingMovement.enabled = false;
            //_playerVibration.StartShaking(0, 10);
            float targetAngle = Random.Range(50f, 60f);
            StartCoroutine(StartOnDead(targetAngle));
        
    }

    private void OnWeaknessTakeDamage(string weaknessName, int damage)
    {
        if (hpBot.TryGetValue(weaknessName, out FanDetector fan))
        {

            if (!fan.IsDead && !fan.TryHandleDamage(damage))
            {
                countDeadFan--;
            }

            if (WeaknessDestroyed)
            {
                botNetwork.TakeDamage(new DamageInfo() { damage = 99999 });
            }

        }

    }
    IEnumerator StartOnDead(float targetAngle)
    {

        startSpawnExplosion();
        _playerVibration.StartShaking(0, 0.4f);

        Quaternion startRotation = transform.rotation;

        while (currentAngle < targetAngle)
        {
            float rotateAmount = rotateSpeed * Time.deltaTime;
            currentAngle += rotateAmount;

            transform.Rotate(Vector3.right, rotateAmount);

            yield return null;
        }

        transform.rotation = startRotation * Quaternion.Euler(targetAngle, 0f, 0f);

        RaycastHit dropPosHit;
        if (Physics.Raycast(headAirPlane.position, headAirPlane.forward, out dropPosHit, 1000, groundMask))
        {
            landPos = dropPosHit.point;
            Vector3 dirNormal = (landPos - headAirPlane.position).normalized;
            landPos += dirNormal * dropPos; // đâm xuyên núi luôn
              Debug.Log("Chạm đất!");
        }
        else
        {
            landPos = transform.position + new Vector3(0, 80, 100);
        }
        Debug.DrawLine(transform.position, landPos, Color.red, 15f);
        while (transform.position.y - landPos.y > 1)
        {
            transform.position = Vector3.MoveTowards(transform.position, landPos, dropSpeed * Time.deltaTime);
            yield return null;
        }
        sound.Stop();
        gameObject.SetActive(false);
    }

    void startSpawnExplosion()
    {
        var explosoin = ObjectPool.Instance.PopFromPool(Exlosion,instantiateIfNone:true);
        explosoin.transform.SetPositionAndRotation(transform.position, transform.rotation);

    }
    
}
