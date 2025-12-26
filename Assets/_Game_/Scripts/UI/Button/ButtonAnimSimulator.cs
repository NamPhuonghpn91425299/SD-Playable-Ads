using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonAnimSimulator : MonoBehaviour
{
    public RectTransform ButtonTrans;
    public float animationSpeed = 1f;
    public float scaleMax = 1.2f; // hệ số phóng to, 1.2 tức là tăng 20%
    public float scaleMin = 1; // hệ số thu nhỏ, 1 là kích thước gốc

    private void Update()
    {
        OnPlayButtonAnim();
    }

    public void OnPlayButtonAnim()
    {
        if (ButtonTrans != null)
        {
            float t = Mathf.PingPong(Time.unscaledTime * animationSpeed, 1f); // dao động từ 0 đến 1
            float scale = Mathf.Lerp(scaleMin, scaleMax, t); // nội suy tuyến tính giữa min và max
            ButtonTrans.localScale = new Vector3(scale, scale, scale);
        }
    }
}
