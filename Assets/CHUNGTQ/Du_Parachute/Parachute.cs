using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using static BotPlayItaStateMachine;

public class Parachute : MonoBehaviour, IPoolObject
{
    [SerializeField] private BotConfigSO _botDuConfig;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private BotNetwork botNetwork;
    [SerializeField] private LayerMask ground;
    [SerializeField] private Transform spwanPos;
    [SerializeField] private GameObject body;
    [SerializeField] private AnimatorOverrideController atorOverride;
    [SerializeField] private Animator ator;
    [Header("Tốc độ rơi khi chưa bung dù,hoặc dù hỏng")]
    [SerializeField]
    protected float dropSpeed = 7;
    [SerializeField]
    protected float dropBot;
    [SerializeField]
    protected float dropDistanceDeath = 7;
    [Header("Tốc độ rơi sau khi bung dù")]
    [SerializeField]
    protected float openParachuteDropSpeed = 2;

    [Header("Độ đung đưa của dù theo trục X")]
    [SerializeField]
    AnimationCurve parachuteRotaX;

    [Header("Độ đung đưa của dù theo trục Z")]
    [SerializeField]
    AnimationCurve parachuteRotaZ;

    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private GameObject botDead;
    private Coroutine C_MoveFirstDistance;
    private Transform myTrans;
    private GameObject botCarry;
    [SerializeField] private RuntimeAnimatorController atorBotCarry;
    private GameObject botCarryInit;
    private Vector3 landPos;
    private bool isOpenParachute;
    private float _countSwingTime;
    public float DistanceStopSwing = 1;
    [SerializeField] private bool isFallingAfterDeath = false;
    private AnimationCurve originalParachuteRotaX;
    private void Awake()
    {
        myTrans = transform;
        originalParachuteRotaX = parachuteRotaX;
    }

    private void OnEnable()
    {
        ResetState();
        RaycastHit dropPosHit;
        if (Physics.Raycast(myTrans.position + Vector3.forward * 3, Vector3.down, out dropPosHit, 300, ground))
        {
            landPos = dropPosHit.transform.position;
            landPos = new Vector3(landPos.x, 0, landPos.z);
        }

        InitParachute();
        //InitCarry();
        //ResetAnimation(); 
        botNetwork.OnBotDead += OnDead;
        C_MoveFirstDistance = StartCoroutine(MoveFirstDistance());
    }

    private void OnDisable()
    {
        botNetwork.OnBotDead -= OnDead;
    }

    protected void InitParachute()
    {
        _spriteRenderer.enabled = true;
        botDead.SetActive(false);
        isOpenParachute = false;
        body.SetActive(false);
        botCarry = _botDuConfig.carryAttributes[0].botConfig.Model;
    }


    protected void InitCarry()
    {
        botCarryInit = ObjectPool.Instance.PopFromPool(botCarry, instantiateIfNone: true);
        botCarryInit.transform.SetPositionAndRotation(spwanPos.position, spwanPos.rotation);
        botCarryInit.transform.SetParent(spwanPos);
        botCarryInit.GetComponent<BotNetwork>().Reset();
        //botCarryInit = Instantiate(botCarry, spwanPos);
        _spriteRenderer.enabled = false;
        atorBotCarry = botCarryInit.GetComponentInChildren<Animator>().runtimeAnimatorController;
        botCarryInit.GetComponentInChildren<Animator>().runtimeAnimatorController = atorOverride;
        botCarryInit.GetComponent<BotNetwork>().SetPath(PathManager.Instance.GetWayPoint(_botDuConfig.carryAttributes[0].botConfig.botType));
    }
    private void Update()
    {
        if (isOpenParachute)
        {
            _countSwingTime += Time.deltaTime;
            if (myTrans.position.y - landPos.y > DistanceStopSwing)
            {
                transform.localRotation = Quaternion.Euler(
                    parachuteRotaX.Evaluate(_countSwingTime),
                    transform.localEulerAngles.y,
                    parachuteRotaZ.Evaluate(_countSwingTime)
                );
            }
        }

        if (isFallingAfterDeath)
        {
            // Rơi xuống theo dropBot
            myTrans.Translate(dropBot * Time.deltaTime * Vector3.down);

            // Khi chạm đất
            if (myTrans.position.y - landPos.y <= DistanceStopSwing)
            {
                isFallingAfterDeath = false;
                _spriteRenderer.enabled = false;
                //Debug.Log("Bot đã chạm đất sau khi chết.");
                StartCoroutine(ResetAfterDeath());
            }
        }
    }

    void ResetState()
    {
        _countSwingTime = 0;
        isOpenParachute = false;
    }

    void ResetAnimation()
    {
        botCarryInit.GetComponentInChildren<Animator>().runtimeAnimatorController = atorOverride;
    }


    public Vector2 FirstDistanceFallMinMax = new Vector2(10, 10);
    public Vector2 HitchForceMinMax = new Vector2(1.25f, 1.7f);

