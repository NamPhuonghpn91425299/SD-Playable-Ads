using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static GameConstants;

/// <summary>
/// 🎯 Bộ điều khiển tấn công của xe tăng với DOTween
/// Phiên bản cải tiến với xoay tháp pháo mượt mà
/// </summary>
/// <remarks>
/// Component này quản lý trạng thái Tấn Công của xe tăng địch:
/// - Xoay tháp pháo hướng về phía người chơi
/// - Bắn đạn với hiệu ứng VFX
/// - Xoay tháp pháo về vị trí ban đầu
/// - Chuyển về trạng thái Di Chuyển
/// 
/// Tính năng:
/// - Xoay tháp pháo mượt mà dùng DOTween hoặc Coroutine
/// - Có thể cấu hình độ trễ và tốc độ xoay
/// - Hỗ trợ VFX và sinh đạn từ pool
/// - Tự động quản lý trạng thái
/// </remarks>
public class TankPzv_Attack : StateBase
{
    [Header("Tham Chiếu - References")]
    [SerializeField] private TankPzv_Move tankPzvMove;
    [SerializeField] public Transform attackTurret;
    [SerializeField] private Transform muzzle;
    [SerializeField] private ParticleSystem vfxAttack;
    [SerializeField] protected ProjectileEnemy _bulletType;
    
    [Header("Cài Đặt Tấn Công - Attack Settings")]
    [SerializeField, Range(10f, 180f)]
    [Tooltip("Tốc độ xoay tháp pháo (độ/giây) - Chỉ dùng cho chế độ Coroutine")]
    private float speedRotateToPlayer = 45f;
    
    [Header("Cài Đặt Animation DOTween")]
    [SerializeField, Range(0.2f, 4f)]
    [Tooltip("Thời gian xoay tháp với DOTween (giây) - Càng nhỏ xoay càng nhanh")]
    private float turretRotationDuration = 2.5f;
    
    [SerializeField, Range(0.1f, 2f)]
    [Tooltip("Độ trễ trước khi bắn (giây) - Thời gian ngắm sau khi xoay xong")]
    private float preFireDelay = 1f;
    
    [SerializeField, Range(0.1f, 2f)]
    [Tooltip("Độ trễ sau khi bắn (giây) - Thời gian chờ trước khi xoay về")]
    private float postFireDelay = 1f;
    
    [SerializeField]
    [Tooltip("Kiểu animation xoay - InOutSine cho chuyển động mượt mà")]
    private Ease turretRotateEase = Ease.InOutSine;
    
    [Header("💥 Hiệu Ứng Giật Lùi - Recoil")]
    [SerializeField] private bool enableRecoil = true;
    [SerializeField, Range(0.1f, 1f)] private float recoilDistance = 0.3f;
    [SerializeField, Range(0.05f, 0.3f)] private float recoilDuration = 0.1f;
    [SerializeField, Range(0.1f, 0.5f)] private float recoilRecoveryDuration = 0.2f;
    [SerializeField, Range(0f, 10f)] private float recoilAngle = 5f;
    
    [Header("🚗 Giật Body Xe Tăng - Tank Body Recoil")]
    [SerializeField] private bool enableBodyRecoil = true;
    [SerializeField, Range(0.05f, 0.5f)] 
    [Tooltip("Khoảng cách body xe giật lùi khi bắn (units)")]
    private float bodyRecoilDistance = 0.15f;
    [SerializeField, Range(1f, 15f)] 
    [Tooltip("Góc xoay body xe khi giật (độ) - Tạo cảm giác xe bị đẩy xoay")]
    private float bodyRecoilRotation = 5f;
    
    [SerializeField] 
    [Tooltip("Transform của body xe - Để trống sẽ tự dùng GameObject này")]
    private Transform tankBody;
    
    [Header("Tùy Chọn Debug")]
    [SerializeField] private bool useDOTween = true;
    [SerializeField] private bool debugMode = false;
    
