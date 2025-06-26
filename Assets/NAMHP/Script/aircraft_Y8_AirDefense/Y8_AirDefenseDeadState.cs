using System.Collections;
using UnityEngine;
using static Y8_AirDefenseStateMachine;

public class Y8_AirDefenseDeadState : BaseState<Y8_AirDefense>
{
    [SerializeField] private GameObject _deadStep;
    [SerializeField] private GameObject _deadStep1;
    [SerializeField] private GameObject _deadStep2;
    [SerializeField] private F15TrackingMovement f15TrackingMovement;
    [SerializeField] private GameObject Explosion;
    [SerializeField] private GameObject _model;

    [Header("Setup Points")] [SerializeField]
    private float initialDropSpeed = 1f; // Tốc độ rơi ban đầu

    [SerializeField] private float gravityAcceleration = 2f; // Gia tốc trọng lực tăng dần
    [SerializeField] private float maxDropSpeed = 15f; // Tốc độ rơi tối đa
    [SerializeField] private float disappearDelay = 2f; // Thời gian chờ trước khi biến mất

    [Header("Rotation Settings")] [SerializeField]
    private float targetRotationAngle = 45f; // Góc xoay chúc xuống mong muốn

    [SerializeField] private float rotationSpeed = 80f; // Tốc độ xoay của máy bay

    [Header("Ground Detection")] [SerializeField]
    private LayerMask groundMask;

    [SerializeField] private Transform headAirPlane;
    [SerializeField] private float dropOffset;

    [Header("Sound")] [SerializeField] private AudioSource sound;

    private float currentDropSpeed; // Tốc độ rơi hiện tại
    private bool hasExploded = false; // Đã nổ hay chưa
    private Vector3 landPos; // Vị trí chạm đất
    private Quaternion targetRotation; // Góc chúc đích đến
    private Vibration _playerVibration;

    public override void EnterState()
    {
        _playerVibration = LocalPlayer.Instance.GetComponent<Vibration>();

        // Vô hiệu hóa tracking
        f15TrackingMovement.enabled = false;

        // Cài đặt góc xoay chúc xuống
        targetRotation = Quaternion.Euler(targetRotationAngle, transform.rotation.eulerAngles.y,
            transform.rotation.eulerAngles.z);

        // Kích hoạt bước chết
        //_deadStep.SetActive(true);
        _deadStep1.SetActive(true);
        // Kích hoạt hiệu ứng chết của bot (tùy chọn)
        BotDeath.Instance.GetBotDeath();

        //StartCoroutine(StartOnDead());
        _model.SetActive(false);
        _deadStep2.SetActive(true);
        Invoke(nameof(HideBot),3f);
    }

    /// <summary>
    /// Xử lý máy bay rơi và chạm đất
    /// </summary>
    IEnumerator StartOnDead()
    {
        _playerVibration.StartShaking(0, 0.4f);
        //TriggerExplosion();
        // Tính toán vị trí chạm đất
        RaycastHit dropPosHit;
        if (Physics.Raycast(headAirPlane.position, headAirPlane.forward, out dropPosHit, 1000, groundMask))
        {
            landPos = dropPosHit.point;
            Vector3 dirNormal = (landPos - headAirPlane.position).normalized;
            landPos += dirNormal * dropOffset;
            Debug.Log("Chạm đất!");
        }
        else
        {
            landPos = transform.position + Vector3.down * 500f;
        }

        // Bắt đầu rơi và xoay đồng thời
        while (transform.position.y > landPos.y + 0.1f)
        {
            // Tăng tốc rơi theo gia tốc trọng lực
            currentDropSpeed = Mathf.Min(currentDropSpeed + gravityAcceleration * Time.deltaTime, maxDropSpeed);

            // Di chuyển xuống và tiến về phía trước
            transform.position += transform.forward * currentDropSpeed * Time.deltaTime; // Tiến tới trước
            transform.position += Vector3.down * currentDropSpeed * Time.deltaTime; // Rơi xuống

            // Xoay dần về góc mục tiêu
            transform.rotation =
                Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Kích hoạt vụ nổ nếu chạm đất
            if (!hasExploded && transform.position.y <= landPos.y)
            {
                TriggerExplosion();
                hasExploded = true;
            }

            yield return null;
        }

        // Kết thúc: Âm thanh dừng, xóa model
        sound.Stop();
        //yield return new WaitForSeconds(disappearDelay);
        _model.gameObject.SetActive(false);
        _deadStep.SetActive(false);
        _deadStep1.SetActive(false);
        _deadStep2.SetActive(true);

    }
    private void HideBot()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Kích hoạt vụ nổ.
    /// </summary>
    private void TriggerExplosion()
    {
        if (Explosion != null)
        {
            var explosion = ObjectPool.Instance.PopFromPool(Explosion, instantiateIfNone: true);
            explosion.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
            explosion.SetActive(true);
        }
    }
    
    public override void ExitState()
    {
        // Reset trạng thái nếu cần
    }

    public override Y8_AirDefense GetNextState()
    {
        return StateKey;
    }

    public override void UpdateState()
    {
        // Không có cập nhật liên tục
    }
}