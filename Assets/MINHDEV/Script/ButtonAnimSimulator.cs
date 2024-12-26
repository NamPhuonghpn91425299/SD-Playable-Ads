using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonAnimSimulator : MonoBehaviour
{
    public RectTransform ButtonTrans;
    public float animationSpeed = 1f;
    public float scaleMax = 1.2f; // hệ số phóng to, 1.2 tức là tăng 20%
    public float scaleMin = 1; // hệ số phóng to, 1.2 tức là tăng 20%


    private void Update()
    {
        OnPlayButtonAnim();
    }
    public void OnPlayButtonAnim()
    {
        float scale = Mathf.PingPong(Time.unscaledTime * animationSpeed, scaleMax - scaleMin) + 1;
        ButtonTrans.localScale = new Vector3(scale, scale, scale);

    }
}
