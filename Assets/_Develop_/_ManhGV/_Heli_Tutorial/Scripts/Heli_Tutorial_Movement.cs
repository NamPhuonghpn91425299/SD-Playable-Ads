using System;
using System.Collections;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;
using UnityEngine.Events;

public class Heli_Tutorial_Movement : VehicleNetwork
{
    #region ==================== PROPERTIES ====================

    [Header("Cho cánh quạt máy bay vào đây!")] 
    [SerializeField] Transform[] rotorBlades;
    [SerializeField] private float[] rotateSpeed;
    
    [Header("Connect Script")] [Header("INPUT (Heli_Tutorial_Movement.cs)")] [SerializeField]
    private HeliSFloatBody floatBody;

    [SerializeField] private Transform bodyPos;
    [Space] [SerializeField] private Transform targetPos;
    
    private Vector3 startPos;
    private Vector3 attackDestination;
    [SerializeField] private Vector3 _diveDirOnDead;
    [SerializeField] private LayerMask _groundMask;

    [Header("Move Setting")] [SerializeField]
    private float maxMoveSpeed = 30;

    [SerializeField] private float currentMoveSpeed = 30;
    [Space] [SerializeField] private float maxRotaSpeed = 30;
    [SerializeField] private float currentRotaSpeed = 30;
    [Space] [SerializeField] private float currentDistance = 0;
    [SerializeField] private float onStopDistance = 10;
    [Space] [SerializeField] private float currentAngle = 0;
    [SerializeField] private float maxRotaAngle = 35;

    [Space] [Header("Horizontal Distance Limit")] [SerializeField]
    private float maxHorizontalDistance = 3f;

    [SerializeField] private float horizontalCorrectionMultiplier = 1.5f;
    
    private float rateOnStop;
    private float rateOnAngle;

    [Header("State")] [SerializeField] private EStateType eStateType;
    [Space] [SerializeField] private bool isNormalMoving = false;
    [SerializeField] private bool isRight = false;
    [SerializeField] private bool isAttacking = false;
    [SerializeField] private bool isTilting = false;

    [Header("CAnim")] [SerializeField] private AnimationCurve onStartTiltCAnim;
    [SerializeField] private AnimationCurve onStopTiltCAnim;
    [SerializeField] private AnimationCurve OnTiltXCAnim;

    [Header("Gun")] [SerializeField] private Transform[] listMuzzlePos;
    [SerializeField] private float timerAttack;
    [SerializeField] private float delayMoveAttack = 9;
    
    [Header("Debug Settings")] 
    [SerializeField] protected bool enableDebugLogs = false; // Tích để bật debug logs
    
    [Header("Sliding Shake Settings")]
    [Range(0f, 5f)]
    [SerializeField] protected float shakeIntensity = 1.5f; // Cường độ rung
    [Range(1f, 20f)]
    [SerializeField] protected float shakeFrequency = 8f; // Tần số rung (rung/giây)
    [Range(0.1f, 2f)]
    [SerializeField] protected float shakeDecayRate = 0.8f; // Tốc độ giảm rung theo thời gian

    private UnityAction UActionMovement;
    
    // Handle TiltOnAttack
    private float counterStart = 0;
    private float counterStop = 0;
    private float tiltLeftRightCounter = 0;

    float MoveXValue = 0;
    float tiltTime = 2.5f;

    float moveYValue = 0;
    float campValue = 0.15f;

    private Vector3 tiltValue;
    
    // Shake variables
    private Vector3 originalBodyPosition;
    private float shakeTimer = 0f;
    private bool isShaking = false;
    private float currentShakeIntensity = 0f;

    #endregion ========================================

    #region ==================== UNITY CORE ====================

    protected override void Update()
    {
        base.Update();
        UActionMovement?.Invoke();
        RotateRotorBlades();
    }

    private void OnDisable()
    {
        UActionMovement = null;
        isNormalMoving = false;
        floatBody.enabled = false;
    }

    #endregion ========================================

    #region ==================== INIT ====================

    protected override void Awake()
    {
        base.Awake();
        explosionManager.InitializeExplosions();
    }
    
