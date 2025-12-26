using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FighterFireEffect : MonoBehaviour
{
    [SerializeField] Transform effectTrans;
    
    [Header("Scale Animation Settings")]
    [SerializeField] Vector3 maxScale = new Vector3(1.2f, 0.6f, 1.2f);
    [SerializeField] float animationDuration = 1f;
    [SerializeField] Ease easeType = Ease.InOutSine;
    [SerializeField] bool autoPlay = true;
    
    [Header("Advanced Settings")]
    [SerializeField] bool useSequence = false;
    [SerializeField] Vector3[] keyFrameScales = {
        new Vector3(1f, 1f, 1f),
        new Vector3(1.2f, 1.1f, 1.15f),
        new Vector3(1.8f, 1.5f, 1.6f),
        new Vector3(1.4f, 1.2f, 1.3f),
        new Vector3(1f, 1f, 1f)
    };
    [SerializeField] float[] keyFrameDurations = { 0.3f, 0.4f, 0.2f, 0.4f };

    private Sequence scaleSequence;
    private Vector3 originalScale;

    void Start()
    {
        if (effectTrans == null)
        {
            this.enabled = false;
            return;
        }
        
        originalScale = effectTrans.localScale;
        
        if (autoPlay)
        {
            PlayFireAnimation();
        }
    }

    public void PlayFireAnimation()
    {
        // Kill existing animation
        KillAnimation();
        
        if (useSequence)
        {
            PlaySequenceAnimation();
        }
        else
        {
            PlaySimpleAnimation();
        }
    }
    
    void PlaySimpleAnimation()
    {
        // Simple back and forth animation
        scaleSequence = DOTween.Sequence();
        
        scaleSequence.Append(effectTrans.DOScale(maxScale, animationDuration).SetEase(easeType));
        scaleSequence.Append(effectTrans.DOScale(originalScale, animationDuration).SetEase(easeType));
        scaleSequence.SetLoops(-1, LoopType.Restart);
    }
    
    void PlaySequenceAnimation()
    {
        // Complex keyframe animation
        scaleSequence = DOTween.Sequence();
        
        for (int i = 0; i < keyFrameScales.Length - 1; i++)
        {
            Vector3 targetScale = keyFrameScales[i + 1];
            float duration = i < keyFrameDurations.Length ? keyFrameDurations[i] : 0.5f;
            
            scaleSequence.Append(effectTrans.DOScale(targetScale, duration).SetEase(easeType));
        }
        
        scaleSequence.SetLoops(-1, LoopType.Restart);
    }
    
    public void StopAnimation()
    {
        KillAnimation();
        effectTrans.localScale = originalScale;
    }
    
    public void PauseAnimation()
    {
        if (scaleSequence != null && scaleSequence.IsActive())
        {
            scaleSequence.Pause();
        }
    }
    
    public void ResumeAnimation()
    {
        if (scaleSequence != null && scaleSequence.IsActive())
        {
            scaleSequence.Play();
        }
    }
    
    void KillAnimation()
    {
        if (scaleSequence != null)
        {
            scaleSequence.Kill();
            scaleSequence = null;
        }
    }
    
    void OnDisable()
    {
        KillAnimation();
    }
    
    void OnDestroy()
    {
        KillAnimation();
    }
}