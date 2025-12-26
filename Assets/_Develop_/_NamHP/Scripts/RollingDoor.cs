using System;
using UnityEngine;
using System.Collections;
using Assets._Develop_.ThanhNT.Scripts.Observer;
public class RollingDoor : MonoBehaviour,
    Assets._Develop_.ThanhNT.Scripts.Observer.IObserver<GameStateChangedEvent>
{
    [Header("Door Settings")]
    public GameObject doorObject;
    public bool autoClose = false; // 🆕 Biến bật/tắt tự động đóng
    public float rollUpSpeed = .8f;
    public float rollDownSpeed = .8f;
    public float openDelay = 0f; // ⏱️ Delay trước khi mở
    public float stayOpenTime = 10f;

    private Vector3 closedScale = new Vector3(1, 1, 1);
    private Vector3 openScale = new Vector3(1, 0, 1);
    private bool isRolling = false;
    private bool isClosingSlowly = false;
    private Coroutine currentCoroutine;

    public void OnNotify(GameStateChangedEvent data)
    {
        if(data.NewState == GameConstants.GameState.InGame)
        {
            ActivateRollingDoor();
            
        }
    }
    private void Start()
    {
        doorObject.transform.localScale = closedScale;
        EventManager.Instance?.Subscribe<GameStateChangedEvent>(this);
    }

    private void OnDestroy()
    {
        EventManager.Instance?.Unsubscribe<GameStateChangedEvent>(this);
    }

    [ContextMenu("Open Door")]
    public void ActivateRollingDoor()
    {
        if (!isRolling && !isClosingSlowly)
        {
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(RollUpAndDown());
        }
    }

    [ContextMenu("Close Door")]
    public void CloseImmediately()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
        
        isRolling = false;
        
        if (!isClosingSlowly)
        {
            StartCoroutine(CloseSlowly());
        }
    }

    // 🆕 HÀM BẬT/TẮT TỰ ĐỘNG ĐÓNG
    [ContextMenu("Toggle Auto Close")]
    public void ToggleAutoClose()
    {
        autoClose = !autoClose;
        Debug.Log("Auto Close is now " + (autoClose ? "ENABLED" : "DISABLED"));
    }

    private IEnumerator CloseSlowly()
    {
        isClosingSlowly = true;
        float timer = 0f;
        Vector3 currentScale = doorObject.transform.localScale;

        while (timer < 1f)
        {
            doorObject.transform.localScale = Vector3.Lerp(currentScale, closedScale, timer);
            timer += Time.deltaTime * rollDownSpeed;
            yield return null;
        }
        
        doorObject.transform.localScale = closedScale;
        isClosingSlowly = false;
        Debug.Log("Door closed slowly!");
    }

    private IEnumerator RollUpAndDown()
    {
        isRolling = true;

        yield return new WaitForSeconds(openDelay);

        // CUỔN LÊN
        float timer = 0f;
        while (timer < 1f)
        {
            doorObject.transform.localScale = Vector3.Lerp(closedScale, openScale, timer);
            timer += Time.deltaTime * rollUpSpeed;
            yield return null;
        }
        doorObject.transform.localScale = openScale;

        // 🆕 KIỂM TRA AUTO CLOSE
        if (autoClose)
        {
            // Đếm thời gian và đóng tự động
            float openTimer = 0f;
            while (openTimer < stayOpenTime)
            {
                openTimer += Time.deltaTime;
                yield return null;
                
                // Kiểm tra nếu có lệnh đóng
                if (currentCoroutine == null) yield break;
            }

            // Đóng cửa tự động
            timer = 0f;
            while (timer < 1f)
            {
                doorObject.transform.localScale = Vector3.Lerp(openScale, closedScale, timer);
                timer += Time.deltaTime * rollDownSpeed;
                yield return null;
            }
            doorObject.transform.localScale = closedScale;
        }
        else
        {
            // Giữ cửa mở mãi mãi nếu autoClose = false
            //Debug.Log("Door will stay open (Auto Close disabled)");
            while (true)
            {
                yield return null;
                // Chỉ thoát khi có lệnh đóng từ bên ngoài
                if (currentCoroutine == null) yield break;
            }
        }

        isRolling = false;
        currentCoroutine = null;
    }
}