    public override void OnInit()
    {
        base.OnInit();
        explosionManager.ResetAllExplosions();
        PointGroup pointGroup = botIdentity.AssignedPath;
        if (pointGroup == null || pointGroup.points.Count <= 0 || pointGroup.attackPoints.Count <= 0) 
        {
            OnDead();
            return;
        }
        startPos = pointGroup.points[0].position;
        attackDestination = pointGroup.points[1].position;
        fallTargetTransform = pointGroup.attackPoints[0];
        slideTargetTransform = pointGroup.attackPoints[1];
        
        _bodyFake.SetActive(false);
        _bodyRew.SetActive(true);
        OnInitAnimationCurves();
        floatBody.enabled = true;
        isNormalMoving = false;
        isAttacking = false;
        isTilting = false;
        isRight = true;

        bodyPos.localRotation = Quaternion.Euler(Vector3.zero);
        bodyPos.localPosition = Vector3.zero;
        targetPos = PlayerInstant.Instance.TF;
        TF.localEulerAngles = new Vector3(0, 90, 0);
        TF.position = new Vector3(startPos.x, startPos.y, startPos.z); // + targetPos.position.z);//
        attackDestination =
            new Vector3(attackDestination.x, attackDestination.y, attackDestination.z); // + targetPos.position.z);
        Set_State(EStateType.OnStartPos);
    }

    /// <summary>
    /// Initializes the AnimationCurves (onStartTiltCAnim, onStopTiltCAnim, OnTiltXCAnim)
    /// with predefined keyframes, replicating the setup from the Inspector.
    /// This allows for programmatic control over the curves.
    /// </summary>
    private void OnInitAnimationCurves()
    {
        // onStartTiltCAnim = new AnimationCurve();
        // Keyframe k1_start = new Keyframe(0.0f, 0.0f) { inTangent = 0.26f, outTangent = 0.26f };
        // Keyframe k2_start = new Keyframe(1.5f, 0.4f) { inTangent = 0.23f, outTangent = 0.23f };
        // Keyframe k3_start = new Keyframe(2.5f, 1.0f) { inTangent = 0.62f, outTangent = 0.62f };
        // onStartTiltCAnim.AddKey(k1_start);
        // onStartTiltCAnim.AddKey(k2_start);
        // onStartTiltCAnim.AddKey(k3_start);
        //
        // onStopTiltCAnim = new AnimationCurve();
        // Keyframe k1_stop = new Keyframe(0.09f, 0.1f) { inTangent = -1.1f, outTangent = -1.1f };
        // Keyframe k2_stop = new Keyframe(1.55f, -0.61f) { inTangent = -0.06f, outTangent = -0.06f };
        // Keyframe k3_stop = new Keyframe(2.1f, -0.006f) { inTangent = 0.7f, outTangent = 0.7f };
        // onStopTiltCAnim.AddKey(k1_stop);
        // onStopTiltCAnim.AddKey(k2_stop);
        // onStopTiltCAnim.AddKey(k3_stop);
        //
        // OnTiltXCAnim = new AnimationCurve();
        // Keyframe k1_tiltX = new Keyframe(0.0f, 0.0f) { inTangent = 2.78f, outTangent = 0.27f };
        // Keyframe k2_tiltX = new Keyframe(1.5f, 0.3f) { inTangent = 0.0005f, outTangent = 0.0005f };
        // Keyframe k3_tiltX = new Keyframe(4.5f, -0.3f) { inTangent = -0.0005f, outTangent = -0.0005f };
        // Keyframe k4_tiltX = new Keyframe(6.0f, 0.0f) { inTangent = 0.23f, outTangent = 2.88f };
        // OnTiltXCAnim.AddKey(k1_tiltX);
        // OnTiltXCAnim.AddKey(k2_tiltX);
        // OnTiltXCAnim.AddKey(k3_tiltX);
        // OnTiltXCAnim.AddKey(k4_tiltX);
    }

    
    
    #endregion ========================================

    #region ==================== MAIN ====================

    void Set_State(EStateType inputState = EStateType.None)
    {
        if (inputState != EStateType.None) eStateType = inputState;
        switch (eStateType)
        {
            case EStateType.OnStartPos:
                Start_Movement();
                break;

            case EStateType.OnTiltAttack:
                isRight = !isRight;
                timerAttack = 0;
                tiltLeftRightCounter = 0;
                maxMoveSpeed = 20;
                UActionMovement += OnTiltAttack;
                break;

            case EStateType.OnDead:
                OnDead();
                break;
        }
    }

    #endregion ========================================

    #region ==================== ATTACK ====================

    private void ActiveMuzzle(bool isActive) => listMuzzlePos.ForEach(x => x.gameObject.SetActive(isActive));

    private Transform muzzleRightShootTrans =>
        listMuzzlePos[0] && listMuzzlePos[0].parent.gameObject.activeInHierarchy
            ? listMuzzlePos[0]
            : (listMuzzlePos[2] ? listMuzzlePos[2] : null);

    private Transform muzzleLeftShootTrans => listMuzzlePos[1] && listMuzzlePos[1].parent.gameObject.activeInHierarchy
        ? listMuzzlePos[1]
        : (listMuzzlePos[3] ? listMuzzlePos[3] : null);

