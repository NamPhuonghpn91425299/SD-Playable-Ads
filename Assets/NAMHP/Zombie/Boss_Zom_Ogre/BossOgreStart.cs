using System;
using System.Collections;
using System.Collections.Generic;
using static HelperCoroutine;
using static NUtiliti;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class BossOgreStart : BaseState<BossOgreState>
{
    [SerializeField] private BotNetwork botNet;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip audioClip;
    [SerializeField] private float timeDelay;
    
    public Animator anim;
    
    public Vector3 startingShakeDistance;               // Khoảng cách tối đa mà GameObject sẽ di chuyển khi rung (theo mỗi trục)
    public Quaternion startingRotationAmount;           // Lượng xoay tối đa mà GameObject sẽ thực hiện khi rung (thành phần x,y,z của Quaternion)
    public float shakeSpeed = 60.0f;                    // Tốc độ của chuyển động rung (ảnh hưởng đến tần suất của hàm Sin)
    public float decreaseMultiplier = 0.5f;             // Hệ số giảm biên độ rung sau mỗi chu kỳ (0 đến 1).
    public int numberOfShakes = 8;                      // Số chu kỳ rung trước khi dừng (nếu shakeContinuous là false)
    
    public Transform playerTarget;                      // Kéo Transform của Player vào đây trong Inspector, hoặc tìm bằng code
    public float rotationSpeed = 2f;                    // Tốc độ xoay (đơn vị: radians/giây hoặc độ/giây tùy cách bạn dùng)
    [SerializeField] public bool isStartDone;
    [SerializeField] public bool isRotateDone;
    readonly int ogreRoar = Animator.StringToHash("IsRoar");
    private Coroutine lookAndAnimateCoroutine; // Để theo dõi coroutine hiện tại
    Coroutine delaystart;

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }
    
    /// <summary>
    /// Bắt đầu quá trình xoay về phía người chơi và sau đó chạy animation.
    /// </summary>
    public void LookAtPlayerAndPlayAnimation()
    {
        if (lookAndAnimateCoroutine != null)
        {
            StopCoroutine(lookAndAnimateCoroutine);
        }

        lookAndAnimateCoroutine = StartCoroutine(LookAtAndAnimate(
            transform,
            playerTarget,
            anim,
            rotationSpeed,
            ogreRoar,
            isRotateDone));
        delaystart = StartCoroutine(IEDelayStartDone(timeDelay));

    }
    
    public override void EnterState()
    {

        if (playerTarget == null)
        {
            playerTarget = LocalPlayer.Instance.GetTranformPlayer();
        }
        isStartDone = false;
        LookAtPlayerAndPlayAnimation();

    }
    
    IEnumerator IEDelayStartDone(float time)
    {
        yield return WaitSeconds(time);
        anim.SetBool(ogreRoar, false);
        isStartDone = true;
    }

    public void PlaySound()
    {
        audioSource.PlayOneShot(audioClip);
        Debug.Log("Play sound: " + audioClip.name);
    }

    public void ShakeCamera()
    {
        Vibration.Instance.StartShaking(
            startingShakeDistance, 
            startingRotationAmount, 
            shakeSpeed, 
            decreaseMultiplier, 
            numberOfShakes);
    }
    public override void UpdateState()
    {
        
    }
    public override void ExitState()
    {

        // Thông báo cho Move State rằng đã hoàn thành Start
        var moveState = GetComponent<BossOgreMoveState>();
        if (moveState != null)
        {
            moveState.SetStartCompleted();
        }

        if(delaystart!=null)
            StopCoroutine(delaystart);
        anim.SetBool(ogreRoar, false);
        audioSource.Stop();
    }
    public override BossOgreState GetNextState()
    {
        if (botNet.DeadExplosion)
            return BossOgreState.DeadExplosion;
        else
        {
            if(botNet.IsDead)
            {
                return BossOgreState.Dead;
            }
            else
            {
                if (isStartDone)
                {
                    return BossOgreState.Move;
                }
                else {
                    return StateKey;
                }

            }
        }
      
    }
    
}
// editor window for play anim

#if UNITY_EDITOR
[CustomEditor(typeof(BossOgreStart))]
public class BossOgreStartEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    
        BossOgreStart bossOgreStart = (BossOgreStart)target;
        if (GUILayout.Button("Play Animation"))
        {
            bossOgreStart.anim.SetBool("IsRoar", true);
            bossOgreStart.anim.Play("Ogre_Roar");
        }
        if (GUILayout.Button("Stop Animation"))
        {
            bossOgreStart.anim.SetBool("IsRoar", false);
        }
    }
}
#endif
    
