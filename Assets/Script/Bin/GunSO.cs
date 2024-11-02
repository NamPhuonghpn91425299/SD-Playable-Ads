using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]
public class GunSO : ScriptableObject
{

    public GunType Type;
    public string Name;
    public GameObject ModePrefab;
    public GameObject bulletPrefab;
    public Vector3 SpawnPoint;
    public Vector3 SpawnbulletPoint;
    public Vector3 SpawnRotation;

    public ShootConfigSO ShootConfig;
    public TrailConfigSO TrailConfig;

    private MonoBehaviour ActiveMonoBehaviour;
    private GameObject Model;
    private float LastShootTime;
    [SerializeField]
    private ParticleSystem ShootSystem; 
    private ObjectPool<GameObject> TrailPool;

    public void Spawn (Transform parent, MonoBehaviour activeMonoBehaviour)
    {
        this.ActiveMonoBehaviour = activeMonoBehaviour;
        LastShootTime = 0;
        TrailPool = new ObjectPool<GameObject>(CreateTrail);

        Model = Instantiate(ModePrefab);
        Model.transform.SetParent(parent,false);
        Model.transform.localPosition = SpawnPoint;
        Model.transform.localRotation = Quaternion.Euler(SpawnRotation);

        ShootSystem = Model.GetComponentInChildren<ParticleSystem>();

    }    
    public void Shoot()
    {
        ShootSystem.Stop();
        ShootSystem.Play();
        //ShootSystem.Emit(1);
        Vector3 shootDirection = ShootSystem.transform.forward
            + new Vector3(
                Random.Range(-ShootConfig.spread.x, ShootConfig.spread.x),
                Random.Range(-ShootConfig.spread.y, ShootConfig.spread.y),
                Random.Range(-ShootConfig.spread.z, ShootConfig.spread.z)
                );
        shootDirection.Normalize();
        if(Physics.Raycast(ShootSystem.transform.position, shootDirection, out RaycastHit hit, float.MaxValue, ShootConfig.hitMask))
        {
            Vector3 spawnPoint = ShootSystem.transform.position + shootDirection * 2;
            ActiveMonoBehaviour.StartCoroutine(Playtrail(spawnPoint, hit.point, hit));
        }
        else
        {
            Vector3 spawnPoint = ShootSystem.transform.position + shootDirection * 2;
            ActiveMonoBehaviour.StartCoroutine(Playtrail(spawnPoint,
                                                ShootSystem.transform.position + (shootDirection* TrailConfig.missDistance),
                                                new RaycastHit()));
        }
    }    

    private IEnumerator Playtrail(Vector3 startPoint, Vector3 endPoint, RaycastHit hit)
    {
        GameObject instance = TrailPool.Get();
        instance.transform.position = startPoint;
        instance.transform.rotation = ShootSystem.transform.rotation;
        instance.gameObject.SetActive(true);
        yield return null;

        //instance.emitting = true;

        float distance = Vector3.Distance(endPoint, startPoint);
        float remainingDistance = distance;
        while( remainingDistance > 0 )
        {
            instance.transform.position = Vector3.Lerp(startPoint, endPoint, Mathf.Clamp01(1- (remainingDistance/distance)));
            remainingDistance -= TrailConfig.simulationSpeed*Time.deltaTime;
            yield return null;
        }
        //instance.transform.position = endPoint;

        //if(hit.collider != null)
        //{
        //    SurfaceManager
        //}

        yield return new WaitForSeconds(TrailConfig.duration);
        yield return null;
       // instance.emitting = false;
        instance.gameObject.SetActive(false);
        TrailPool.Release(instance);
    }    
    private GameObject CreateTrail()
    {
        //GameObject instance = new GameObject("Bull Trail");
        //TrailRenderer trail = instance.AddComponent<TrailRenderer>();
        //trail.colorGradient = TrailConfig.color;
        //trail.material = TrailConfig.trailMaterial;
        //trail.widthCurve = TrailConfig.trailCurve;
        //trail.time = TrailConfig.duration;
        //trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        //return trail;

        var bulet = Instantiate(bulletPrefab);
        
        return bulet;

    }    
}
