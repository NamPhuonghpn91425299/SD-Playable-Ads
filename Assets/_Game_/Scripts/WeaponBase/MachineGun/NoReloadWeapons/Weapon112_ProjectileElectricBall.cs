using System.Collections.Generic;
using UnityEngine;

// public class Weapon112_ProjectileElectricBall : ProjectileExplosionBase
// {
//     [SerializeField] private ParticleSystem _electricBallEffectChid;
//     [SerializeField] private Transform _electricBallTransformChid;
//     
//     [Header("Max Size and Damage")]
//     [SerializeField] int _damageElectricLine = 10;
//     [SerializeField] float _maxSize = 1f;
//     [SerializeField] int _maxDamage = 200;
//
//     Dictionary<GameObject, bool> _damageDictionary = new Dictionary<GameObject, bool>();
//     private List<EffectLine> _effectElectricLines = new List<EffectLine>();
//
//     public override void OnInit(Vector3 _direction, int _percentSize)
//     {
//         base.OnInit(_direction, _percentSize);
//         _electricBallTransformChid.localScale =Vector3.one*(_maxSize* _percentSize / 100f);
//         _damage = (int)(_maxDamage * _percentSize / 100f);
//         _electricBallEffectChid.Play();
//         _damageDictionary.Clear();
//         _effectElectricLines.Clear();
//     }
//     
//     protected override void Update()
//     {
//         base.Update();
//         CheckDienGiat();
//     }
//
//     private float colsLeng = 0;
//     public void CheckDienGiat()
//     {
//         Collider[] cols = Physics.OverlapSphere(TF.position, _radiusExplosion, _layerHit);
//         if (cols.Length == colsLeng)
//             return;
//         else
//             colsLeng = cols.Length;
//
//         foreach (Collider col in cols)
//         {
//             // Ưu tiên lấy ITakeDamage từ chính collider đó
//             ITakeDamage iTakeDamage = col.GetComponent<ITakeDamage>() ?? col.GetComponentInParent<ITakeDamage>();
//
//             if (iTakeDamage != null)
//             {
//                 GameObject damageTarget = iTakeDamage.GetTransformThis().gameObject;
//
//                 if (!_damageDictionary.ContainsKey(damageTarget))
//                 {
//
//                     var damageInfo = new DamageInfo()
//                     {
//                         damageType = DamageType.Normal,
//                         damage = _damageElectricLine,
//                         name = col.gameObject.name,
//                     };
//
//                     _damageDictionary.Add(damageTarget, true);
//                     CreateElectricLine(iTakeDamage.GetTransformCenter());
//                     iTakeDamage.TakeDamage(damageInfo);
//                 }
//             }
//         }
//     }
//
//     public override void CheckHitExplosion()
//     {
//         Collider[] cols = Physics.OverlapSphere(TF.position, _radiusExplosion, _layerHit);
//         foreach (Collider col in cols)
//         {
//             // Ưu tiên lấy ITakeDamage từ chính collider đó
//             ITakeDamage iTakeDamage = col.GetComponent<ITakeDamage>() ?? col.GetComponentInParent<ITakeDamage>();
//
//             if (iTakeDamage != null)
//             {
//                 GameObject damageTarget = iTakeDamage.GetTransformThis().gameObject;
//
//                 if (!_damageDictionary.ContainsKey(damageTarget))
//                 {
//
//                     var damageInfo = new DamageInfo()
//                     {
//                         damageType = DamageType.Gas,
//                         damage = _damageElectricLine,
//                         name = col.gameObject.name,
//                     };
//
//                     _damageDictionary.Add(damageTarget, true);
//                     CreateElectricLine(iTakeDamage.GetTransformCenter());
//                     iTakeDamage.TakeDamage(damageInfo);
//                 }
//             }
//         }
//     }
//
//     public override void OnDespawn()
//     {
//         base.OnDespawn();
//         colsLeng = 0;
//         foreach (EffectLine effect in _effectElectricLines)
//         {
//             effect.OnDespawn();
//         }
//         _damageDictionary.Clear();
//         _effectElectricLines.Clear();
//     }
//
//     public void CreateElectricLine(Transform _pointCenter)
//     {
//         EffectLine effectElectricLine = SimplePool.Spawn<EffectLine>(GameConstants.PoolType.vfx_electricLine, TF.position, Quaternion.identity);
//         effectElectricLine.OnInit(TF, _pointCenter,Vector3.Distance(_pointCenter.position,TF.position));
//         _effectElectricLines.Add(effectElectricLine);
//     }
// }
