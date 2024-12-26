//using System;
//using System.Collections;
//using UnityEngine;
//using UnityEngine.AI;
//using UnityEngine.Events;
//using Random = UnityEngine.Random;

//public class Aircraft1CParachute : Parachute
//{
//    [Header("INPUT")]
//    /// <summary>
//    /// là lính
//    /// </summary>
//    public BotNetwork botNet;

//    /// <summary>
//    /// là dù
//    /// </summary>
//    [SerializeField] protected BotNetwork myBotNet;
//    [SerializeField] protected Transform parachuteBody;
//    [SerializeField] protected Transform parachuteScale;
//    [SerializeField] Collider _colli;
//    [SerializeField] GameObject _body;

//    public Vector3 landPos;
//    static readonly int parachuteHp = 200;

//    [Header("Độ đung đưa của dù theo trục X")] [SerializeField]
//    AnimationCurve parachuteRotaX;

//    [Header("Độ đung đưa của dù theo trục Z")] [SerializeField]
//    AnimationCurve parachuteRotaZ;

//    [Header("Tốc độ rơi của dù theo time")] [SerializeField]
//    protected AnimationCurve speedOnLifeTime;

//    [Header("Thời gian và tốc độ bung dù")] [SerializeField]
//    protected AnimationCurve parachuteSize;

//    [Header("Tốc độ rơi khi chưa bung dù,hoặc dù hỏng")] [SerializeField]
//    protected float dropSpeed = 20;

//    [Header("Độ cao nguy hiểm,Cao hơn thì rơi xuống bot sẽ chết")] [SerializeField]
//    float dangerHight = 50;

//    [Header("Thời gian dù co lại thu dù lại khi dù hỏng")] [SerializeField]
//    protected float shrinkParachute = 1.5f;

//    protected Transform myTrans;
//    protected float countTime;
//    protected float _countSwingTime;
//    protected Vector3 dropDir;
//    protected bool isFallDead;
//    RaycastHit dropPosHit;
//    protected bool landDone;
//    protected float parachuteBrokenSpeed = 0;

//    [HideInInspector] public UnityAction dropDoneAc;
//    public Action FallingAction;
//    public Action FallingDeadWhenHitParachute;
//    protected Transform target;
//    private bool _callDeadOnThisTurn = false;
//    float _flySpeed;
//    bool _isSwing;
//    bool _straightFall;
//    /// <summary>
//    /// hệ số trừ tốc độ khi lính được thả ở gần ng chơi => để tránh bị cảm giác lính ở gần thì rơi nhanh hơn ở xa
//    /// </summary>
//    public float SpdCoefficient = -0.1f;
//    public float DistanceStopSwing = 5;

//    #region UNITY CORE >>>>>>>>>>>>>>>>>>>>>>

//    protected override void OnEnable()
//    {
//        if (_colli) _colli.enabled = false;
//        _isHitch = false;
//        _callDeadOnThisTurn = false;
//        _isSwing = false;
//        isFallDead = false;
//        landDone = false;
//        countTime = 0;
//        _countSwingTime = 0;
//        myTrans = transform;
//        parachuteBrokenSpeed = 0;
//        dropDir = new Vector3(Random.value, -1, Random.value);
//        parachuteScale.localScale = Vector3.zero;
//        if (parachuteStats != null) botState.enemyStats = parachuteStats;
//        myBotNet.Id = BotManager.Instance.GetUniqueID();
//        if (myBotNet.Dead == true)
//        {
//            myBotNet.Dead = false;
//            myBotNet.Health = myBotNet.state.enemyStats.health;
//        }
//        if (_body) _body.gameObject.SetActive(true);
//        botState.enabled = true;
//        myBotNet.enabled = true;
//        BotManager.Instance.Bots.Add(myBotNet);
//        target = PlayerManager.GetLocalPlayerTrans();

//        StartCoroutine(DelayInit());
//    }

