using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BinhGa : MonoBehaviour
{
    [SerializeField] public int countExplosion = 4;
    
     [SerializeField] private AudioSource _audioSource;
     [SerializeField] private AudioClip _audioClip;
    [SerializeField] private ParticleSystem _explosion;
    [SerializeField] private int _dame;
    [SerializeField] private Transform _centerColli;
    [SerializeField] private Transform _centerExplosion;
    [SerializeField] private float _radiusColli;
    [SerializeField] private float _radiusExplosion;
    [SerializeField] private LayerMask _layerHit;
    private Vector3 _direction;
    private float _lifeTimer = 5f;

    public bool E;
    
    void Start()
    {
        E = true;
    }

    public void Explosion()
    {
        if (countExplosion > 0)
        {
            countExplosion--;
            return;
        }
        
        if(!E)
            return;
        _audioSource.PlayOneShot(_audioClip);
        E = false;
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        Destroy(gameObject, _lifeTimer);
        _explosion.Play();
        CheckHitCollider();
    }
    
    public void CheckHitCollider()
    {
        Collider[] cols = Physics.OverlapSphere(_centerColli.position, _radiusColli, _layerHit);
        if(cols.Length != 0 )
        {
            CheckHitExplosion();
        }
    }
    public void CheckHitExplosion()
    {
        Collider[] cols = Physics.OverlapSphere(_centerExplosion.position, _radiusExplosion, _layerHit);
        List<Transform> lstRoot = new List<Transform> ();
        foreach (Collider col in cols)
        {
            if (!lstRoot.Contains(col.gameObject.transform.root))
            {
                lstRoot.Add(col.gameObject.transform.root);
            }
        }
        foreach(var elem in lstRoot)
        {
            var takeDamageController = elem.gameObject.GetComponentInParent<ITakeDamage>();
            BotNetwork botnet = elem.gameObject.GetComponentInParent<BotNetwork>();
            
            if (botnet != null)
            {
                botnet.posExplosion = transform.position;
            }
            
            if(takeDamageController != null)
            {
                var damageInfo = new DamageInfo()
                {
                    damageType = DamageType.Gas,
                    damage = _dame,
                    name = elem.gameObject.name,
                };
                takeDamageController.TakeDamage(damageInfo);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!_centerColli || !_centerExplosion) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(_centerExplosion.position, _radiusExplosion);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(_centerColli.position, _radiusColli);
    }
}
