// BossWeakPointManager.cs (Không tự động SetActive)
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class BossWeakPointManager : MonoBehaviour
{
    // === CÁC BIẾN ===
    [Header("Danh sách Điểm Yếu")]
    [SerializeField] private List<Detector> detectorsState1;
    [SerializeField] private List<Detector> detectorsState2;

    public Action<int> OnPhaseCleared;
    public Action<bool> OnDetectorCleared;
    // Biến này vẫn cần để biết đang kiểm tra cho phase nào
    private int currentActiveState = 0; 

    // === CÁC HÀM ===
    void Awake()
    {
        SubscribeToAllDetectors();
    }

    /// <summary>
    /// HÀM CÔNG KHAI: Được gọi từ bên ngoài để báo cho Manager biết
    /// trạng thái nào hiện đang hoạt động. Nó không còn kích hoạt GameObject nữa.
    /// </summary>
    public void NotifyStateChange(int newState)
    {
        currentActiveState = newState;
        Debug.Log($"WeakPointManager đã được thông báo: Trạng thái hiện tại là {newState}");
        // Có thể kiểm tra ngay khi chuyển trạng thái, phòng trường hợp phase đó đã xong từ trước
        CheckForPhaseCompletion();
    }

    private void SubscribeToAllDetectors()
    {
        Action<Detector> subscription = (detector) => {
            if (detector != null) detector.OnDetectorDestroyed += HandleDetectorDestroyed;
        };
        detectorsState1.ForEach(subscription);
        detectorsState2.ForEach(subscription);
    }
    
    // Hàm này được gọi khi một Detector bất kỳ bị phá hủy
    private void HandleDetectorDestroyed(Detector destroyedDetector)
    {
        Debug.Log($"Boss nhận được tin: {destroyedDetector.gameObject.name} đã bị phá hủy.");
        // Ngay sau khi một điểm yếu bị phá hủy, kiểm tra xem phase đã hoàn thành chưa.
        CheckForPhaseCompletion();
    }

    private void CheckForPhaseCompletion()
    {

        List<Detector> currentDetectors = GetDetectorListForState(currentActiveState);
        if (currentDetectors == null || currentDetectors.Count == 0) return;
        
        bool allDestroyed = currentDetectors.All(d => d.IsDestroyed());
        //Debug.Log($"Kiểm tra trạng thái {currentActiveState}: Tất cả điểm yếu đã bị tiêu diệt? {allDestroyed}");
        OnDetectorCleared?.Invoke(allDestroyed);
        if (allDestroyed)
        {
            Debug.LogWarning($"TẤT CẢ ĐIỂM YẾU CỦA TRẠNG THÁI {currentActiveState} ĐÃ BỊ TIÊU DIỆT!");
            OnPhaseCleared?.Invoke(currentActiveState);
            
        }
    }

    private List<Detector> GetDetectorListForState(int state)
    {
        if (state == 0) return detectorsState1;
        if (state == 1) return detectorsState2;
        return null;
    }

    private void OnDisable()
    {
        Action<Detector> unsubscription = (detector) => {
            if (detector != null) detector.OnDetectorDestroyed -= HandleDetectorDestroyed;
        };
        if(detectorsState1 != null) detectorsState1.ForEach(unsubscription);
        if(detectorsState2 != null) detectorsState2.ForEach(unsubscription);
       
    }
    
}