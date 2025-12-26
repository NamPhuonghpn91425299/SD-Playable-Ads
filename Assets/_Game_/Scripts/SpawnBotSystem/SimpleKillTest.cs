using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// LÀ MỘT CÔNG CỤ DEBUG ĐƠN GIẢN.
/// Cung cấp các nút bấm trên màn hình để kiểm tra nhanh các chức năng cốt lõi của GameManager.
/// </summary>
public class SimpleKillTest : MonoBehaviour
{
    // [System.Diagnostics.Conditional("UNITY_EDITOR")]
    // private void OnGUI()
    // {
    //     GUILayout.BeginArea(new Rect(10, 10, 200, 100));
    //     GUILayout.Label("--- TEST CONTROLS ---");
    //
    //     if (GUILayout.Button("Kill First Bot"))
    //     {
    //         Test_KillFirstBot();
    //     }
    //
    //     if (GUILayout.Button("Force Next Round"))
    //     {
    //         Test_ForceNextRound();
    //     }
    //
    //     GUILayout.EndArea();
    // }
    [Header("UI Elements")]
    [Tooltip("Dòng text hiển thị thông tin round, ví dụ: 'Round 1 / 3: The First Wave'")]
    [SerializeField] private Text roundInfoText;

    [Tooltip("Dòng text hiển thị số bot đã giết, ví dụ: 'Killed: 10 / 45'")]
    [SerializeField] private Text botCountText;

    [Tooltip("Panel thông báo chiến thắng, sẽ được bật lên khi hoàn thành level.")]
    [SerializeField] private GameObject levelCompletePanel;

    private void Start()
    {
        // Ẩn các panel không cần thiết khi bắt đầu
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
    }

    /// <summary>
    /// HÀM NHẬN SỰ KIỆN OnRoundStart.
    /// Nó có các tham số khớp với sự kiện: (string, int, int).
    /// </summary>
    public void UpdateRoundInfo(string roundName, int currentRound, int totalRounds)
    {
        if (roundInfoText != null)
        {
            roundInfoText.text = $"{roundName}";
        }
    }

    /// <summary>
    /// HÀM NHẬN SỰ KIỆN OnBotCountChanged.
    /// Nó có các tham số khớp với sự kiện: (int, int).
    /// </summary>
    public void UpdateBotCount(int killedCount, int totalCount)
    {
        if (botCountText != null)
        {
            botCountText.text = $"Killed: {killedCount} / {totalCount}";
        }
    }

    /// <summary>
    /// HÀM NHẬN SỰ KIỆN OnLevelComplete.
    /// Nó không có tham số.
    /// </summary>
    public void ShowLevelCompletePanel()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }
    }
    /// <summary>
    /// Tìm bot đầu tiên đang hoạt động và yêu cầu nó tự hủy.
    /// </summary>
    
    public void Test_KillFirstBot()
    {
        var botToKill = FindObjectOfType<BotIdentity>();

        if (botToKill != null)
        {
            Debug.Log($"<color=green>[TEST SCRIPT]</color> Found bot '{botToKill.name}'. Ordering it to self-destruct.");
            botToKill.Bot_ReportKill();
        }
        else
        {
            Debug.LogWarning("[TEST SCRIPT] Kill button pressed, but no active bots were found to kill.");
        }
    }

    /// <summary>
    /// Ép GameManager kết thúc round hiện tại.
    /// </summary>
    public void Test_ForceNextRound()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.InRound)
        {
            Debug.Log("<color=green>[TEST SCRIPT]</color> Forcing current round to end.");
            GameManager.Instance.EndCurrentRound();
        }
        else
        {
            Debug.LogWarning("[TEST SCRIPT] Cannot force next round. GameManager is either missing or not currently in a round.");
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(SimpleKillTest))]
    public class SimpleKillTestEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            SimpleKillTest myScript = (SimpleKillTest)target;
            if (GUILayout.Button("Kill First Bot"))
            {
                myScript.Test_KillFirstBot();
            }
            if (GUILayout.Button("Force Next Round"))
            {
                myScript.Test_ForceNextRound();
            }
        }
    }
#endif
}