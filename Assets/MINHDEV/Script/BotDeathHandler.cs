using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BotDeathHandler : MonoBehaviour
{
    public static BotDeathHandler Instance { get; private set; } // Singleton instance

    public Camera mainCamera; // Camera chính trong Scene
    public GameObject headShotIconPrefab; // Prefab của icon HeadShot
    public Canvas uiCanvas; // Canvas chính của UI
    public float headOffset = 1.5f; // Khoảng cách từ vị trí bot đến đầu của bot
    public float TimeDisAppear = 1.5f; // Thời gian icon tồn tại trước khi biến mất
    public float durationTime;
    public float EndAlpha;
    public Vector3 StartScale;
    public Vector3 EndScale;

    private void Awake()
    {
        // Đảm bảo rằng chỉ có một instance của Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Phương thức gọi khi bot chết
    public void OnBotDeath(Vector3 botPosition)
    {
        // Chuyển đổi vị trí bot từ thế giới sang màn hình và thêm offset để biểu tượng xuất hiện trên đầu bot
        Vector3 botHeadPosition = botPosition + Vector3.up * headOffset;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(botHeadPosition);

        // Lấy icon HeadShot từ Object Pool
        GameObject headShotIcon = ObjectPool.Instance.PopFromPool(headShotIconPrefab, false, true, uiCanvas.transform);
        RectTransform headShotIconTrans = headShotIcon.GetComponent<RectTransform>();
        CanvasGroup headShotIconCVG = headShotIcon.GetComponent<CanvasGroup>();
        StartCoroutine(OnShowIconHeadShot(headShotIconTrans, headShotIconCVG));
        headShotIconTrans.position = screenPosition;

        // Tùy chọn: Thêm logic để icon tự động biến mất sau một khoảng thời gian
        StartCoroutine(ReturnIconToPool(headShotIcon, TimeDisAppear));
    }

    private IEnumerator ReturnIconToPool(GameObject icon, float delay)
    {
        yield return new WaitForSeconds(delay);
        RectTransform rectTransform = icon.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = icon.GetComponent<CanvasGroup>();
        yield return StartCoroutine(OnHideIconHeadShot(rectTransform, canvasGroup));
        ObjectPool.Instance.PushToPool(icon.GetComponent<IPoolObject>(), icon);
    }

    private IEnumerator OnShowIconHeadShot(RectTransform rectTransform, CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 0f;
        rectTransform.localScale = StartScale;
        float timeElapsed = 0;

        while (timeElapsed < durationTime)
        {
            float time = timeElapsed / durationTime;
            rectTransform.localScale = Vector3.Lerp(StartScale, EndScale, time);
            canvasGroup.alpha = Mathf.Lerp(0, EndAlpha, time);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = EndAlpha;
        rectTransform.localScale = EndScale;
    }

    private IEnumerator OnHideIconHeadShot(RectTransform rectTransform, CanvasGroup canvasGroup)
    {
        float targetAlpha = 0;
        float timeElapsed = 0;

        while (timeElapsed < durationTime)
        {
            float time = timeElapsed / durationTime;
            rectTransform.localScale = Vector3.Lerp(EndScale, StartScale, time);
            canvasGroup.alpha = Mathf.Lerp(EndAlpha, targetAlpha, time);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
        rectTransform.localScale = StartScale;
    }
}
