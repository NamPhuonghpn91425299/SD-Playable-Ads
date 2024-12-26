using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static BotPlayItaStateMachine;
using static BotTankStateMachine;
using static UnityEngine.GraphicsBuffer;
public class BotTankAttackState : BaseState<TankState>
{
    [SerializeField] protected BotConfigSO BotConfigSO;
    [SerializeField] private TankBaseMovement tankMovement;
    [SerializeField] protected BotNetwork botNetwork;
    [SerializeField] bool isActtacked;
    [SerializeField] protected GameObject muzzle;
    [SerializeField] protected ParticleSystem muzzlePS;
    public float lastAttackTime;
    public GameObject bazookaPrefab; // Prefab của đạn bazooka

    public override void EnterState()
    {

        isActtacked = false;
        botNetwork.StartCoroutine(AttackCoroutine(BotConfigSO.attackDuration));
    }

    public override void ExitState()
    {

    }

    public override void UpdateState()
    {
        
    }
    private IEnumerator AttackCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        yield return RotateTurretToTargetAndBack(tankMovement.target);

        int numAttacks = Mathf.FloorToInt(BotConfigSO.timeAttack * BotConfigSO.fireRate); // Số lần bắn trong timeAttack
        for (int i = 0; i < numAttacks; i++)
        {
            //_source.Play();  // Phát âm thanh cho mỗi phát bắn
            // Tạo đạn
            var rocketTank = ObjectPool.Instance.PopFromPool(bazookaPrefab,instantiateIfNone: true);
            rocketTank.transform.SetPositionAndRotation(muzzle.transform.position, muzzle.transform.rotation);
            RocketOnBot bullet = rocketTank.GetComponent<RocketOnBot>();
            // Tính toán damage dựa vào ngưỡng máu
            // Khởi tạo thông số cho đạn
            Vector3 directionToTarget = (tankMovement.target.position - muzzle.transform.position).normalized;
            bullet.transform.rotation = Quaternion.LookRotation(directionToTarget);
            if (botNetwork.IsImmortal == true)
            {
                bullet.Initialize(99999f, directionToTarget);
            }
            else
            {

                bullet.Initialize(BotConfigSO.damage, directionToTarget);
            }

            // Chờ theo tốc độ bắn
            yield return new WaitForSeconds(1f / BotConfigSO.fireRate);
        }
        yield return RotateTurretBack(tankMovement.target);
        lastAttackTime = Time.time;
        isActtacked = true;
    }

    [SerializeField]private Quaternion initialRotation;  // Lưu giá trị quay ban đầu của tháp pháo

    // Hàm để quay tháp pháo đến target và quay lại giá trị ban đầu
    public IEnumerator RotateTurretToTargetAndBack(Transform target)
    {
        // Lưu lại rotation ban đầu của tháp pháo
        initialRotation = tankMovement.acttackTurret.localRotation;

        // Kiểm tra nếu target không null
        if (target != null)
        {
            // Tính toán hướng tới mục tiêu
            Vector3 directionToTarget = (target.position - tankMovement.acttackTurret.position).normalized;
            // Tạo quaternion quay theo hướng mục tiêu (chỉ xét trên trục XZ)
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));
            // Quay tháp pháo (hoặc game object cụ thể) về hướng mục tiêu
            while (Quaternion.Angle(tankMovement.acttackTurret.rotation, lookRotation) > 0.1f)
            {
                // Sử dụng Slerp để quay mượt mà
                tankMovement.acttackTurret.rotation = Quaternion.Slerp(
                    tankMovement.acttackTurret.rotation,
                    lookRotation,
                    BotConfigSO.turretRotation * Time.deltaTime
                );
                yield return null;
            }
            muzzle.SetActive(true);
            muzzlePS.GetComponent<ParticleSystem>().Play();
           // FireBazooka();

        }
    }


    public IEnumerator RotateTurretBack(Transform target)
    {
        // Sau khi quay đến target, quay lại rotation ban đầu
        yield return new WaitForSeconds(1f);  // Chờ 1 giây (hoặc có thể tùy chỉnh thời gian)

        // Quay lại rotation ban đầu
        while (Quaternion.Angle(tankMovement.acttackTurret.localRotation, initialRotation) > 0.1f)
        {
            tankMovement.acttackTurret.localRotation = Quaternion.Slerp(
                tankMovement.acttackTurret.localRotation,
                initialRotation,                            // Quay về rotation ban đầu
                BotConfigSO.turretRotation * Time.deltaTime
            );
            yield return null;
        }
    }
        
    public override TankState GetNextState()
    {
        if (botNetwork.IsDead)
        {
            return TankState.Dead;
        }
        else 
        {
            if (isActtacked && Time.time >= lastAttackTime + BotConfigSO.attackDuration)
            {
                return TankState.MoveToAttack;
            }
            else
            {
                return StateKey;
            }
        }

    }
}
