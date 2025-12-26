using System.Collections;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using GameUtilities;
using static GameConstants;
using UnityEngine;

public class MOH_Move : StateBase
{
    [Header("Movement Settings")]
    [SerializeField] private Transform _bodyLookToPlayer;

    private Vector3 _playerPosition;

    [Tooltip("Tốc độ di chuyển của bot.")] [SerializeField]
    private float m_moveSpeed = 5.0f;

    [Tooltip("Tốc độ xoay của bot khi đổi hướng.")] [SerializeField]
    private float m_rotationSpeed = 10.0f;

    [Header("Pathing Info (Read-Only)")] [Tooltip("Tuyến đường mà bot này đang đi theo.")]
    public PointGroup AssignedPath; // Để debug trong Inspector

    public BotIdentity BotIdentity; // Tham chiếu đến BotIdentity để lấy thông tin về đường đi
    [SerializeField] private int m_currentPointIndex = 0; // Điểm tiếp theo cần đến

    [Header("Move Attack")] [SerializeField]
    private Transform pointSpawnBullet;
    [SerializeField] private ParticleSystem vfxGun;

    [Header("Fire Rocket")] 
    [SerializeField] private Transform[] _pointFireRocket;
    
    private bool canAttackPoint;
    private bool isAttackMove;
    bool RanDomAttack;
    private void OnEnable()
    {
        m_currentPointIndex = 0;
        _playerPosition = PlayerInstant.Instance.TF.position;
    }

    public void GetAssignPath()
    {
        AssignedPath = BotIdentity.AssignedPath; // Lấy đường đi từ BotIdentity
    }
    
    public override void EnterState()
    {
        isAttackMove = false;
        Invoke(nameof(Init), .1f);
    }

    void Init()
    {
        if (botContext.botNetwork.IsDeadExplosion || botContext.botNetwork.IsDead)
            return;
        
        botContext.ChangeAnimAndType(HashMove, 0);
    }

    public override void UpdateState()
    {
        if (isAttackMove)
            return;

        WaypointMovementUtility.RotateTowards(_bodyLookToPlayer, _playerPosition, m_rotationSpeed);

        if (!canAttackPoint)
        {
            if (AssignedPath == null || AssignedPath.points.Count == 0)
                return;

            Vector3 targetPosition = AssignedPath.points[m_currentPointIndex].position;

            WaypointMovementUtility.RotateTowards(TF, targetPosition, m_rotationSpeed);

            if (WaypointMovementUtility.MoveTowards(TF, targetPosition, m_moveSpeed))
            {
                m_currentPointIndex++;
                if (m_currentPointIndex >= AssignedPath.points.Count)
                {
                    canAttackPoint = true;
                    m_currentPointIndex = 0;
                }
            }
        }
        else
        {
            Vector3 targetPosition = AssignedPath.attackPoints[m_currentPointIndex].position;
            WaypointMovementUtility.RotateTowards(TF, targetPosition, m_rotationSpeed);

            if (WaypointMovementUtility.MoveTowards(TF, targetPosition, m_moveSpeed))
            {
                m_currentPointIndex = Random.Range(0, AssignedPath.attackPoints.Count);
                if (RanDomAttack)
                {
                    isAttackMove = true;
                    RanDomAttack = !RanDomAttack;
                    botContext.stateController.ChangeState(EnemyState.Attack);
                }
                else
                {
                    isAttackMove = true;
                    RanDomAttack = !RanDomAttack;
                    if (botContext.botNetwork.GetBool(2))
                        StartCoroutine(IEAttackMove(botContext.botNetwork.GetBool(0)));//bool sửa lại cho help fire ball
                    else
                        botContext.stateController.ChangeState(EnemyState.Shield);
                }
            }
        }
    }

