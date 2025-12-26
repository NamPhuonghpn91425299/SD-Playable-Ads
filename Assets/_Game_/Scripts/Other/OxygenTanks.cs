using System.Collections.Generic;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;
using UnityEngine.Serialization;

public class OxygenTanks : MonoBehaviour, ITakeDamage
{
    [SerializeField] public int maxHealth = 100;
    
    [SerializeField] private GameObject _explosionGameObject;
    [SerializeField] private int _dame;
    
    [Header("Gizmod Explosion")]
    [SerializeField] private Transform _centerExplosion;
    [SerializeField] private float _radiusExplosion;
    [SerializeField] private LayerMask _layerHit;
    private Vector3 _direction;
    private float _lifeTimer = 5f;
    private bool CanExplosion = true;
    [SerializeField] private SoundSource _audioSourceExplosion;
    
    [FormerlySerializedAs("_meshRendererGas")]
    [Header("DisableIfExplosion")] 
    [SerializeField] private GameObject _body;
    [SerializeField] private CapsuleCollider _capsuleColliderThis;

#if UNITY_EDITOR
    public bool DebugMode;
#endif
    
    public void CheckHitExplosion()
    {
        Collider[] cols = Physics.OverlapSphere(_centerExplosion.position, _radiusExplosion, _layerHit);
        
        if (cols.Length <= 0)
            return;
            
        List<Transform> lstRoot = new List<Transform> ();
        foreach (Collider col in cols)
            if (!lstRoot.Contains(col.gameObject.transform.root))
                lstRoot.Add(col.gameObject.transform.root);
        
        foreach(var elem in lstRoot)
        {
            var takeDamageController = elem.GetComponent<ITakeDamage>();
            if (takeDamageController == null)
                takeDamageController = elem.transform.root.gameObject.GetComponent<ITakeDamage>();
            
            if(takeDamageController != null)
            {
                var damageInfo = new DamageInfo()
                {
                    damageType = DamageType.Explosion,
                    damage = _dame,
                    posExplosion = transform.position,
                };
                takeDamageController.OnTakeDamage(damageInfo);
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(!DebugMode)
            return;
        if (!_centerExplosion) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_centerExplosion.position, _radiusExplosion);
    }
#endif

    public void OnTakeDamage(DamageInfo damageInfo)
    {
        maxHealth -= damageInfo.damage;
         _audioSourceExplosion.PlayOneShotByIndex(Random.Range(0,3));
        
        if (maxHealth <= 0)
        {
            if (!CanExplosion)
                return;

            if (_explosionGameObject != null)
                _explosionGameObject.SetActive(true);
            CanExplosion = false;
            _body.SetActive(false);
            _capsuleColliderThis.enabled = false;
            _audioSourceExplosion.PlayOneShotByIndex(3);
            CheckHitExplosion();
            Destroy(gameObject, _lifeTimer);
            EventManager.Instance?.Publish(new CamShakeEvent(new CamShakeData{duration = .3f,strength = .015f,vibrato = 15,randomness = 45}));
        }
    }

    public Transform GetTransformThis() => transform;
    public Transform GetTransformCenter() => _centerExplosion;
}