    private bool haveMuzzleRight =>
        muzzleRightShootTrans && muzzleRightShootTrans.parent.gameObject.activeInHierarchy;

    private bool haveMuzzleLeft => muzzleLeftShootTrans && muzzleLeftShootTrans.parent.gameObject.activeInHierarchy;

    private void Start_Attack()
    {
        if (isAttacking) return;
        isAttacking = true;

        currentRotaSpeed = 20;

        if (isRight && !haveMuzzleRight || !isRight && !haveMuzzleLeft) isRight = !isRight;

        ActiveMuzzle(false);
        if (haveMuzzleRight)
        {
            if (isRight) muzzleRightShootTrans.LookAt(targetPos);
            muzzleRightShootTrans.gameObject.SetActive(isRight);
        }

        if (haveMuzzleLeft)
        {
            if (!isRight) muzzleLeftShootTrans.LookAt(targetPos);
            muzzleLeftShootTrans.gameObject.SetActive(!isRight);
        }

        if (!haveMuzzleRight && !haveMuzzleLeft) return;
        StartCoroutine(PlayerTakeDamage(isRight));
    }

    private void Stop_Attack()
    {
        if (!isAttacking) return;
        isAttacking = false;

        ActiveMuzzle(false);
    }

    private int i = 0;
    IEnumerator PlayerTakeDamage(bool _isRight)
    {
        i++;
        Vector3 posPlayer = PlayerInstant.Instance.TF.position;
#if UNITY_EDITOR
        if (enableDebugLogs) print("Take Damage");
#endif
        if (!isRight)
        {
            for (int j = 0; j < 3; j++) 
            { 
                SimplePool<GameConstants.ProjecttilePlayer>.Spawn<BulletTrail>(GameConstants.ProjecttilePlayer.Projectile_Bullet_Norman, muzzleLeftShootTrans.position, Quaternion.identity).Init((posPlayer - muzzleLeftShootTrans.position).normalized,posPlayer);
                yield return HelperCoroutine.GetWait(.3f); 
            }
        }
        else if (isRight)
        {
            for (int j = 0; j < 3; j++) 
            { 
                SimplePool<GameConstants.ProjecttilePlayer>.Spawn<BulletTrail>(GameConstants.ProjecttilePlayer.Projectile_Bullet_Norman, muzzleRightShootTrans.position, Quaternion.identity).Init((posPlayer - muzzleRightShootTrans.position).normalized,posPlayer); 
                yield return HelperCoroutine.GetWait(.3f);
            }
        }
        if(GameController.Instance.CurrentGameState == GameConstants.GameState.InGame)
            EventManager.Instance?.Publish(new PlayerHealthChangedEvent(damage: Damage, state:"OnlyDamage"));
        
        if (i==3)
        {
            i = 0;    
            SimplePool<GameConstants.ProjectileEnemy>.Spawn<Rocket>(GameConstants.ProjectileEnemy.Projectile_Bullet_Rocket, listMuzzlePos[4].position, listMuzzlePos[4].rotation).Init(Damage);
            yield return HelperCoroutine.GetWait(.4f);
            SimplePool<GameConstants.ProjectileEnemy>.Spawn<Rocket>(GameConstants.ProjectileEnemy.Projectile_Bullet_Rocket, listMuzzlePos[5].position, listMuzzlePos[5].rotation).Init(Damage);
        }
    }

    #endregion ========================================

    #region ==================== MOVEMENT ====================

    private void Start_Movement()
    {
        if (isNormalMoving) return;
        isNormalMoving = true;
        counterStart = 0;
        counterStop = 0;
        currentMoveSpeed = maxMoveSpeed / 2;
        _currentAcceleration = _maxAccelerationMove;
        _movingPhase = MovingPhase.IncreasingSpd;
        UActionMovement += Change_Speed;
        UActionMovement += OnMovement;
    }

    private void OnMovement()
    {
        counterStart += Time.deltaTime;
        if (currentDistance > onStopDistance)
            HeliExtension.RotaToTarget(TF, attackDestination, currentRotaSpeed);
        // Move
        if (currentDistance > onStopDistance && !isTilting)
        {
            TF.Translate(currentMoveSpeed * Time.deltaTime * Vector3.forward);

            HeliExtension.TiltBodyOnMoveX(bodyPos, 1, 15, Mathf.Min(counterStart * 0.6f, 1), 1.2f, clampRotaAngle: 10);
        }
        else if (currentDistance > 0.5f && isTilting)
            TF.position = Vector3.MoveTowards(TF.position, attackDestination, currentMoveSpeed / 1.5f * Time.deltaTime);
        
        if (rateOnStop < 3)
            Start_TiltOnStop();
    }

