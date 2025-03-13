using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IconEffect : MonoBehaviour,IPoolObject
{
    public float defaultFlyUpDistance = 5f;
    public float duration = 1f;
    public float fadeOutTime = 0.2f;
    public Vector3 startScale;
    public Vector3 endScale;

    private Vector3 startPos;
    private Vector3 targetPos;
    private float elapsedTime;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void StartEffect(float flyDistance = -1f)
    {
        if (flyDistance < 0) flyDistance = defaultFlyUpDistance;

        startPos = transform.position;
        targetPos = startPos + Vector3.up * flyDistance;
        StartCoroutine(AnimateIcon());
    }

    private IEnumerator AnimateIcon()
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
