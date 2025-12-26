using UnityEngine;
[System.Serializable]
public class Effect : GameUnit<GameConstants.EffectType>
{
    [Header("Thời gian tối đa mà vfx hoạt động")]
    [SerializeField] private float lifeTime;
    private float realTime;
    [SerializeField] public ParticleSystem[] particles;

    private void Update()
    {
        if (realTime > lifeTime)
        {
            OnDespawn();
        }
        else
        {
            realTime += Time.deltaTime;
        }
    }

    public void OnDespawn()
    {
        SimplePool<GameConstants.EffectType>.Despawn(this);
    }

    public void OnInit()
    {
        realTime = 0;
        if (particles.Length>0)
        {
            //transform.rotation = Quaternion.LookRotation();
            foreach (var item in particles)
                item.Play();
        }
    }
}
