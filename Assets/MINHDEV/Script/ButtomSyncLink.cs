using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtomSyncLink : MonoBehaviour
{
    // Đường dẫn URL mà bạn muốn mở
    public Button DownloadBtn;
    public string url = "https://play.google.com/store/apps/details?id=com.horus.sky.defense";
    //public string url = "https://play.google.com/store/apps/details?id=com.horus.beach.head";
    // Gán sự kiện cho nút bấm
    private void Start()
    {
        DownloadBtn.onClick.AddListener(OnCickButton);

    }

    private void OnEnable()
    {

    }
    private void OnDisable()
    {
        DownloadBtn.onClick.RemoveListener(OnCickButton);
    }

    // Phương thức để mở URL
    public void OnCickButton()
    {
        ButtonCTA();
    }
    
    public void ButtonCTA()
    {
        Luna.Unity.Playable.InstallFullGame();
        Debug.Log(nameof(ButtonCTA));
    }
}