    private Quaternion initialRotation;
    private Vector3 targetPos;
    private Coroutine coroutineAttack;
    private Sequence attackSequence;
    

    public override void EnterState()
    {
        // Validate player reference
        if (PlayerInstant.Instance?.TF == null)
        {
            if (debugMode) Debug.LogWarning($"[{gameObject.name}] No player found!");
            botContext.stateController.ChangeState(EnemyState.Move);
            return;
        }
        
        // Validate required references
        if (!ValidateReferences())
        {
            Debug.LogError($"[{gameObject.name}] Missing references!");
            botContext.stateController.ChangeState(EnemyState.Move);
            return;
        }
        
        // Use DOTween or Coroutine based on setting
        if (useDOTween)
        {
            CreateDOTweenAttackSequence();
        }
        else
        {
            coroutineAttack = StartCoroutine(IEAttack());
        }
    }
    
    #region Original Coroutine Implementation
    
    /// <summary>
    /// Phương thức tấn công dùng Coroutine truyền thống.
    /// Cho phép kiểm soát xoay tháp pháo theo từng frame.
    /// </summary>
    /// <returns>IEnumerator cho coroutine</returns>
    /// <remarks>
    /// Trình tự:
    /// 1. Lưu góc xoay ban đầu
    /// 2. Xoay tháp pháo về phía người chơi
    /// 3. Chờ độ trễ trước khi bắn
    /// 4. Bắn đạn kèm VFX
    /// 5. Xoay tháp pháo về vị trí cũ
    /// 6. Quay lại trạng thái Di Chuyển
    /// </remarks>
    private IEnumerator IEAttack()
    {
        initialRotation = attackTurret.localRotation;
        targetPos = PlayerInstant.Instance.TF.position;
        Vector3 directionToTarget = targetPos - attackTurret.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        // Rotate to target
        while (Quaternion.Angle(attackTurret.rotation, targetRotation) > 0.5f)
        {
            attackTurret.rotation = Quaternion.RotateTowards(
                attackTurret.rotation, targetRotation, speedRotateToPlayer * Time.deltaTime);
            yield return null;
        }
        
        // Pre-fire delay
        yield return HelperCoroutine.GetWait(preFireDelay);
        
        // Fire
        if (vfxAttack != null) vfxAttack.Play();
        
        if (_bulletType != null && muzzle != null)
        {
            Rocket bullet = SimplePool<ProjectileEnemy>.Spawn<Rocket>(
                _bulletType, muzzle.position, muzzle.rotation);
            if (bullet != null)
                bullet.Init(botContext.botNetwork.Damage);
        }
        
        // Rotate back
        yield return RotateTurretBack();
        
        coroutineAttack = null;
        botContext.stateController.ChangeState(EnemyState.Move);
    }

    /// <summary>
    /// Xoay tháp pháo về vị trí ban đầu sau khi bắn.
    /// Được sử dụng bởi phương thức Coroutine.
    /// </summary>
    /// <returns>IEnumerator cho việc xoay mượt</returns>
    private IEnumerator RotateTurretBack()
    {
        yield return HelperCoroutine.GetWait(postFireDelay);

        while (Quaternion.Angle(attackTurret.localRotation, initialRotation) > 0.1f)
        {
            attackTurret.localRotation = Quaternion.RotateTowards(
                attackTurret.localRotation,
                initialRotation,
                speedRotateToPlayer * Time.deltaTime
            );
            yield return null;
        }
    }
    
    #endregion
    
    public override void UpdateState()
    {
        // Tấn công được xử lý bởi sequence/coroutine, không cần cập nhật theo frame
    }
    
    public override void ExitState()
    {
        // Kill DOTween sequence if active
        if (attackSequence != null && attackSequence.IsActive())
        {
            attackSequence.Kill();
            attackSequence = null;
        }
        
        // Stop coroutine if running
        if (coroutineAttack != null)
        {
            StopCoroutine(coroutineAttack);
            coroutineAttack = null;
        }
    }
    
    #region DOTween Implementation
    
