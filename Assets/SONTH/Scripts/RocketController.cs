using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RocketController : MonoBehaviour
{
    [SerializeField] private Transform test;
    [SerializeField] private GameObject _rocket;
    [SerializeField] private Transform _posSpawn;
    public List<BotNetwork> listBot;
    private Camera _camera;
    public static RocketController Instance;
    public int bulletFollowRocket;
    public int bulletForwardRocket;
    private Animation _fireAnim;
    private int currentBullet;
    [System.NonSerialized]
    public bool isFollowRocket = false;
    
    [Header("Snake Camera")]
    [SerializeField] private Transform shakeCam; // Biến để tham chiếu đến MainCamera
    [SerializeField] private float shakeCamMin;
    [SerializeField] private float shakeCamMax;
    [SerializeField] private float duration = 0.3f; // Thời gian rung lắc
    [SerializeField] private float magnitude;

    [Header("ExplosionAudio")] 
    public AudioSource _audioSource;
    public AudioClip explosionAudio;
    
    private void Awake()
    {
        Instance = this;
    }

    //private void OnEnable()
    //{
    //    Instance = this;
    //}

    // Start is called before the first frame update
    void Start()
    {
        _camera = Camera.main;
        _fireAnim = GetComponent<Animation>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(1)) {
            Fire();
        }
    }

    public void PlayAudioExplosion()
    {
        _audioSource.PlayOneShot(explosionAudio);
    }
    
    public void Fire()
    {
        currentBullet = isFollowRocket ? bulletFollowRocket : bulletForwardRocket;
        if(currentBullet > 0)
        {
            _fireAnim.Play();
            var obj = ObjectPool.Instance.PopFromPool(_rocket, instantiateIfNone: true);
            obj.transform.SetPositionAndRotation(_posSpawn.position, transform.rotation);
            Rocket rocket = obj.GetComponent<Rocket>();
            Transform bot = GetBot();
            rocket.SetupTarget(bot);
            if (bot == null)
            {
                Debug.LogWarning("Bot not in view");
                //CameraFollowRocket.Instance.isFollow = false;
                rocket.SetFollowTargetCheck(false);
            }
            else
            {
                //CameraFollowRocket.Instance.isFollow = true;
                if (isFollowRocket) rocket.SetFollowTargetCheck(true);
            }
            //rocket.controlPoint = _controlPoint;
            rocket.Initialize(test.position);
            //CameraFollowRocket.Instance.Follow(rocket.PosCamFollow);
            currentBullet--;
        }
        else
        {
            Debug.LogWarning("Out of bullet");
        }
        if (isFollowRocket)
        {
            bulletFollowRocket = currentBullet;
            EventManager.Invoke(EventName.UpdateRocketFollowCount, bulletFollowRocket);
        }
        else
        {
            bulletForwardRocket = currentBullet;
            EventManager.Invoke(EventName.UpdateRocketForwardCount, bulletForwardRocket);
        }
    }
    public Transform GetBot()
    {
        Transform bot = null;
        List<BotNetwork> validBot = new List<BotNetwork>();
        for(int i = 0; i < listBot.Count; i++)
        {
            Vector3 viewportPoint = _camera.WorldToViewportPoint(listBot[i].transform.position);
            if(viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
                viewportPoint.z >= 0)
            {
                validBot.Add(listBot[i]);
            }
        }
        int index = -1;
        index = Random.Range(0, validBot.Count);
        if(index != -1 && validBot.Count > 0)
        {
            bot = validBot[index].transform;
            Debug.Log(bot.name);
        }
        return bot;
    }
    
    public void SnakeCameraRocket()=> StartCoroutine(ShakeCamera(duration, magnitude));
    
    // Thêm hàm rung lắc camera
    private IEnumerator ShakeCamera(float duration, float magnitude)
    {
        Quaternion originalRot = shakeCam.localRotation;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(shakeCamMin, shakeCamMax) * magnitude;
            float y = Random.Range(shakeCamMin, shakeCamMax) * magnitude;

            shakeCam.localRotation = originalRot * Quaternion.Euler(x, y, 0);

            elapsed += Time.deltaTime;

            yield return null;
        }

        shakeCam.localRotation = originalRot;
        EventManager.Invoke(EventName.OnCheckShakeCam, shakeCam.localEulerAngles);
    }
}
