using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IconEffect : MonoBehaviour,IPoolObject
{
    // public BotNetwork botNetwork;
    public float defaultFlyUpDistance = 5f;
    public float duration = 1f;
    public float fadeOutTime = 0.2f;
    public Vector3 startScale;
    public Vector3 endScaleNormal;
    public Vector3 endScaleCritical;
    private Vector3 startPos;
    private Vector3 targetPos;
    private float elapsedTime;
    private CanvasGroup canvasGroup;
    [SerializeField] private CaculatorDamageOnBot calculatorDamageOnBot;
    [SerializeField] private Text _Dmgtxt;
    [SerializeField] private Text _DmgtxtShadow;
    //[SerializeField] private int minDamage;
    //[SerializeField] private int maxDamage;
    public int _damageTotal;
    [SerializeField] private Color[] color;
    
    private void OnDisable()
    {
        // botNetwork.OnLastTakeDamage -= OnLastDamage;
    }
    
    void OnLastDamage(bool isCritical,int lastDamage)
    {
        if (isCritical)
        {
            _Dmgtxt.color = color[0];
            _Dmgtxt.text = lastDamage.ToString();
            _DmgtxtShadow.text = lastDamage.ToString();
        }
        else
        {
            _Dmgtxt.color = color[1];
            _Dmgtxt.text = lastDamage.ToString();
            _DmgtxtShadow.text = lastDamage.ToString();
        }
    }
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        //_damageTotal = calculatorDamageOnBot.damageTotal;
    }

    private void OnEnable()
    {
        // damageRan = UnityEngine.Random.Range(minDamage, maxDamage);
        // botNetwork.OnLastTakeDamage += OnLastDamage;
        //_damageTotal = calculatorDamageOnBot.damageTotal;
        transform.localScale = startScale;
        //StartEffect();
    }

    public void StartEffect(int damage, int damageCritical, float flyDistance = -1f)
    {
        _damageTotal = damage;
        //Debug.Log($"Damage {_damageTotal}---",gameObject);
        OnLastDamage(damage > damageCritical,damage);
        // _damageTotal = calculatorDamageOnBot.damageTotal;
        if (flyDistance < 0) flyDistance = defaultFlyUpDistance;

        startPos = transform.position;
        targetPos = startPos + Vector3.up * flyDistance;
        if(damage > damageCritical)
        {
            //Debug.Log($"Damage1 {damage}---{endScaleCritical}",gameObject);
            StartEffectCritical();
        }
        else if(damage <= damageCritical && damageCritical > 0)
        {
            //Debug.Log($"Damage2 {damage}---{endScaleNormal}",gameObject);
            StartEffectNormal();
        }
        else
        {
            //Debug.Log($"Damage3 {damage}---{endScaleNormal}",gameObject);
            StartEffectNormal();
        }
    }
    public void StartEffectCritical()
    {
        StartCoroutine(AnimateIcon(endScaleCritical));
    }
    public void StartEffectNormal()
    {
        StartCoroutine(AnimateIcon(endScaleNormal));
    }
    private IEnumerator AnimateIcon(Vector3 endScale)
    {
        elapsedTime = 0f;
        transform.localScale = startScale;
        if (canvasGroup) canvasGroup.alpha = 1f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            // Di chuyển và Scale icon
            transform.position = Vector3.Lerp(startPos, targetPos, progress);
            transform.localScale = Vector3.Lerp(startScale, endScale, progress);

            // Làm mờ icon dần khi gần kết thúc
            if (elapsedTime > duration - fadeOutTime && canvasGroup)
            {
                float fadeProgress = (duration - elapsedTime) / fadeOutTime;
                canvasGroup.alpha = fadeProgress;
            }
            
            yield return null;
        }
        yield return new WaitForSeconds(1f);
        ObjectPool.Instance.PushToPool(this, gameObject);
    }
    

    public GameObject Prefab { get; set; }
    public void Init()
    {
        
    }

    public void OnPushToPool()
    {
       
    }
}