//    protected virtual IEnumerator DelayInit()
//    {
//        yield return new WaitForSeconds(0.1f);

//        myBotNet.IsDead.RemoveListener(OnMyBotNetDead);
//        myBotNet.IsDead.AddListener(OnMyBotNetDead);
//    }

//    [Header("Giật khi bung dù")]
//    public Vector2 FirstSpeedMinMax = new Vector2(10, 10);
//    public Vector2 FirstDistanceFallMinMax = new Vector2(10, 10);
//    bool _isHitch;
//    public Vector2 HitchForceMinMax = new Vector2(1.25f, 1.7f);
//    IEnumerator IEScale()
//    {
//        float firstSpd = Random.Range(FirstSpeedMinMax.x, FirstSpeedMinMax.y);
//        float firstDistance = Random.Range(FirstDistanceFallMinMax.x, FirstDistanceFallMinMax.y);
//        Vector3 firstDes = myTrans.position + Vector3.down * firstDistance;
//        if (firstDes.y <= landPos.y) firstDes.y = landPos.y - 1;
//        yield return null;
//        while (myTrans.position.y > firstDes.y) 
//        {
//            myTrans.Translate(firstSpd * Time.deltaTime * Vector3.down);
//            yield return null;
//        }
//        float hitchForce = Random.Range(HitchForceMinMax.x, HitchForceMinMax.y);
//        float countTimeScale = 0;
//        float total = parachuteSize.keys[parachuteSize.length - 1].time;
//        float hitchTime = Random.Range(0.75f * total, 0.85f * total);
//        while (countTimeScale < total)
//        {
//            if (!_isHitch && countTimeScale > hitchTime)
//            {
//                _isHitch = true;
//                LeanTween.move(this.gameObject, myTrans.position + (Vector3.up * hitchForce), 1).setEaseOutQuad()
//                    .setOnComplete(() =>
//                    {
//                        if (FallingAction == null) FallingAction += OnFalling;
//                        _isHitch = false;
//                        _isSwing = true;
//                        if (_colli) _colli.enabled = true;
//                    });
//            }

//            countTimeScale += Time.deltaTime;
//            float scaleValue = parachuteSize.Evaluate(countTimeScale);
//            parachuteScale.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
//            if (!_isHitch) myTrans.Translate(firstSpd * Time.deltaTime * Vector3.down);
//            yield return null;
//        }
//    }


//    protected virtual void OnDisable()
//    {
//        if (myBotNet != null)
//        {
//            myBotNet.IsDead.RemoveListener(OnMyBotNetDead);
//        }

//        if (botNet != null)
//        {
//            botNet.IsDead.RemoveListener(OnBotNetDead);
//        }

//        FallingAction = null;
//        KillBotOnEnemyPos();
//    }

//    #endregion

//    #region MAIN >>>>>>>>>>>>>>>>>>>>

//    protected virtual void OnDead(bool isDead)
//    {
//        if (isDead)
//        {
//            _isSwing = false;
//            LeanTween.cancel(gameObject);
//            if (FallingAction == null) FallingAction += OnFalling;
//            StartCoroutine(SetTimeKillBot());

//            LeanTween.scale(parachuteScale.gameObject, new Vector3(0.1f, 0.1f, 1f), shrinkParachute);
//            dropDir = Vector3.down;
//            if (Physics.Raycast(myTrans.position + Vector3.up, Vector3.down, out dropPosHit, 200,
//                    Layers.ExtendDefaultMask))
//            {
//#if UNITY_EDITOR
//                Debugger.DrawLine(dropPosHit.point, dropPosHit.point + Vector3.up, Color.red, 10f, DebugCategories.BotDamage);
//#endif
//                if (NavMesh.SamplePosition(dropPosHit.point, out var navMeshHit, 5, NavMesh.AllAreas))
//                {
//                    landPos = navMeshHit.position;
//                    isFallDead = Vector3.Distance(myTrans.position, navMeshHit.position) > dangerHight;