    private void Change_Speed()
    {
        Get_CurrentDistance();
        SlerpSpeed();
        Get_CurrentAngle();

        rateOnStop = TiltHelper.OnRange(currentDistance / onStopDistance, 3, .3f);
        // rateOnStop01 = (rateOnStop > 1) ? 1 : rateOnStop;
        rateOnAngle = TiltHelper.OnRange(Mathf.Abs(currentAngle) / maxRotaAngle);
        // rateAngleOnMove = (Mathf.Abs(currentAngle) > 25)
        //     ? TiltHelper.OnRange(1 - rateOnAngle, 1, 0.3f)
        //     : 1;

        //currentMoveSpeed = maxMoveSpeed * rateOnStop01 * rateAngleOnMove;
        currentRotaSpeed = maxRotaSpeed * rateOnAngle;
    }

    MovingPhase _movingPhase;
    [SerializeField] float _maxAccelerationMove;
    public float _currentAcceleration;

    float SlerpSpeed()
    {
        Get_CurrentDistance();
        // lerp speed through time
        switch (_movingPhase)
        {
            case MovingPhase.None:
                break;
            case MovingPhase.IncreasingSpd:
                currentMoveSpeed += _currentAcceleration * Time.deltaTime;
                if (currentMoveSpeed >= maxMoveSpeed)
                {
                    _movingPhase = MovingPhase.OnMaxSpd;
                }
                else if (currentDistance <= onStopDistance)
                {
                    ChangeToDecreaseSpeed();
                }

                break;
            case MovingPhase.OnMaxSpd:
                if (currentDistance <= onStopDistance)
                    ChangeToDecreaseSpeed();
                break;
            case MovingPhase.DecreasingSpd:
                currentMoveSpeed += _currentAcceleration * Time.deltaTime;
                break;
            default:
                break;
        }

        return currentMoveSpeed;

        void ChangeToDecreaseSpeed()
        {
            _currentAcceleration = -currentMoveSpeed * currentMoveSpeed / (2 * currentDistance);
            _movingPhase = MovingPhase.DecreasingSpd;
        }
    }

    void Start_TiltOnStop()
    {
        if (isTilting) return;
        isTilting = true;
        UActionMovement += TiltOnStop;
    }

    private void TiltOnStop()
    {
        counterStop += Time.deltaTime;

        HeliExtension.TiltBodyOnMoveX(bodyPos, 10, 1, Mathf.Min(counterStop * 0.6f, 1), 1.2f, clampRotaAngle: 10);
        TF.RotateLocalSlerp(Quaternion.Euler(TF.eulerAngles.x, 0, TF.eulerAngles.z), counterStop * Time.deltaTime);
        //var rate = TiltHelper.OnRange(counterStop, onStopTiltCAnim);

        //tiltValue = beforeTilt.OnTiltTrans(rate * 10, isAxisX: true);
        //bodyPos.localEulerAngles = 
        //    new Vector3(tiltValue.x, bodyPos.localEulerAngles.y, bodyPos.localEulerAngles.z);

        if (counterStop >= onStopTiltCAnim[onStopTiltCAnim.length - 1].time && currentMoveSpeed <= 0.2f)
        {
            isTilting = false;
            UActionMovement -= TiltOnStop;
            isNormalMoving = false;
            UActionMovement -= Change_Speed;
            UActionMovement -= OnMovement;

            Set_State(EStateType.OnTiltAttack);
        }
    }

    public float rateOnStart()
    {
        counterStart += Time.deltaTime;
        return TiltHelper.OnRange(counterStart, onStartTiltCAnim);
    }

    #endregion ========================================

    #region ==================== TILT ON ATTACK ====================

