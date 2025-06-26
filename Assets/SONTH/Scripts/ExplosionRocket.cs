using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionRocket : MonoBehaviour, IPoolObject
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private ParticleSystem _explosionEffect;
    [SerializeField] private float _timeRelease = .5f;
    public GameObject Prefab { get; set; }
    public ParticleSystem ExplosionEffect => _explosionEffect;
    public void Explosion()
    {
        _audioSource?.Play();
        _explosionEffect?.Play();
        StartCoroutine(WaitToPushToPool());
    }

    public void Init()
    {
    }

    public void OnPushToPool()
    {
    }

    IEnumerator WaitToPushToPool()
    {
        yield return new WaitForSeconds(_timeRelease);
        _audioSource?.Stop();
        _explosionEffect?.Stop(); 
        var mainPar = _explosionEffect.main;
        mainPar.startSize = new ParticleSystem.MinMaxCurve(5, 7);
        ObjectPool.Instance.PushToPool(this, gameObject);
    }
}