    private IEnumerator IEAttackMove(bool CanPlayFireBall)
    {
        if (CanPlayFireBall)
        {
            botContext.ChangeAnimAndType(HashMove, 1);
            vfxGun.Play();
            float maxDuration = 3f; // Thời gian chạy tối đa (3 giây)
            
            if (GameController.Instance.CurrentGameState == GameState.InGame)
            {
                    botContext.audioPlayable.PlayAudioIndexLoop(GameConstants.AudioType.BotAttack,0);
                float timerTakeDamage = .5f;
                while (maxDuration > 0) 
                {
                    Vector3 targetPosition = AssignedPath.attackPoints[m_currentPointIndex].position;
                    WaypointMovementUtility.RotateTowards(TF, targetPosition, m_rotationSpeed);
                    WaypointMovementUtility.RotateTowards(_bodyLookToPlayer, _playerPosition, 10);
                    if (WaypointMovementUtility.MoveTowards(TF, targetPosition, m_moveSpeed))
                        m_currentPointIndex = Random.Range(0, AssignedPath.attackPoints.Count);
                    maxDuration -= Time.deltaTime;
                    timerTakeDamage += Time.deltaTime;
                    if (timerTakeDamage >= .2f)
                    {
                        timerTakeDamage = 0;
                        EventManager.Instance?.Publish(new PlayerHealthChangedEvent(damage: botContext.botNetwork.Damage, state:"OnlyDamage"));
                        BulletTrail bullet = SimplePool<GameConstants.ProjecttilePlayer>.Spawn<BulletTrail>(ProjecttilePlayer.Projectile_Bullet_Norman, pointSpawnBullet.position, Quaternion.identity);
                        bullet.Init((_playerPosition - pointSpawnBullet.position).normalized, _playerPosition);
                    }
                    yield return null;
                }
                botContext.audioPlayable.StopAllAudioDontEnbleFalse();
            }
            else
            {

                while (maxDuration <= 0) 
                {
                    maxDuration += Time.deltaTime;
                    Vector3 targetPosition = AssignedPath.attackPoints[m_currentPointIndex].position;
                    WaypointMovementUtility.RotateTowards(TF, targetPosition, m_rotationSpeed);
                    WaypointMovementUtility.RotateTowards(_bodyLookToPlayer, _playerPosition, 10);
                    if (WaypointMovementUtility.MoveTowards(TF, targetPosition, m_moveSpeed))
                        m_currentPointIndex = Random.Range(0, AssignedPath.attackPoints.Count);
                    yield return null;
                }
            }
            vfxGun.Stop();
            // botContext.ChangeAnimAndType(HashIdle);
            // yield return new WaitForSeconds(1f);
            botContext.ChangeAnimAndType(HashMove, 0);
            isAttackMove = false;
        }
        else
        {
            //TODO: bắn rocket
            int countRocket = 8;
            float timer = 0f;
            float timerDelayFire = .3f;
            int indexPoint = 0;
            while (countRocket > 0)
            {
                timer += Time.deltaTime;
                Vector3 targetPosition = AssignedPath.attackPoints[m_currentPointIndex].position;
                WaypointMovementUtility.RotateTowards(TF, targetPosition, m_rotationSpeed);
                WaypointMovementUtility.RotateTowards(_bodyLookToPlayer, _playerPosition, 10);
                if (WaypointMovementUtility.MoveTowards(TF, targetPosition, m_moveSpeed))
                    m_currentPointIndex = Random.Range(0, AssignedPath.attackPoints.Count);

                if (timer >= timerDelayFire)
                {
                    BulletBezier bullet = SimplePool<ProjectileEnemy>.Spawn<BulletBezier>(ProjectileEnemy.RocketSupersoldat, _pointFireRocket[indexPoint].position, _pointFireRocket[indexPoint].rotation);
                    bullet.Init(_pointFireRocket[indexPoint].position, PlayerInstant.Instance.explosionPos.position, 3.5f, 20);
                    timer = 0;
                    indexPoint++;
                    if (indexPoint >= _pointFireRocket.Length)
                        indexPoint = 0;
                    countRocket--;
                }
                yield return null;
            }
            
            isAttackMove = false;
        }
    }

    public override void ExitState()
    {
        StopAllCoroutines();
        vfxGun.Stop();
        botContext.audioPlayable.StopAllAudioDontEnbleFalse();
    }

    public void RotateToPlayer() => WaypointMovementUtility.RotateTowards(_bodyLookToPlayer, _playerPosition, 10);
}