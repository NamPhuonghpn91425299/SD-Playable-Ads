using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using static GameConstants;
using Debug = UnityEngine.Debug;

public abstract class StateBase : MonoBehaviour
{
    protected Transform TF;
    protected BotContext botContext { get; private set; }
    public EnemyState StateKey { get; private set; }
    
    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    public void Initialize(EnemyState state, BotContext _botContext)
    {
        StateKey = state;
        botContext = _botContext;
        TF = _botContext.botNetwork.TF;
    }

    public virtual void AnimationFinishTrigger()
    {
        
    }

    public virtual void TriggerCenterAnimation()
    {
        
    }
}

[Serializable]
public class BotContext
{
    // public config bot
    public StateControllerBase stateController;
    public EnemyBase botNetwork;
    public BotIdentity botIdentity;
    public Animator animator;
    public AudioPlayable audioPlayable;

    [Space(5)]
    [SerializeField] private string currentAnimHash;
    
    #region Set Anim
    public void SetFloatAnim(int _nameHash, float value) => animator.SetFloat(_nameHash, value);
    public void SetIntAnim(int _nameHash, int value) => animator.SetInteger(_nameHash, value);
    public void ChangeAnimAndType(string _nameHash)
    {
#if UNITY_EDITOR
        if (animator == null)
        {
            Debug.LogError("Null anim");
            return;
        }
        // StackTrace stackTrace = new StackTrace(true); // true để lấy thông tin file và dòng
        // StackFrame caller = stackTrace.GetFrame(1); // 0 là chính hàm này, 1 là caller
        //
        // string callerInfo = $"Gọi từ: {caller.GetMethod().DeclaringType.FullName}.{caller.GetMethod().Name} (Dòng {caller.GetFileLineNumber()})";
        // UnityEngine.Debug.Log(botNetwork.gameObject.name + $" Chuyển từ {currentAnimHash} sang {_nameHash}\n{callerInfo}");
#endif
        animator.ResetTrigger(currentAnimHash);
        currentAnimHash = _nameHash;
        animator.SetTrigger(currentAnimHash);
    }
    
    public void ChangeAnimAndType(string _nameHash, int animType)
    {
#if UNITY_EDITOR
        if (animator == null)
        {
            Debug.LogError("Null anim");
            return;
        }
        // StackTrace stackTrace = new StackTrace(true); // true để lấy thông tin file và dòng
        // StackFrame caller = stackTrace.GetFrame(1); // 0 là chính hàm này, 1 là caller
        //
        // string callerInfo = $"Gọi từ: {caller.GetMethod().DeclaringType.FullName}.{caller.GetMethod().Name} (Dòng {caller.GetFileLineNumber()})";
        // UnityEngine.Debug.Log(botNetwork.gameObject.name + $" Chuyển từ {currentAnimHash} sang {_nameHash}\n{callerInfo}");
#endif
        animator.SetInteger(HashAnimType, animType);
        animator.ResetTrigger(currentAnimHash);
        currentAnimHash = _nameHash;
        animator.SetTrigger(currentAnimHash);
    }
    #endregion

    /// <summary>
    /// Set animation trigger
    /// </summary>
    public void SetAnimation(int animHash)
    {
        if (animator != null)
            animator.SetTrigger(animHash);
    }
}