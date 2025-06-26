using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SewerController : physicexplo
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private GameObject sewerParticle;
    [SerializeField] private float _delayTime = 3f;
    private float _timer;
    private bool _isActive = false;
    protected override void ActivePhysicExplosion()
    {
        base.ActivePhysicExplosion();
        _audioSource.Play();
        sewerParticle.SetActive(true);
    }

    protected override void OnEnable()
    {
        _isActive = false;
        //Invoke(nameof(ActivePhysicExplosion), _delayTime);
    }
    protected void OnDisable()
    {
        _audioSource.Stop();
        sewerParticle.SetActive(false);
    }
    protected void Update()
    {

        _timer += Time.deltaTime;
        if (_timer >= _delayTime && !_isActive)
        {
            _isActive = true;
            _timer = 0f;
            ActivePhysicExplosion();
            
        }
    }
}