    private void OnTiltAttack()
    {
        MoveXValue = OnTiltXCAnim.Evaluate(tiltLeftRightCounter);

        timerAttack += Time.deltaTime;
        tiltLeftRightCounter += Time.deltaTime;

        var dirTilt = (isRight ? 1 : 0) + (int)timerAttack / 3;
        if (TF.position.x > attackDestination.x + 4)
        {
            currentMoveSpeed += 10 * ((dirTilt % 2 == 0) ? Time.deltaTime : -Time.deltaTime);
        }
        else if (TF.position.x > attackDestination.x - 4)
        {
            currentMoveSpeed -= 10 * ((dirTilt % 2 == 0) ? Time.deltaTime : -Time.deltaTime);
        }

        var isStopTilt = timerAttack > delayMoveAttack - tiltTime;

        if (timerAttack < tiltTime)
        {
            currentMoveSpeed += Time.deltaTime * 10; // timerAttack / tiltTime ;
        }
        else if (isStopTilt)
        {
            Stop_Attack();
            currentMoveSpeed -= Time.deltaTime * 10; // (delayMoveAttack - timerAttack) / tiltTime;
        }
        else
        {
            Start_Attack();
        }

#if UNITY_EDITOR
        if (enableDebugLogs)
        {
            print("MoveXValue: " + MoveXValue + " | Direction: " +
                  (MoveXValue > 0 ? "PHẢI" : MoveXValue < 0 ? "TRÁI" : "GIỪĂ"));
            print("TF X: " + TF.position.x + " | AttackDest X: " + attackDestination.x + " | Distance: " +
                  Mathf.Abs(TF.position.x - attackDestination.x));
        }
#endif

        currentMoveSpeed = TiltHelper.OnRange(currentMoveSpeed, maxMoveSpeed, 10);
        // currentMoveSpeed = TiltHelper.OnRange(currentMoveSpeed + Time.deltaTime, maxMoveSpeed);

        HeliExtension.RotaToTarget((isRight) ? listMuzzlePos[2] : listMuzzlePos[1], targetPos.position,
            currentRotaSpeed);

        // Áp dụng điều chỉnh khoảng cách ngang
        float adjustedMoveXValue = GetAdjustedMoveXValue();
#if UNITY_EDITOR
        if (enableDebugLogs) print("Adjusted MoveXValue: " + adjustedMoveXValue);
#endif

        TF.Translate(new Vector3(adjustedMoveXValue, GetMoveYValue(), 0) * (currentMoveSpeed * Time.deltaTime));
        TF.localEulerAngles = new Vector3(TF.localEulerAngles.x, TF.localEulerAngles.y, MoveXValue * -35);

        HeliExtension.TiltBodyOnMoveX(bodyPos, 0, 0, 1, 1.2f, 30);
        HeliExtension.TiltBodyOnMoveZ(TF, bodyPos, bodyPos.position + (bodyPos.forward * 20), 0.4f);

        // Next State
        if (timerAttack > delayMoveAttack)
        {
            UActionMovement -= OnTiltAttack;
            Set_State(EStateType.OnTiltAttack);
        }
    }

    float GetAdjustedMoveXValue()
    {
        float currentHorizontalDistance = Mathf.Abs(TF.position.x - attackDestination.x);
        float adjustedValue = MoveXValue;

        // Nếu khoảng cách ngang vượt quá giới hạn
        if (currentHorizontalDistance > maxHorizontalDistance)
        {
            // Tính toán hướng cần điều chỉnh (dương nếu cần đi sang trái, âm nếu cần sang phải)
            float directionToCenter = attackDestination.x - TF.position.x;
            float correctionDirection = Mathf.Sign(directionToCenter);

            // Nếu MoveXValue hiện tại đi ra xa điểm tấn công
            if (Mathf.Sign(MoveXValue) != correctionDirection)
            {
                // Tăng cường movement ngược lại để kéo máy bay về gần điểm tấn công
                adjustedValue = MoveXValue + (correctionDirection * horizontalCorrectionMultiplier * 0.1f);
#if UNITY_EDITOR
                if (enableDebugLogs)
                {
                    print("CORRECTION APPLIED: Distance=" + currentHorizontalDistance + " | Original=" + MoveXValue +
                          " | Adjusted=" + adjustedValue);
                }
#endif
            }
            else
            {
                // Vẫn đi về đúng hướng nhưng giảm nhẹ để không đi quá xa
                adjustedValue = MoveXValue * 0.8f;
            }
        }

        return adjustedValue;
    }

    float GetMoveYValue()
    {
        if (TF.position.y > attackDestination.y + 1.5f && moveYValue > -campValue)
        {
            moveYValue -= Time.deltaTime * 0.3f;
        }
        else if (TF.position.y < attackDestination.y - 1.5f && moveYValue < campValue)
        {
            moveYValue += Time.deltaTime * 0.3f;
        }

        return moveYValue;
    }

    #endregion ========================================

    #region ==================== ON DEAD ====================

    private Vector3 _landPos;

    public override void BotDead()
    {
        if (PointKillCaculatorMeldal < 10)
            AchievementEvaluator.Instance.OnBotKilled(PointKillCaculatorMeldal,false);
        else
        {
            AchievementEvaluator.Instance.ResetKillData();
            AchievementEvaluator.Instance.GrantMedal(4);
        }
        base.BotDead();
        ExplosionAndTakeDamageInRadius();
        BotSpawnManager.Instance.botInScene.Remove(GetTransformCenter());
        if (botIdentity.Type != SpawnableType.None)
            botIdentity.Bot_ReportKill();
        Set_State(EStateType.OnDead);
    }

