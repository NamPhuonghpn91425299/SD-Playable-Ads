using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static HelperCoroutine;

public class PlayerHP : MonoBehaviour
{
    [SerializeField] private Image HPimage;
    [SerializeField] private Gradient HPState;
    [SerializeField] private Text HPTxt;
    [SerializeField] private float HPMax;
    [SerializeField] private float HPPoint;

    private Queue<float> damageQueue = new Queue<float>();  // Hàng đợi lưu trữ sát thương
    private bool isProcessingDamage = false;

    private void OnEnable()
    {
        HPPoint = HPMax;
        HPTxt.text = HPMax.ToString();
        EventManager.AddListener<float>(EventName.OnTakeDamagePlayer, OnTakeDamagePlayer);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<float>(EventName.OnTakeDamagePlayer, OnTakeDamagePlayer);
    }

    private void OnTakeDamagePlayer(float Damage)
    {
        // Đưa lượng sát thương vào hàng đợi để xử lý
        damageQueue.Enqueue(Damage);

        // Nếu chưa có Coroutine xử lý sát thương, bắt đầu một Coroutine mới
        if (!isProcessingDamage)
        {
            StartCoroutine(ProcessDamageQueue());
        }
    }

    private IEnumerator ProcessDamageQueue()
    {
        isProcessingDamage = true;

        // Duyệt qua hàng đợi sát thương và xử lý từng lượng sát thương một
        while (damageQueue.Count > 0)
        {
            float damage = damageQueue.Dequeue();

            // Trừ lượng máu đúng với lượng sát thương trong hàng đợi
            if (HPPoint <= 0) continue;
            HPPoint -= damage;
            HPPoint = Mathf.Max(HPPoint, 0);  // Đảm bảo HP không giảm dưới 0
            EventManager.Invoke(EventName.OnPlayerDead, HPPoint <= 0);
            
            // Cập nhật giao diện
            HPTxt.text = HPPoint.ToString();
            HPimage.fillAmount = HPPoint / HPMax;
            HPimage.color = HPState.Evaluate(HPimage.fillAmount);
            // Tạo một khoảng nghỉ nhỏ để tránh trừ sát thương quá nhanh, điều này có thể điều chỉnh tùy ý
            yield return WaitSeconds(0.1f);
        }

        isProcessingDamage = false;
    }
}
