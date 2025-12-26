using System;
using System.Collections;
using DG.Tweening;
using static GameConstants;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Powerup<TEnumGift> : HealthObject<Gift> where TEnumGift : Enum
{
    public float durationRotation360 = 2f;
    [Header("Settings Spawn")] 
    public TEnumGift typeGift;
    
    [Header("Settings Collected")] 
    public Transform TF_Rotate; // Transform to rotate
    public GameObject myBody;
    public RectTransform ContentObj;
    public CanvasGroup ContentCanvasGroup;
    public Vector2 moveStartEndPositions = new Vector2(0f, 100f); // Start (a) and end (b) positions on the y-axis
    public float moveDuration = 1f; // Duration for moving ContentObj from a to b
    public float displayDuration = 2f; // Duration for keeping ContentObj fully visible
    public SoundSource soundSource;

    public override void OnInit()
    {
        base.OnInit();
        Vector3 targetPos = GameController.Instance.CameraMainTF.position;
        // Giữ nguyên Y của TF để tránh nghiêng lên/xuống
        targetPos.y = TF.position.y;
        TF.LookAt(targetPos);
        TF_Rotate.DOLocalRotate(new Vector3(0, 360, 0), durationRotation360, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
    }

    protected override void OnDeath()
    {
        TF.LookAt(GameController.Instance.CameraMainTF);
        //TODO: Event change weapon
        StartCoroutine(ShowFireRateContent());
        StartCoroutine(ShowEffectCollected());
    }

    public override void OnTakeDamage(DamageInfo damageInfo)
    {
        soundSource.PlayOneShotByIndex(Random.Range(0,3));
        base.OnTakeDamage(damageInfo);
    }

    private IEnumerator ShowEffectCollected()
    {
        myBody.SetActive(false);
        vfxOnDeath[0].Play();
        yield return HelperCoroutine.GetWait(0.2f);
        vfxOnDeath[1].Play();
        soundSource.PlayOneShotByIndex(3);
        yield return HelperCoroutine.GetWait(0.2f);
    }
    
    private IEnumerator ShowFireRateContent()
    {
        DOTween.Kill(TF_Rotate);
        float elapsedTime = 0f;
        Vector3 startPosition = new Vector3(ContentObj.anchoredPosition.x, moveStartEndPositions.x, 0);
        Vector3 endPosition = new Vector3(ContentObj.anchoredPosition.x, moveStartEndPositions.y, 0);
        ContentCanvasGroup.alpha = 0;

        // Move ContentObj from position a to position b while fading in
        while (elapsedTime < moveDuration)
        {
            ContentObj.anchoredPosition = Vector3.Lerp(startPosition, endPosition, elapsedTime / moveDuration);
            ContentCanvasGroup.alpha = elapsedTime / moveDuration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure it reaches the final position and alpha is set to 1
        ContentObj.anchoredPosition = endPosition;
        ContentCanvasGroup.alpha = 1;

        // Wait for displayDuration
        yield return HelperCoroutine.GetWait(displayDuration);

        // Fade out over moveDuration
        elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            ContentCanvasGroup.alpha = 1 - (elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure alpha is set to 0 after fade out
        ContentCanvasGroup.alpha = 0;
        OnDespawn();
    }
}