    private void OnDead()
    {
        step1.SetActive(true);
        floatBody.enabled = false;
        listMuzzlePos[0].gameObject.SetActive(false);
        listMuzzlePos[1].gameObject.SetActive(false);
        maxMoveSpeed = 35;
        maxRotaSpeed = 30;

        UActionMovement = null;
        StartCoroutine(FallOnCenterAndSlideOnTheGround());
    }

    [Header("Dead Setting - New Design")]
    public Transform fallTargetTransform; // Transform 1: Điểm rơi (có rotation sẵn)
    public Transform slideTargetTransform; // Transform 2: Điểm trượt tới
    public float maxSpeedDrop = 10f; // Tốc độ rơi tối đa
    public float initialSpeedDrop = 1f; // Tốc độ rơi ban đầu
    public float maxRotationSpeed = 180f; // Tốc độ xoay tối đa (độ/giây)
    public float initialRotationSpeed = 30f; // Tốc độ xoay ban đầu

    [Header("Dead Effect")]
    public GameObject step1;
    public GameObject _bodyRew;
    public GameObject _bodyFake;

    public ParticleSystem vfxExplosionGround;
    public ParticleSystem vfxSlideGround;
    public ParticleSystem vfxExplosionEnd;
    public ExplosionManager explosionManager;
    
    private float currentSpeedDrop;
    private float currentRotationSpeed;

    IEnumerator FallOnCenterAndSlideOnTheGround()
    {
        // Reset giá trị
        currentSpeedDrop = initialSpeedDrop;
        currentRotationSpeed = initialRotationSpeed;

        while (TF.position.y > fallTargetTransform.position.y)
        {
            // Xoay tự nhiên theo trục Y
            TF.Rotate(0, currentRotationSpeed * Time.deltaTime, 0, Space.Self);
            
            // Di chuyển xuống với tốc độ tăng dần
            TF.position = Vector3.MoveTowards(TF.position, fallTargetTransform.position,
                currentSpeedDrop * Time.deltaTime);
            
            // Tăng tốc độ rơi
            currentSpeedDrop = Mathf.Min(currentSpeedDrop + maxSpeedDrop * Time.deltaTime, maxSpeedDrop);
            
            // Tăng tốc độ xoay
            currentRotationSpeed = Mathf.Min(currentRotationSpeed + maxRotationSpeed * Time.deltaTime, maxRotationSpeed);
            
            yield return null;
        }

        // Đảm bảo vị trí chính xác
        bodyPos.localRotation = Quaternion.Euler(Vector3.zero);
        floatBody.ResetRotation();
        TF.position = fallTargetTransform.position;
        
        // Gọi callback khi đến fallTargetTransform
        OnReachedFallTarget();
        
        // Bắt đầu tính toán và quay trục X về phía player trong vòng 1 giây
        yield return StartCoroutine(RotateXToPlayerIn1Second());
    }