    /// <summary>
    /// 🎯 Tạo và thực thi chuỗi tấn công sử dụng DOTween.
    /// Cung cấp animation mượt mà với khả năng kiểm soát thời gian chính xác.
    /// </summary>
    /// <remarks>
    /// Các bước trong DOTween Sequence:
    /// 1. Xoay tháp pháo về phía mục tiêu (với easing)
    /// 2. Độ trễ trước khi bắn (thời gian ngắm)
    /// 3. Bắn đạn kèm VFX
    /// 4. Độ trễ sau khi bắn (thời gian hồi)
    /// 5. Xoay tháp pháo về vị trí ban đầu
    /// 6. Chuyển sang trạng thái Di Chuyển
    /// 
    /// Ưu điểm so với Coroutine:
    /// - Hiệu suất tốt hơn
    /// - Animation mượt hơn với easing
    /// - Tự động dọn dẹp khi GameObject bị hủy
    /// - Kiểm soát thời gian chính xác hơn
    /// </remarks>
    private void CreateDOTweenAttackSequence()
    {
        if (debugMode) Debug.Log($"[{gameObject.name}] Starting DOTween attack sequence");
        
        // Store initial rotation
        initialRotation = attackTurret.localRotation;
        targetPos = PlayerInstant.Instance.TF.position;
        
        // Kill any existing sequence
        attackSequence?.Kill();
        
        // Create new attack sequence
        attackSequence = DOTween.Sequence();
        
        // Step 1: Rotate turret to target
        Vector3 directionToTarget = targetPos - attackTurret.position;
        directionToTarget.y = 0; // Horizontal rotation only
        
        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            
            attackSequence.Append(
                attackTurret.DORotateQuaternion(targetRotation, turretRotationDuration)
                    .SetEase(turretRotateEase)
                    .OnComplete(() => {
                        if (debugMode) Debug.Log($"[{gameObject.name}] Turret aimed!");
                    })
            );
        }
        
        // Step 2: Pre-fire delay
        attackSequence.AppendInterval(preFireDelay);
        
        // Step 3: Fire với giật lùi!
        attackSequence.AppendCallback(() => {
            FireProjectile();
            
            // Thêm hiệu ứng giật lùi tháp pháo
            if (enableRecoil && attackTurret != null) {
                //ApplyRecoilEffect();  // Dùng bản đơn giản để test
                ApplyAdvancedRecoilEffect();
            }
            
            // Thêm hiệu ứng giật body xe tăng
            if (enableBodyRecoil) {
                ApplyBodyRecoilEffect();
                //ApplyAdvancedBodyRecoilEffect();
            }
        });
        
        // Step 4: Post-fire delay  
        attackSequence.AppendInterval(postFireDelay);
        
        // Step 5: Rotate turret back to initial position
        attackSequence.Append(
            attackTurret.DOLocalRotateQuaternion(initialRotation, turretRotationDuration)
                .SetEase(turretRotateEase)
                .OnComplete(() => {
                    if (debugMode) Debug.Log($"[{gameObject.name}] Turret reset");
                })
        );
        
        // Step 6: Return to Move state
        attackSequence.OnComplete(() => {
            OnAttackComplete();
        });
        
        // Auto-kill and link to GameObject
        attackSequence.SetLink(gameObject);
        attackSequence.SetAutoKill(true);
        
