using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AircraftSystem;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using static GameConstants;
using static HelperCoroutine;
public class AircraftAttack : MonoBehaviour
{
    [SerializeField] private AircraftFlightController flightController;
    [SerializeField] private float timeOneShoot = 2f;
    [SerializeField] private GameObject muzzle;
    [SerializeField] private Transform[] boomPos;
    [SerializeField] private GameObject[] boomObj;
   
    public enum AttackType
    {
        AttackDefault,
        SingleBomb,      // Thả 1 quả bom
        DoubleRocket,    // Bắn 2 rocket
            
    }
    private void OnEnable()
    {
        muzzle.SetActive(false);
        flightController.OnAttackTriggered += HandleAttackTriggered;
    }

    private void OnDisable()
    {
        flightController.OnAttackTriggered -= HandleAttackTriggered;
    }
    private void HandleAttackTriggered(Transform attackPoint)
    {
         // Random kiểu tấn công
         AttackType randomAttack = (AttackType)UnityEngine.Random.Range(0, 3);
    
         switch (randomAttack)
         {
                 case AttackType.SingleBomb:
                     StartCoroutine(OnBoomDrop());
                    break;
                 case AttackType.DoubleRocket:
                     StartCoroutine(OnSpawnRocket());
                    break;
                 case AttackType.AttackDefault:
                 default:
                    StartCoroutine(AttackCoroutine());                    
                    break;
         }
    }

    private IEnumerator AttackCoroutine()
    {
        muzzle.SetActive(true);
        yield return GetWait(0.5f);
        if(GameController.Instance.CurrentGameState == GameState.InGame)
        {
           EventManager.Instance?.Publish(new PlayerHealthChangedEvent(damage: flightController.CurrentDamage, state:"OnlyDamage"));
        }
        Debug.Log("💥 Player hit by aircraft attack AttackDefault with Damage: " + flightController.CurrentDamage);
        yield return GetWait(timeOneShoot);
        muzzle.SetActive(false);
    }

    private IEnumerator OnBoomDrop()
    {
        boomObj[0].SetActive(false);
        if (boomPos != null)
        {
            Rocket bullet = SimplePool<ProjectileEnemy>.Spawn<Rocket>(
                ProjectileEnemy.BombDropWarthog,boomPos[0].position, boomPos[0].rotation);
            if (bullet != null)
                bullet.Init(flightController.CurrentDamage);
        }
        Debug.Log("💥 Player hit by aircraft attack SingleBomb with Damage: " + flightController.CurrentDamage); 
        yield return GetWait(timeOneShoot);
        boomObj[0].SetActive(true);
    }

    private IEnumerator OnSpawnRocket()
    {
        boomObj[1].SetActive(false);
        boomObj[2].SetActive(false);
        var bulletLeft = SimplePool<ProjectileEnemy>.Spawn<Rocket>(
            ProjectileEnemy.RocketTungWarthog2,boomPos[1].position, boomPos[1].rotation);
        if (bulletLeft != null)
            bulletLeft.Init(flightController.CurrentDamage);
        var bulletRight = SimplePool<ProjectileEnemy>.Spawn<Rocket>(
            ProjectileEnemy.RocketTungWarthog2,boomPos[2].position, boomPos[2].rotation);
        if (bulletRight != null)
            bulletRight.Init(flightController.CurrentDamage);
        Debug.Log("💥 Player hit by aircraft attack DoubleRocket with Damage: " + flightController.CurrentDamage); 
        yield return GetWait(timeOneShoot);
        boomObj[1].SetActive(true);
        boomObj[2].SetActive(true);
    }
}