    /// <summary>
    /// Vừa trượt vừa xoay trục X thẳng trong 1 giây, giữ nguyên chiều xoay như khi rơi
    /// </summary>
    IEnumerator RotateXToPlayerIn1Second()
    {
        // Kiểm tra null references
        if (fallTargetTransform == null || slideTargetTransform == null) 
        {
            Debug.LogError("fallTargetTransform hoặc slideTargetTransform bị null!");
            yield break;
        }

        // === SETUP CHO XOAY TRỤC X ===
        // Tính vector hướng từ fallTarget đến slideTarget
        Vector3 directionVector = (slideTargetTransform.position - fallTargetTransform.position).normalized;
        directionVector.y = 0; // Chỉ xét trên mặt phẳng ngang

        // Tính góc mà trục X hiện tại tạo với hướng mục tiêu
        Vector3 currentXAxis = TF.right;
        currentXAxis.y = 0;
        currentXAxis.Normalize();

        // Tính góc cần xoay - LUÔN XOAY THEO CHIỀU DƯƠNG (CÙNG CHIỀU VỚI RƠI)
        float targetAngle = Vector3.SignedAngle(currentXAxis, directionVector, Vector3.up);
        
        // CHUỲEN TẤT CẢ GÓC THÀNH GÓC DƯƠNG ĐỂ XOAY CÙNG CHIỀU VỚI RƠI
        if (targetAngle < 0)
        {
            targetAngle += 360f; // Luôn chuyển góc âm thành góc dương
        }
        
        // Bây giờ targetAngle luôn >= 0, đảm bảo xoay cùng chiều với rơi

        Vector3 startEulerAngles = TF.eulerAngles;
        float startYRotation = startEulerAngles.y;
        float targetYRotation = startYRotation + targetAngle;

        // === SETUP CHO TRƯỢT ===
        Vector3 startPosition = TF.position; // fallTargetTransform.position
        Vector3 endPosition = slideTargetTransform.position;
        float totalSlideDistance = Vector3.Distance(startPosition, endPosition);
        
        // Tốc độ trượt giảm dần
        float slideSpeed = maxSpeedDrop * 0.5f; // Tốc độ ban đầu
        float minSlideSpeed = 0.1f;
        float decelerationRate = 2f;

#if UNITY_EDITOR
        if (enableDebugLogs)
        {
            Debug.Log($"Bắt đầu vừa trượt vừa xoay trục X vừa rung body");
            Debug.Log($"Góc cần xoay: {targetAngle}° (cùng chiều với rơi)");
            Debug.Log($"Khoảng cách trượt: {totalSlideDistance}m");
            Debug.Log($"Shake: Intensity={shakeIntensity}, Frequency={shakeFrequency}Hz, Decay={shakeDecayRate}");
        }
#endif

        // === SETUP SHAKE SYSTEM ===
        originalBodyPosition = bodyPos.localPosition;
        isShaking = true;
        currentShakeIntensity = shakeIntensity;
        shakeTimer = 0f;
        
        // === THỰC HIỆN ĐỒNG THỜI: XOAY + TRƯỢT + RUNG ===
        float rotationDuration = 1f; // XOAY TRỤC X THẲNG TRONG 1 GIÂY
        float elapsedTime = 0f;
        
        while (elapsedTime < rotationDuration || Vector3.Distance(TF.position, endPosition) > 0.1f)
        {
            elapsedTime += Time.deltaTime;
            
            // === PHẦN XOAY TRỤC X (1 GIÂY) ===
            if (elapsedTime < rotationDuration)
            {
                float t = elapsedTime / rotationDuration;
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                
                // Xoay trục X thẳng đến đúng vị trí trong 1 giây
                float currentYRotation = Mathf.LerpAngle(startYRotation, targetYRotation, smoothT);
                TF.eulerAngles = new Vector3(startEulerAngles.x, currentYRotation, startEulerAngles.z);
            }
            
            // === PHẦN TRƯỢT (TỐC ĐỘ GIẢM DẦN) ===
            float currentDistance = Vector3.Distance(TF.position, endPosition);
            if (currentDistance > 0.1f)
            {
                // Giảm tốc độ dần
                slideSpeed = Mathf.Max(slideSpeed - decelerationRate * Time.deltaTime, minSlideSpeed);
                
                // Di chuyển về phía slideTargetTransform
                TF.position = Vector3.MoveTowards(TF.position, endPosition, slideSpeed * Time.deltaTime);
            }
            
            // === PHẦN RUNG BODY (LIÊN TỤC KHI TRƯỢT) ===
            if (isShaking)
            {
                shakeTimer += Time.deltaTime;
                
                // Giảm cường độ rung theo thời gian
                currentShakeIntensity = shakeIntensity * Mathf.Exp(-shakeDecayRate * shakeTimer);
                
                // Tính toán shake offset
                float shakeX = Mathf.Sin(shakeTimer * shakeFrequency * 2f * Mathf.PI) * currentShakeIntensity * 0.1f;
                float shakeY = Mathf.Sin(shakeTimer * shakeFrequency * 1.7f * Mathf.PI) * currentShakeIntensity * 0.05f;
                float shakeZ = Mathf.Sin(shakeTimer * shakeFrequency * 2.3f * Mathf.PI) * currentShakeIntensity * 0.08f;
                
                Vector3 shakeOffset = new Vector3(shakeX, shakeY, shakeZ);
                bodyPos.localPosition = originalBodyPosition + shakeOffset;
                
                // Dừng rung khi cường độ quá nhỏ hoặc hết thời gian
                if (currentShakeIntensity < 0.01f)
                {
                    isShaking = false;
                    bodyPos.localPosition = originalBodyPosition; // Reset về vị trí gốc
                }
            }
            
            yield return null;
        }

        // === ĐẢM BẢO KẾT QUẢ CUỐI CÙNG CHÍNH XÁC ===
        TF.position = endPosition;
        TF.eulerAngles = new Vector3(startEulerAngles.x, targetYRotation, startEulerAngles.z);
        
        // Dừng shake system
        isShaking = false;
        bodyPos.localPosition = originalBodyPosition;
        
        // Verify kết quả
        Vector3 finalXAxis = TF.right;
        finalXAxis.y = 0;
        finalXAxis.Normalize();
        float finalAngleError = Vector3.Angle(finalXAxis, directionVector);
        
#if UNITY_EDITOR
        if (enableDebugLogs) Debug.Log($"Hoàn thành vừa trượt vừa xoay. Sai số góc: {finalAngleError:F2}°");
#endif
        
        // Gọi callback khi đến slideTargetTransform
        OnReachedSlideTarget();
        
        // Tiếp tục với sliding
        yield return StartCoroutine(SlideToTargetPosition());
    }