//                    if (dropPosHit.transform.root != SectionController.ActiveSection.transform && !isFallDead)
//                        isFallDead = true;
//                }
//                else
//                {
//                    landPos = dropPosHit.point;
//                    isFallDead = true;
//                }
//            }
//            else
//            {
//                SetLandPos();
//            }
//        }
//        else
//        {
//            SetLandPos();
//        }

//        if (isFallDead && myBotNet.Dead)
//        {
//            FallingDeadWhenHitParachute?.Invoke();
//        }
//    }

//    /// <summary>
//    /// Lính chết
//    /// </summary>
//    protected virtual void OnBotNetDead(bool isDead)
//    {
//        if (!this.gameObject.scene.isLoaded)
//        {
//            return;
//        }
//        StopAllCoroutines();
//        if (botNet) botNet.IsDead.RemoveListener(OnBotNetDead);
//        if (_callDeadOnThisTurn == false)
//        {
//            _callDeadOnThisTurn = true;
//            OnDead(isDead);
//        }
//    }

//    /// <summary>
//    /// Dù chết
//    /// </summary>
//    protected virtual void OnMyBotNetDead(bool isDead)
//    {
//        if (!this.gameObject.scene.isLoaded)
//        {
//            return;
//        }
//        StopAllCoroutines();
//        if (myBotNet) myBotNet.IsDead.RemoveListener(OnMyBotNetDead);
//        if (_callDeadOnThisTurn == false)
//        {
//            _callDeadOnThisTurn = true;
//            if (botNet) OnDead(isDead);
//        }
//    }

//    protected virtual IEnumerator SetTimeKillBot()
//    {
//        yield return new WaitForSeconds(6.7f);
//        if (botNet != null && isFallDead)
//        {
//            botNet.ForceToDie();
//            botNet.transform.position = new Vector3(0, -1000, 0);
//            botNet.transform.SetParent(null);
//        }
//    }

//    protected virtual void SetLandPos()
//    {
//        if (myTrans != null && target != null)
//        {
//            landPos = myTrans.position;
//            landPos.y = target.position.y - 30;
//            isFallDead = true;
//        }
//    }

//    void OnFalling()
//    {
//        countTime += Time.deltaTime;
//        if (myBotNet.Dead || _callDeadOnThisTurn)
//        {
//            if (parachuteBrokenSpeed == 0)
//            {
//                parachuteBrokenSpeed = speedOnLifeTime.Evaluate(countTime);
//            }

//            parachuteBrokenSpeed += Time.deltaTime * 15;
//            myTrans.Translate(dropDir * (Mathf.Min(parachuteBrokenSpeed, dropSpeed * 3) * Time.deltaTime));
//        }
//        else
//        {
//            if (!_isHitch)
//            {
//                myTrans.position = Vector3.MoveTowards(myTrans.position, landPos,
//                    Time.deltaTime * speedOnLifeTime.Evaluate(countTime));
//                if (target != null)
//                {
//                    Vector3 lootAtPos = new Vector3(target.position.x, myTrans.position.y, target.position.z);
//                    myTrans.LookAt(lootAtPos);
//                }
//            }
//        }

//        if (myTrans.position.y <= landPos.y + 0.1f && !landDone)
//        {
//            landDone = true;

//            if (isFallDead)
//            {
//                //Nếu Ngã Chết
//                LeanTween.scale(parachuteScale.gameObject, Vector3.zero, shrinkParachute);
//                dropDoneAc?.Invoke();
//                LeanTween.rotateX(botNet.gameObject, 0, 1f);
//                if (dropPosHit.transform != null)
//                {
//                    if (Vector3.Distance(myTrans.position, landPos) > 0.5f)
//                    {
//                        LeanTween.move(botNet.gameObject, landPos, 0.5f);
//                    }
//                }
//            }
//            else
//            {
//                //Nếu Ngã Ứ Chết
//                dropDoneAc?.Invoke();
//            }