    IEnumerator MoveFirstDistance()
    {
        yield return new WaitForEndOfFrame(); // Đảm bảo đã thiết lập xong
        var startY = myTrans.position.y;
        float firstDistance = Random.Range(FirstDistanceFallMinMax.x, FirstDistanceFallMinMax.y);

        Vector3 firstDes = new Vector3(myTrans.position.x, startY - firstDistance, myTrans.position.z);

        //Debug.Log($"Bắt đầu rơi từ: {startY}, cần rơi {firstDistance}, mục tiêu: {firstDes.y}");

        while (Mathf.Abs(myTrans.position.y - firstDes.y) > 0.1f) // So sánh chính xác hơn
        {
            myTrans.position = Vector3.MoveTowards(myTrans.position, firstDes, dropSpeed * Time.deltaTime);
            //Debug.Log($"Hiện tại: {myTrans.position.y}, mục tiêu: {firstDes.y}, đã rơi: {startY - myTrans.position.y}");
            yield return null;
        }
        body.SetActive(true);
        float hitchForce = Random.Range(HitchForceMinMax.x, HitchForceMinMax.y);
        Vector3 forceDes = myTrans.position + (Vector3.up * hitchForce);
        while (myTrans.position.y < forceDes.y)
        {
            myTrans.Translate(dropSpeed * Time.deltaTime * Vector3.up);
            yield return null;
        }
        isOpenParachute = true;
        //dropSpeed = openParachuteDropSpeed;
        while (myTrans.position.y - landPos.y > DistanceStopSwing)
        {
            myTrans.Translate(openParachuteDropSpeed * Time.deltaTime * Vector3.down);
            yield return null;
        }
        ator.Play("DongDu");

        InitCarry();
        botCarryInit.GetComponentInChildren<Animator>().runtimeAnimatorController = atorBotCarry;
        botCarryInit.transform.SetParent(null);
        BotLandingManager.Instance.IncrementLandCount();
        //ObjectPool.Instance.PushToPool(this,gameObject);
    }

    private Vector3 botDeadPos;
    void OnDead()
    {
        isOpenParachute = false;
        botDeadPos = this.transform.position;
        //BotDeathHandler.Instance.OnBotDeath(botDeadPos);
        StopCoroutine(C_MoveFirstDistance);
        //Debug.Log($"Bắt đầu HandleBotDie(), dropBot = {dropBot}");
        myTrans.rotation = Quaternion.Euler(0, -180f, 0);
        isFallingAfterDeath = true;
        //StartCoroutine(HandleBotDie());
        float distanceToLand = myTrans.position.y - landPos.y;
        ator.Play("DongDu");
        if (distanceToLand > dropDistanceDeath)
        {
            var damageInfo = new DamageInfo()
            {
                damageType = DamageType.Normal,
                damage = 69,
                //name = hit.collider.name,
            };
            _spriteRenderer.enabled = false;
            botDead.SetActive(true);

            //Debug.Log(transform.rotation);
            audioSource.PlayOneShot(AudioManager.Instance.GetAudioHitClip());
            _spriteRenderer.GetComponentInChildren<ITakeDamage>()?.TakeDamage(damageInfo);
            BotDeath.Instance.GetBotDeath();
            // Bot Die
        }
        else if (distanceToLand <= dropDistanceDeath)
        {
            _spriteRenderer.enabled = false;
            InitCarry();
            //botCarryInit.GetComponentInChildren<Animator>().runtimeAnimatorController = atorBotCarry;
            //botCarryInit.transform.SetParent(null);
        }
        //botCarryInit.GetComponentInChildren<Animator>().runtimeAnimatorController = atorBotCarry;

    }

    IEnumerator HandleBotDie()
    {
        Debug.Log($"Bắt đầu HandleBotDie(), dropBot = {dropBot}");
        while (myTrans.position.y - landPos.y > DistanceStopSwing)
        {
            myTrans.Translate(dropBot * Time.deltaTime * Vector3.down);
            Debug.Log($"Đang rơi sau khi chết, vị trí: {myTrans.position.y}, tốc độ: {dropBot}");
            yield return null;
        }
        Debug.Log("Bot đã chạm đất sau khi chết.");
        if (botCarryInit && !botCarryInit.GetComponent<BotNetwork>().IsDead)
        {
            botCarryInit.GetComponentInChildren<Animator>().runtimeAnimatorController = atorBotCarry;
            botCarryInit.transform.SetParent(null);
        }

        yield return new WaitForSeconds(2f); // Đợi animation hoàn thành
        ResetAnimation();
        ObjectPool.Instance.PushToPool(this, gameObject);
    }

    private IEnumerator ResetAfterDeath()
    {
        if (botCarryInit && !botCarryInit.GetComponent<BotNetwork>().IsDead)
        {
            botCarryInit.GetComponentInChildren<Animator>().runtimeAnimatorController = atorBotCarry;
            botCarryInit.transform.SetParent(null);
        }

        yield return new WaitForSeconds(2f); // Chờ animation hoàn thành
        ObjectPool.Instance.PushToPool(this, gameObject);
    }


    public GameObject Prefab { get; set; }
    public void Init()
    {

    }

    public void OnPushToPool()
    {
        parachuteRotaX = originalParachuteRotaX;
    }

}