    /// <summary>
    /// Di chuyển từ fallTarget đến slideTarget với tốc độ giảm dần
    /// </summary>
    IEnumerator SlideToTargetPosition()
    {
        if (slideTargetTransform == null) yield break;

        Vector3 startPosition = TF.position;
        Vector3 endPosition = slideTargetTransform.position;
        float slideDistance = Vector3.Distance(startPosition, endPosition);
        
        if (slideDistance < 0.1f) yield break; // Đã ở gần target

        float slideSpeed = maxSpeedDrop * 0.5f; // Tốc độ ban đầu
        float minSlideSpeed = 0.1f;
        float decelerationRate = 2f;

#if UNITY_EDITOR
        if (enableDebugLogs) 
            Debug.Log($"Bắt đầu trượt từ {startPosition} đến {endPosition}, khoảng cách: {slideDistance}m");
#endif

        while (Vector3.Distance(TF.position, endPosition) > 0.1f)
        {
            // Giảm tốc độ dần
            slideSpeed = Mathf.Max(slideSpeed - decelerationRate * Time.deltaTime, minSlideSpeed);
            
            // Di chuyển về phía target
            TF.position = Vector3.MoveTowards(TF.position, endPosition, slideSpeed * Time.deltaTime);
            
            yield return null;
        }

        // Đảm bảo vị trí chính xác
        TF.position = endPosition;
#if UNITY_EDITOR
        if (enableDebugLogs) 
            Debug.Log("Hoàn thành trượt đến slideTargetTransform");
#endif
    }

    #endregion ========================================

    #region ==================== CALLBACK METHODS ====================

    /// <summary>
    /// Gọi khi helicopter đến fallTargetTransform (vừa đáp đất)
    /// </summary>
    private void OnReachedFallTarget()
    {
#if UNITY_EDITOR
        if (enableDebugLogs) Debug.Log("Helicopter đã đến fallTargetTransform - Vừa đáp đất!");
#endif
        _bodyFake.SetActive(true);
        _bodyRew.SetActive(false);
        vfxExplosionGround.Play();
        vfxSlideGround.Play();
        explosionManager.TriggerExplosion(0);
    }

    /// <summary>
    /// Gọi khi helicopter đến slideTargetTransform (hoàn thành trượt)
    /// </summary>
    private void OnReachedSlideTarget()
    {
#if UNITY_EDITOR
        if (enableDebugLogs) Debug.Log("Helicopter đã đến slideTargetTransform - Hoàn thành trượt!");
#endif
        vfxSlideGround.Stop();
        step1.SetActive(false);
        _bodyFake.SetActive(false);
        vfxExplosionEnd.Play();
        explosionManager.TriggerExplosion(1);
        OnDespawn(5f);
    }

    #endregion ========================================
    

    #region ==================== SUPPORT ====================
    void RotateRotorBlades()
    {
        rotorBlades[0].Rotate(Vector3.up,  rotateSpeed[0]);
        rotorBlades[1].Rotate(Vector3.right, rotateSpeed[1]);
    }
    
    private void Get_CurrentDistance()
    {
        currentDistance = Vector3.Distance(TF.position, attackDestination);
    }

    private void Get_CurrentAngle()
    {
        Vector3 targetDir = attackDestination - TF.position;
        targetDir.y = 0;
        currentAngle = Vector3.Angle(targetDir, TF.forward);
    }
    #endregion ========================================
    

    #region ==================== DEBUG HELPER ====================

    public enum EStateType
    {
        None,
        OnStartPos,
        OnTiltAttack,
        OnDead
    }

    public enum MovingPhase
    {
        None,
        IncreasingSpd,
        OnMaxSpd,
        DecreasingSpd,
    }

    #endregion ========================================
}

#region ==================== TiltHelper ====================

public static class TiltHelper
{
    public static float OnRange(float timer, AnimationCurve CAnim)
    {
        if (CAnim.length == 0) return 0;
        return CAnim.Evaluate(
            OnRange(timer, CAnim[CAnim.length - 1].time, CAnim[0].time)
        );
    }

    public static float OnRange(float input, float max = 1, float min = 0)
    {
        return (input > max)
            ? max
            : (input < min)
                ? min
                : input;
    }
}

#endregion ========================================