//            KillParachuteAfterDone();
//        }
//    }

//    protected virtual void Update()
//    {
//        FallingAction?.Invoke();
//        if (_isSwing)
//        {
//            _countSwingTime += Time.deltaTime;
//            if (myTrans.position.y - landPos.y > DistanceStopSwing)
//            {
//                //Lắc dù
//                parachuteBody.localRotation = Quaternion.Euler(parachuteRotaX.Evaluate(_countSwingTime),
//                    parachuteBody.localEulerAngles.y, parachuteRotaZ.Evaluate(_countSwingTime));
//            }
//            else
//            {
//                parachuteBody.localRotation = Quaternion.Slerp(parachuteBody.localRotation, Quaternion.identity, 0.01f);
//            }
//        }
//    }

//    /// <summary>
//    /// Giết dù sau khi dùng xong
//    /// </summary>
//    protected virtual void KillParachuteAfterDone()
//    {
//        if (!myBotNet.Dead)
//        {
//            if (_body) _body.SetActive(false);
//            //myBotNet.ForceToDie();
//            if (botNet)
//            {
//                botNet.IsDead.RemoveListener(OnBotNetDead);
//                botNet = null;
//            }
//        }
//    }

//    /// <summary>
//    /// fix bug kẹt bot trong dù làm k qua được màn
//    /// </summary>
//    protected virtual void KillBotOnEnemyPos()
//    {
//        if (!this.gameObject.scene.isLoaded)
//        {
//            return;
//        }

//        if (botNet != null && !botNet.Dead && botNet.transform.parent != null && botParent.childCount > 0)
//        {
//            botNet.ForceToDie();
//            botNet.transform.parent = null;
//        }
//    }

//    #endregion

//    #region SUPPORT >>>>>>>>>>>>>>>>>>>
   
//    public void StartFalling(int idx)
//    {
//        _flySpeed = myBotNet.state.enemyStats.moveSpeed;
//        if (_flySpeed < 2) _flySpeed = 2;
//        BattleDelegate.BotSpawn(myBotNet);
//        StartCoroutine(IEScale());
//        Vector3 lootAtPos = new Vector3(target.position.x, myTrans.position.y, target.position.z);
//        myTrans.LookAt(lootAtPos);
//        InitSpeed();

//        void InitSpeed()
//        {
//            dropSpeed = Mathf.Max(_flySpeed * Random.Range(2f, 2.5f), 15);
//            dangerHight = myBotNet.state.enemyStats.dangerHight;
//            float spdScale = _flySpeed + SpdCoefficient * _flySpeed * idx;
//            _flySpeed = Mathf.Max(_flySpeed * 0.5f, spdScale);
//            speedOnLifeTime.keys = new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, _flySpeed), new Keyframe(2, 0.8f * _flySpeed) };
//        }
//    }

//    public virtual void SetBotNet(ParachuteInfo parachuteInfo)
//    {
//        botNet = parachuteInfo.SoliderBot;
//        if (parachuteStats != null)
//        {
//            this.parachuteStats = parachuteInfo.ParachuteStats;
//        }
//        else
//        {
//            this.parachuteStats = new EnemyStats() { Id = 9999999, health = parachuteHp };
//        }

//        botState.enemyStats = this.parachuteStats;
//        myBotNet.state.enemyStats = parachuteInfo.ParachuteStats;
//        myBotNet.Health = botState.enemyStats.health;
//        this.landPos = parachuteInfo.LandPos;
//        OnSetLandPos(landPos);
//        if (botNet != null)
//        {
//            botNet.IsDead.RemoveListener(OnBotNetDead);
//            botNet.IsDead.AddListener(OnBotNetDead);
//        }
//        _straightFall = parachuteInfo.Straight;
//        this.enabled = true;
//    }

//    protected virtual void OnSetLandPos(Vector3 landPos)
//    {
//    }

//    #endregion
//}