        // Play the sequence
        attackSequence.Play();
    }
    
    /// <summary>
    /// Xử lý logic bắn đạn.
    /// Sinh đạn từ object pool và phát VFX.
    /// </summary>
    /// <remarks>
    /// Các bước:
    /// 1. Phát VFX nổ súng
    /// 2. Sinh đạn từ pool tại vị trí muzzle
    /// 3. Khởi tạo đạn với giá trị sát thương
    /// 4. Xử lý lỗi sinh đạn một cách an toàn
    /// </remarks>
    private void FireProjectile()
    {
        if (debugMode) Debug.Log($"[{gameObject.name}] FIRE!");
        
        // Play VFX
        if (vfxAttack != null)
            vfxAttack.Play();
        
        // Spawn bullet
        if (_bulletType != null && muzzle != null)
        {
            try
            {
                Rocket bullet = SimplePool<ProjectileEnemy>.Spawn<Rocket>(
                    _bulletType, muzzle.position, muzzle.rotation);
                
                if (bullet != null)
                    bullet.Init(botContext.botNetwork.Damage);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[{gameObject.name}] Failed to spawn bullet: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// Callback được gọi khi chuỗi tấn công hoàn tất.
    /// Chuyển về trạng thái Di Chuyển.
    /// </summary>
    private void OnAttackComplete()
    {
        if (debugMode) Debug.Log($"[{gameObject.name}] Attack complete");
        botContext.stateController.ChangeState(EnemyState.Move);
    }
    
    /// <summary>
    /// Kiểm tra tất cả tham chiếu cần thiết.
    /// Đảm bảo hệ thống tấn công có đủ component để hoạt động.
    /// </summary>
    /// <returns>True nếu tất cả tham chiếu hợp lệ, false nếu thiếu</returns>
    /// <remarks>
    /// Kiểm tra:
    /// - Transform tháp pháo tồn tại
    /// - Transform muzzle để spawn đạn tồn tại
    /// - Prefab loại đạn đã được gán
    /// </remarks>
    private bool ValidateReferences()
    {
        bool isValid = true;
        
        if (attackTurret == null)
        {
            Debug.LogError($"[{gameObject.name}] Missing attackTurret!");
            isValid = false;
        }
        
        if (muzzle == null)
        {
            Debug.LogError($"[{gameObject.name}] Missing muzzle!");
            isValid = false;
        }
        
        if (_bulletType == null)
        {
            Debug.LogError($"[{gameObject.name}] Missing bulletType!");
            isValid = false;
        }
        
        return isValid;
    }
    
    #endregion
    
    #region Recoil Effects - Hiệu Ứng Giật Lùi
    
    /// <summary>
    /// 💥 Áp dụng hiệu ứng giật lùi đơn giản cho tháp pháo
    /// Tháp pháo giật lùi về sau rồi từ từ phục hồi
    /// </summary>
    private void ApplyRecoilEffect()
    {
        if (debugMode) {
            Debug.Log($"[{gameObject.name}] 💥 RECOIL!");
            Debug.Log($"Turret Rotation: {attackTurret.localEulerAngles}");
        }
        
        // Tạo sequence riêng cho recoil (chạy song song)
        Sequence recoilSeq = DOTween.Sequence();
        
        // Lưu vị trí ban đầu
        Vector3 originalPos = attackTurret.localPosition;
        
        // Tính hướng giật lùi dựa trên hướng HIỆN TẠI của tháp pháo
        Vector3 recoilDirection = Vector3.zero;
        // Tự động: Giật ngược hướng nòng súng (giả sử nòng hướng +Z local)
        // Chuyển từ local space của tháp pháo sang world, rồi sang local space của parent
        Vector3 backDirection = attackTurret.TransformDirection(Vector3.back);
        if (attackTurret.parent != null) {
            // Chuyển từ world về local space của parent
            backDirection = attackTurret.parent.InverseTransformDirection(backDirection);
        }
        recoilDirection = backDirection * recoilDistance;
        if (debugMode) Debug.Log($"Auto: Recoil direction = {recoilDirection}");
        
        Vector3 recoilPos = originalPos + recoilDirection;
        
        if (debugMode) {
            Debug.Log($"Recoil Direction: {recoilDirection}");
            Debug.Log($"From: {originalPos} To: {recoilPos}");
        }
        
        // Bước 1: GIẬT LÙI NHANH (0.1s)
        recoilSeq.Append(
            attackTurret.DOLocalMove(recoilPos, recoilDuration)
                .SetEase(Ease.OutQuad) // Nhanh rồi chậm = mạnh mẽ
        );
        
        // Bước 2: PHỤC HỒI CHẬM (0.2s)
        recoilSeq.Append(
            attackTurret.DOLocalMove(originalPos, recoilRecoveryDuration)
                .SetEase(Ease.InOutSine) // Mượt mà
        );
        
        // Chạy và tự động dọn dẹp
        recoilSeq.SetLink(gameObject);
        recoilSeq.SetAutoKill(true);
        recoilSeq.Play();
    }
    
    /// <summary>
    /// 💥🎯 Hiệu ứng giật lùi nâng cao
    /// Vừa giật lùi vừa ngẩng lên (muzzle climb)
    /// </summary>
    private void ApplyAdvancedRecoilEffect()
    {
        if (!enableRecoil || attackTurret == null) return;
        
        if (debugMode) Debug.Log($"[{gameObject.name}] 💥🎯 Advanced RECOIL!");
        
        Sequence recoilSeq = DOTween.Sequence();
        
        // Lưu vị trí và góc ban đầu
        Vector3 originalPos = attackTurret.localPosition;
        Vector3 originalRot = attackTurret.localEulerAngles;
        
        // Tính giật lùi theo hướng HIỆN TẠI của tháp pháo
        Vector3 backDirection = attackTurret.TransformDirection(Vector3.back);
        if (attackTurret.parent != null) {
            backDirection = attackTurret.parent.InverseTransformDirection(backDirection);
        }
        Vector3 recoilPos = originalPos + backDirection * recoilDistance;
        Vector3 recoilRot = originalRot + new Vector3(-recoilAngle, 0, 0); // Ngẩng lên
        
        // GIẬT: Lùi + Ngẩng CÙNG LÚC
        recoilSeq.Append(
            attackTurret.DOLocalMove(recoilPos, recoilDuration)
                .SetEase(Ease.OutQuad)
        );
        
        recoilSeq.Join( // Join = song song
            attackTurret.DOLocalRotate(recoilRot, recoilDuration)
                .SetEase(Ease.OutQuad)
        );
        
        // Thêm rung nhẹ
        recoilSeq.Join(
            attackTurret.DOShakeRotation(
                recoilDuration,
                new Vector3(1, 0.5f, 0), // Độ rung
                10, // Tần số
                90  // Random
            )
        );
        
        // PHỤC HỒI: Vị trí + Góc CÙNG LÚC
        recoilSeq.Append(
            attackTurret.DOLocalMove(originalPos, recoilRecoveryDuration)
                .SetEase(Ease.InOutSine)
        );
        
        recoilSeq.Join(
            attackTurret.DOLocalRotate(originalRot, recoilRecoveryDuration)
                .SetEase(Ease.InOutSine)
        );
        
        recoilSeq.SetLink(gameObject).Play();
    }
    
    #endregion
    
    #region Tank Body Recoil - Giật Body Xe Tăng
    
    /// <summary>
    /// 🚗💥 Áp dụng hiệu ứng giật cho body xe tăng
    /// Body xe giật lùi + xoay nhẹ để tạo cảm giác mạnh mẽ
    /// </summary>
    private void ApplyBodyRecoilEffect()
    {
        // Tìm body xe tăng nếu chưa gán
        if (tankBody == null) {
            tankBody = transform; // Dùng chính GameObject này
        }
        
        if (tankBody == null) return;
        
        if (debugMode) Debug.Log($"[{gameObject.name}] 🚗💥 BODY RECOIL!");
        
        // Tạo sequence riêng cho body recoil
        Sequence bodyRecoilSeq = DOTween.Sequence();
        
        // Lưu vị trí và góc ban đầu của body xe
        Vector3 originalBodyPos = tankBody.localPosition;
        Vector3 originalBodyRot = tankBody.localEulerAngles;
        
        // Tính hướng giật lùi của body
        // -tankBody.forward: Giật ngược hướng xe đang đối diện
        // * bodyRecoilDistance: Nhân với khoảng cách giật
        Vector3 bodyRecoilDirection = -tankBody.forward * bodyRecoilDistance;
        Vector3 bodyRecoilPos = originalBodyPos + bodyRecoilDirection;
        
        // Tính góc xoay giật (random để tự nhiên hơn)
        // Random.Range(-5, 5): Xoay ngẫu nhiên từ -5° đến +5°
        // Tạo cảm giác xe bị đẩy xoay bởi lực giật
        float randomRotation = Random.Range(-bodyRecoilRotation, bodyRecoilRotation);
        Vector3 bodyRecoilRot = originalBodyRot + new Vector3(0, randomRotation, 0);
        
        // Bước 1: GIẬT - Lùi + Xoay + Rung CÙNG LÚC
        // Append: Thêm animation di chuyển lùi
        bodyRecoilSeq.Append(
            tankBody.DOLocalMove(bodyRecoilPos, recoilDuration)
                .SetEase(Ease.OutQuad) // OutQuad: Bắt đầu nhanh, kết thúc chậm = giật mạnh
        );
        
        // Join: Chạy đồng thời với animation trên
        // Xoay body xe một góc ngẫu nhiên
        bodyRecoilSeq.Join(
            tankBody.DOLocalRotate(bodyRecoilRot, recoilDuration)
                .SetEase(Ease.OutQuad)
        );
        
        // Join: Thêm rung nhẹ cho chân thực hơn
        // DOShakeRotation(duration, strength, vibrato, randomness)
        // - duration: Thời gian rung (= recoilDuration để đồng bộ)
        // - strength: Độ mạnh rung (0.5f = nhẹ)
        // - vibrato: Tần số rung (5 = vừa phải)
        // - randomness: Độ ngẫu nhiên (90 = rất ngẫu nhiên)
        bodyRecoilSeq.Join(
            tankBody.DOShakeRotation(
                recoilDuration,
                new Vector3(0.5f, 0.5f, 0.5f), // Rung nhẹ theo cả 3 trục
                5, // Tần số
                90 // Random
            )
        );
        
        // Bước 2: PHỤC HỒI - Về vị trí + góc ban đầu
        // Append: Sau khi giật xong, bắt đầu phục hồi
        bodyRecoilSeq.Append(
            tankBody.DOLocalMove(originalBodyPos, recoilRecoveryDuration)
                .SetEase(Ease.InOutSine) // InOutSine: Chậm-nhanh-chậm = mượt mà
        );
        
        // Join: Đồng thời xoay về góc ban đầu
        bodyRecoilSeq.Join(
            tankBody.DOLocalRotate(originalBodyRot, recoilRecoveryDuration)
                .SetEase(Ease.InOutSine) // Phục hồi mượt
        );
        
        // Auto cleanup
        bodyRecoilSeq.SetLink(gameObject);
        bodyRecoilSeq.SetAutoKill(true);
        bodyRecoilSeq.Play();
    }
    
    /// <summary>
    /// 🚗💥🎯 Hiệu ứng giật body nâng cao với camera shake
    /// </summary>
    private void ApplyAdvancedBodyRecoilEffect()
    {
        if (!enableBodyRecoil) return;
        
        // Giật body
        ApplyBodyRecoilEffect();
        
        // Thêm camera shake để tăng cảm giác mạnh mẽ
        if (Camera.main != null)
        {
            // DOShakePosition: Rung vị trí camera
            // 0.3f: Thời gian rung
            // 0.2f: Độ mạnh di chuyển
            // 10: Tần số rung
            // 90: Độ ngẫu nhiên
            Camera.main.DOShakePosition(
                0.3f,      // Duration - thời gian rung
                0.2f,      // Strength - độ mạnh
                10,        // Vibrato - tần số rung
                90         // Randomness - độ ngẫu nhiên
            );
            
            // DOShakeRotation: Rung góc camera
            // 3f: Độ mạnh xoay (lớn hơn position để thấy rõ)
            Camera.main.DOShakeRotation(
                0.3f,      // Duration
                3f,        // Strength - mạnh hơn position
                10,        // Vibrato
                90         // Randomness
            );
        }
    }
    
    #endregion
}
