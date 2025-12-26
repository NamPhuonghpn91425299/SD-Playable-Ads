using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// GẮN VÀO CÁC BOT "MẸ" ĐỂ CHO CHÚNG KHẢ NĂNG SPAWN RA BOT "CON" (MINION).
/// Đã được thiết kế lại để tương thích hoàn toàn với Object Pooling.
/// </summary>
public class MinionSpawner : MonoBehaviour
{
    [Header("Minion Spawning Contract")]
    [Tooltip("Danh sách các đợt minion mà unit này sẽ spawn.")]
    public List<BotWave> MinionWaveContract;

    private bool isSpawningActivated = false;
    private int minionsPhysicallySpawned = 0;

    public int TotalMinionCount => MinionWaveContract?.Sum(wave => wave.Quantity) ?? 0;
    public int RemainingMinionCount => TotalMinionCount - minionsPhysicallySpawned;
    
    private void OnEnable()
    {
        // Reset lại toàn bộ trạng thái của spawner mỗi khi nó được "tái sinh".
        ResetSpawner();
        ActivateMinionSpawning();
    }
    /// <summary>
    /// Đưa Spawner trở lại trạng thái ban đầu, sẵn sàng cho một vòng đời mới.
    /// Bất kỳ hệ thống pooling nào cũng nên gọi hàm này khi tái sử dụng đối tượng.
    /// </summary>
    public void ResetSpawner()
    {
        StopAllSpawningCoroutines();
        isSpawningActivated = false;
        minionsPhysicallySpawned = 0;
    }
    /// <summary>
    /// Dừng tất cả các coroutine spawn của MinionSpawner này.
    /// </summary>
    public void StopAllSpawningCoroutines()
    {
        StopAllCoroutines();
    }
    /// <summary>
    /// Bắt đầu thực thi hợp đồng spawn minion.
    /// </summary>
    public void ActivateMinionSpawning()
    {
        if (isSpawningActivated) return;
        isSpawningActivated = true;
        StartCoroutine(ProcessMinionSpawningContract());
    }
    
    private IEnumerator ProcessMinionSpawningContract()
    {

        foreach (var minionWave in MinionWaveContract)
        {
            if (minionWave.Conditions != null && minionWave.Conditions.Count > 0)
            {
                List<ISpawnCondition> runtimeConditions = minionWave.Conditions.Select(condDef => condDef.CreateRuntimeCondition()).ToList();
                foreach(var cond in runtimeConditions) cond.Reset();
                while (runtimeConditions.Any(cond => !cond.IsMet()))
                {
                    yield return null;
                }
                foreach(var cond in runtimeConditions) cond.Terminate();
            }
            BotDefinition definition = BotSpawnManager.Instance.GetDefinitionForType(minionWave.BotToSpawn);
            if (definition == null)
            {
                Debug.LogError($"Không tìm thấy Definition cho BotType '{minionWave.BotToSpawn}', không thể spawn minion.");
                continue; // Bỏ qua bước này nếu không có definition
            }
            var singleMinionOrder = BotSpawnOrder.Get();
            singleMinionOrder.BotTypeToSpawn = minionWave.BotToSpawn;
            singleMinionOrder.BotMoveType = definition.BotMoveType;
            singleMinionOrder.IsFromRoundScript = false;
            
            WaitForSeconds delayWait = minionWave.DelayBetweenSpawns > 0 ? HelperCoroutine.GetWait(minionWave.DelayBetweenSpawns) : null;
            
            for (int i = 0; i < minionWave.Quantity; i++)
            {
                BotSpawnManager.Instance.ExecuteSpawnOrder(singleMinionOrder);
                minionsPhysicallySpawned++;
                
                if (i < minionWave.Quantity - 1 && delayWait != null)
                {
                    yield return delayWait;
                }
            }
            
            BotSpawnOrder.Return(singleMinionOrder);
        }
    }

    #region Editor-Only
#if UNITY_EDITOR
    [CustomEditor(typeof(MinionSpawner))]
    public class MinionSpawnerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            MinionSpawner spawner = (MinionSpawner)target;
            GUILayout.Space(10);
            if (GUILayout.Button("▶ Kích hoạt Spawn Minion (Test)"))
            {
                if (Application.isPlaying)
                {
                    spawner.ActivateMinionSpawning();
                }
                else
                {
                    Debug.LogWarning("Nút test chỉ hoạt động khi game đang ở chế độ Play.");
                }
            }
        }
    }
#endif
    #endregion
}