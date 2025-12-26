
using System.Collections;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;
using static GameConstants;

public class ParachuteHeli111 : GameUnit<ProjectileEnemy>, ITakeDamage
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip getHitClip;
    [SerializeField] private AudioClip explosionClip;
    [SerializeField] private float speedFlyToPlayer;
    [SerializeField] private float distanceToDestroy;
    [SerializeField] private ProjectileEnemy _explosionType;
    [SerializeField] private int damage;
    [SerializeField] private float health;
    [SerializeField] private float maxHealth;
    [SerializeField] public GameObject[] gameObjectsToDisableOnHit;
    bool isExplosion = false;

    void OnEnable()
    {
        isExplosion = false;
        foreach (var obj in gameObjectsToDisableOnHit)
        {
            obj.SetActive(true);
        }
        health = maxHealth;
        StartCoroutine(IEFlyToThePlayer());
       
    }


    private void Update()
    {
        if (Vector3.Distance(TF.position, PlayerInstant.Instance.transform.position) < distanceToDestroy)
        {
            if (!isExplosion)
            {
                OnDespawn(true);
            }
           
        }

        Vector3 dir = (PlayerInstant.Instance.transform.position - TF.position).normalized;
        TF.position += dir * speedFlyToPlayer * Time.deltaTime;
    }



    private IEnumerator IEFlyToThePlayer()
    {
        Vector3 startPos = TF.position;
        Vector3 targetPos = PlayerInstant.Instance.transform.position;

        Vector3 dir = (targetPos - startPos).normalized;
        float yRotation = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        TF.rotation = Quaternion.Euler(TF.rotation.eulerAngles.x, yRotation, TF.rotation.eulerAngles.z);
        yield return null;
        
    }


    public void OnDespawn(bool isTakeDamage = false)
    {
         isExplosion = true;
        SimplePool<GameConstants.ProjectileEnemy>.Spawn<ExplosionPanzerwerfer>(_explosionType, this.transform.position, Quaternion.identity);
        audioSource.PlayOneShot(explosionClip);
        if (isTakeDamage)
        {
            EventManager.Instance?.Publish(new PlayerHealthChangedEvent(damage: damage, state: "OnlyDamage"));
            EventManager.Instance?.Publish(new CamShakeEvent(new CamShakeData { duration = .3f, strength = .025f, vibrato = 15, randomness = 45 }));
        }
        foreach (var obj in gameObjectsToDisableOnHit)
        {
            obj.SetActive(false);
        }
        SimplePool<ProjectileEnemy>.Despawn(this, 2f);
        StopAllCoroutines();
    }

    public void OnTakeDamage(DamageInfo damageInfo)
    {
        health -= damageInfo.damage;
        audioSource.PlayOneShot(getHitClip);
        if (health <= 0)
        {
            if (!isExplosion)
            {
                OnDespawn(false);
            }
            
            
        }
    }

    public Transform GetTransformThis()
    {
        return TF;
    }

    public Transform GetTransformCenter()
    {
        return TF;
    }
}
