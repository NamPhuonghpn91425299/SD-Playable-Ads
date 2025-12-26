
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;
using static GameConstants;


public class Aircraft_Swordfish_Attack : StateBase
{
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private Transform firePointLeft;
    [SerializeField] private Transform firePointRight;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] public float distanceToFire = 10f;
    [SerializeField] private float maxAttackAngle = 30f; // Góc tối đa để có thể tấn công (độ)
    [SerializeField] private float timer;
    [SerializeField] private bool debugAngle = false; // Debug để hiển thị góc trong console
    [SerializeField] private Transform positionSpawnRocket;
    [SerializeField] private int numberRocket = 5;
    [SerializeField] private ProjectileEnemy _rocketType;
    [SerializeField] private float timeDelayBetweenRockets = 1f;
    [SerializeField] private int indexAttack;
    [SerializeField] private int currentRocketCount = 0; // Số rocket đã spawn
    [SerializeField] private bool isSpawningRockets = false; // Trạng thái đang spawn rockets
    


    void Update()
    {
        Vector3 playerPosition = PlayerInstant.Instance.transform.position;
        float distanceToPlayer = Vector3.Distance(transform.position, playerPosition);

        if (botContext.botNetwork.IsDeadExplosion || botContext.botNetwork.IsDead)
            return;
        if (GameController.Instance.CurrentGameState != GameState.InGame)
        {
            return;
        }

        // Kiểm tra khoảng cách
        if (distanceToPlayer <= distanceToFire)
        {
            // Kiểm tra góc hướng của máy bay
            if (IsPlayerInAttackAngle(playerPosition))
            {
                // Cả khoảng cách và góc đều phù hợp -> Có thể tấn công
                StartAttacking();
            }
            else
            {
                // Trong tầm bắn nhưng không đúng góc -> Không tấn công
                StopAttacking();
            }
        }
        else
        {
            // Ngoài tầm bắn -> Không tấn công
            StopAttacking();
        }
    }

    private bool IsPlayerInAttackAngle(Vector3 playerPosition)
    {
        // Tính hướng từ máy bay đến player
        Vector3 directionToPlayer = (playerPosition - transform.position).normalized;

        // Lấy hướng forward của máy bay
        Vector3 aircraftForward = transform.forward;

        // Tính góc giữa hướng máy bay và hướng tới player
        float angle = Vector3.Angle(aircraftForward, directionToPlayer);

        // Debug góc nếu được bật
        if (debugAngle)
        {
            Debug.Log($"Aircraft {gameObject.name}: Angle to player = {angle:F1}°, Max allowed = {maxAttackAngle}°");
        }

        // Trả về true nếu góc nhỏ hơn hoặc bằng góc tối đa
        return angle <= maxAttackAngle;
    }

    private void StartAttacking()
    {
        if (indexAttack == 0)
        {
            muzzleFlash.SetActive(true);
            botContext.audioPlayable.OnlyEnableAudio(GameConstants.AudioType.BotAttack, true);
            timer += Time.deltaTime;

            if (timer >= fireRate)
            {

                // Gây damage và play audio chỉ 1 lần
                if (GameController.Instance.CurrentGameState == GameState.InGame)
                {
                    EventManager.Instance?.Publish(new PlayerHealthChangedEvent(damage: botContext.botNetwork.Damage, state: "OnlyDamage"));

                }

                timer = 0f;
            }

        }
        else
        {
            // Rocket attack mode
            if (!isSpawningRockets)
            {
                // Bắt đầu spawn rockets
                isSpawningRockets = true;
                currentRocketCount = 0;
                timer = 0f;
                muzzleFlash.SetActive(true);
                botContext.audioPlayable.OnlyEnableAudio(GameConstants.AudioType.BotAttack, true);
            }

            timer += Time.deltaTime;
            if (timer >= timeDelayBetweenRockets && currentRocketCount < numberRocket)
            {
                // Spawn 1 rocket
                SimplePool<ProjectileEnemy>.Spawn<ParachuteHeli111>(_rocketType, positionSpawnRocket.position, positionSpawnRocket.rotation);
                currentRocketCount++;
                timer = 0f;

                // Nếu đã spawn đủ số rocket thì kết thúc
                if (currentRocketCount >= numberRocket)
                {
                    isSpawningRockets = false;
                    muzzleFlash.SetActive(false);
                    botContext.audioPlayable.OnlyEnableAudio(GameConstants.AudioType.BotAttack, false);
                }
            }
            
        }

    }

    private void StopAttacking()
    {
        muzzleFlash.SetActive(false);
        botContext.audioPlayable.OnlyEnableAudio(GameConstants.AudioType.BotAttack, false);
        // Reset rocket spawning state
        isSpawningRockets = false;
        currentRocketCount = 0;
    }



    public override void EnterState()
    {
        indexAttack = Random.Range(0, 2);
        timer = 0f;
        // Reset rocket spawning state
        isSpawningRockets = false;
        currentRocketCount = 0;
    }

    public override void ExitState()
    {
        // Đảm bảo dừng tấn công khi exit state
        StopAttacking();
    }

    public override void UpdateState()
    {
        // Logic chính đã được xử lý trong Update()
    }

    /// <summary>
    /// Thiết lập góc tấn công tối đa
    /// </summary>
    /// <param name="angle">Góc tối đa tính bằng độ</param>
    public void SetMaxAttackAngle(float angle)
    {
        maxAttackAngle = Mathf.Clamp(angle, 0f, 180f);
    }

    /// <summary>
    /// Lấy góc hiện tại giữa máy bay và player
    /// </summary>
    /// <returns>Góc tính bằng độ</returns>
    public float GetCurrentAngleToPlayer()
    {
        Vector3 playerPosition = PlayerInstant.Instance.transform.position;
        Vector3 directionToPlayer = (playerPosition - transform.position).normalized;
        Vector3 aircraftForward = transform.forward;
        return Vector3.Angle(aircraftForward, directionToPlayer);
    }

    /// <summary>
    /// Kiểm tra xem hiện tại có thể tấn công được không
    /// </summary>
    /// <returns>True nếu có thể tấn công</returns>
    public bool CanAttack()
    {
        Vector3 playerPosition = PlayerInstant.Instance.transform.position;
        float distanceToPlayer = Vector3.Distance(transform.position, playerPosition);
        return distanceToPlayer <= distanceToFire && IsPlayerInAttackAngle(playerPosition);
    }
}
