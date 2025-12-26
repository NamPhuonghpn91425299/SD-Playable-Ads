using System.Collections;
using System.Collections.Generic;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using DG.Tweening;
using UnityEngine;

[System.Serializable]
public struct CamShakeData
{
    [Tooltip("Thời gian rung camera (tính bằng giây)")]
// Ví dụ: 0.5f nghĩa là rung trong nửa giây
    public float duration;

    [Tooltip("Độ mạnh của rung (càng cao thì camera rung càng dữ dội)," +
             "Ví dụ: 0.5f là rung nhẹ, 2.0f sẽ rung rất mạnh")]
    public float strength;

    [Tooltip("Số lần rung trong suốt khoảng thời gian (duration)," +
             "Ví dụ: 10 là rung 10 lần trong 0.5 giây")]
    public int vibrato;

    [Tooltip("Mức độ ngẫu nhiên của hướng rung (độ lệch hướng)," +
             "Càng cao thì camera rung càng loạn (khó đoán hướng rung)")]
// Ví dụ: 90f là khá loạn, 0f là rung đều một hướng
    public float randomness;
}
[System.Serializable]
public class CamShakeEvent : IGameEvent
{
    public CamShakeData _camShakeData;   
    public float Timestamp => Time.time;

    public CamShakeEvent(CamShakeData camShakeData)
    {
        _camShakeData = camShakeData;
    }


}
