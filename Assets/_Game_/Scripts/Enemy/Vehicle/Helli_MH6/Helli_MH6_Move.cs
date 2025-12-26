using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class Helli_MH6_Move : StateBase
{
    [SerializeField] private BotIdentity botIdentity; // Tham chiếu đến BotIdentity để lấy thông tin về đường đi
    [FormerlySerializedAs("moveSpeed")] [SerializeField] float durationMoveToDropTroops = 3f; //duration di chuyển của máy bay
    [SerializeField] float speedRot = 3f; //tốc độ xoay của máy bay
    [SerializeField] private int indexPointStartDownDrop = 3; //điểm bắt đầu xuống điểm đỗ (index 3)
    [SerializeField] private int indexPointDropTroops = 5; //điểm đậu máy bay thả lính (index 5)
    [SerializeField] private int indexPointStartMoveEnd; //điểm lên cao để bắt đầu di chuyển về end point
    
    [Header("Speed Control Settings")]
    [SerializeField] private float slowDownDuration = 2f; // Thời gian giảm tốc từ index 3->5
    [SerializeField] private float speedUpDuration = 2f; // Thời gian tăng tốc khi resume
    [SerializeField] private float slowSpeedMultiplier = 0.1f; // Tốc độ chậm (10% tốc độ gốc)
    
    private Vector3[] pathPoints;
    public Tween currentPathTween;
    public Tween speedControlTween;
    int indexCurrent; //điểm hiện tại đang di chuyển đến
    Vector3 pointLookat;
    private bool hasDropped = false; // Flag để biết đã thả lính chưa
    
    public void OnInitState()
    {
        indexCurrent = 0;
        hasDropped = false;
    }
    
    public override void EnterState()
    {
        StartCoroutine(IEMove());
    }

    private IEnumerator IEMove()
    {
        yield return HelperCoroutine.GetWait(.5f);
        if (!botContext.stateController.canDead)
            yield break;
        if (indexCurrent == 0 && !hasDropped)
        {
            // Lần đầu tiên - di chuyển tới điểm thả lính
            currentPathTween = TF.DOPath(pathPoints, durationMoveToDropTroops, pathType: PathType.CatmullRom)
                .SetEase(Ease.Linear)
                .OnWaypointChange(OnWaypointReached)
                .OnComplete(OnPathComplete)
                .OnUpdate(UpdateRotationAndTilt);
        }
        else if (hasDropped)
        {
            // Sau khi thả lính - tiếp tục path từ điểm hiện tại
            ResumePathAfterDrop();
        }
    }

    private void OnPathComplete()
    { 
        if (!hasDropped)
        {
//            Debug.Log("Đã hoàn thành path tới điểm thả lính");
        }
        else
        {
//            Debug.Log("Đã hoàn thành toàn bộ path");
            botContext.botNetwork.OnDespawn(0f);
        }
    }

    private void OnWaypointReached(int waypointIndex)
    {
//        Debug.Log($"Reached waypoint {waypointIndex}");
        indexCurrent = waypointIndex;

        // Bắt đầu giảm tốc tại index 3
        if (waypointIndex == indexPointStartDownDrop && !hasDropped)
        {
//            Debug.Log("Index 3: Bắt đầu giảm tốc độ");
            StartSlowDown();
        }
        
        // Dừng hẳn tại index 5 và chuyển state
        if (waypointIndex == indexPointDropTroops && !hasDropped)
        {
//            Debug.Log("Index 5: Dừng hẳn và chuyển sang DropTroops state");
            PauseMovement();
            hasDropped = true;
            botContext.stateController.ChangeState(GameConstants.EnemyState.DropTroops);
        }
    }

    /// <summary>
    /// Giảm tốc độ dần dần từ index 3->5
    /// </summary>
    private void StartSlowDown()
    {
        // Kill tween tốc độ trước đó nếu có
        if (speedControlTween != null)
        {
            speedControlTween.Kill();
        }
        
        // Giảm timeScale từ 1.0 xuống slowSpeedMultiplier
        speedControlTween = DOTween.To(() => currentPathTween.timeScale, 
                                      x => currentPathTween.timeScale = x, 
                                      slowSpeedMultiplier, 
                                      slowDownDuration)
            .SetEase(Ease.OutQuad) // Giảm tốc mượt mà
            .OnComplete(() => {
                Debug.Log($"Đã giảm tốc xuống {slowSpeedMultiplier * 100}%");
            });
    }

    /// <summary>
    /// Dừng movement tại điểm thả lính
    /// </summary>
    private void PauseMovement()
    {
        if (currentPathTween != null)
        {
            currentPathTween.Pause();
        }
        
        // Kill speed control tween nếu đang chạy
        if (speedControlTween != null)
        {
            speedControlTween.Kill();
        }
    }

    /// <summary>
    /// Resume movement sau khi thả lính xong (gọi từ bên ngoài)
    /// </summary>
    public void ResumeMovement()
    {
        Debug.Log("Resume movement sau khi thả lính");
        
        if (currentPathTween != null && !currentPathTween.IsPlaying())
        {
            currentPathTween.Play();
            StartSpeedUpFromZero();
        }
        else
        {
            // Nếu không có tween nào, tạo path mới cho phần còn lại
            ResumePathAfterDrop();
        }
    }

    /// <summary>
    /// Tạo path mới cho phần còn lại sau khi thả lính
    /// </summary>
    private void ResumePathAfterDrop()
    {
        if (indexCurrent >= pathPoints.Length - 1)
        {
            OnPathComplete();
            return;
        }

        // Tạo path từ vị trí hiện tại đến các điểm còn lại
        List<Vector3> newPath = new List<Vector3>();

        // Bắt đầu từ vị trí hiện tại để nối tiếp mượt mà
        newPath.Add(TF.position);

        // Thêm các điểm còn lại trong path
        for (int i = indexCurrent + 1; i < pathPoints.Length; i++)
        {
            newPath.Add(pathPoints[i]);
        }

        float remainingDuration = durationMoveToDropTroops * ((float)newPath.Count / pathPoints.Length);

        currentPathTween = TF.DOPath(newPath.ToArray(), remainingDuration, pathType: PathType.CatmullRom)
            .SetEase(Ease.Linear)
            .OnWaypointChange((waypointIndex) =>
            {
                indexCurrent++; // Cập nhật từng điểm tiếp theo
//                Debug.Log($"Continuing to waypoint {indexCurrent}");
            })
            .OnComplete(OnPathComplete)
            .OnUpdate(UpdateRotationAndTilt);

        StartSpeedUpFromZero();
    }


    /// <summary>
    /// Tăng tốc độ dần dần từ 0 lên max
    /// </summary>
    private void StartSpeedUpFromZero()
    {
        // Kill tween tốc độ trước đó nếu có
        if (speedControlTween != null)
        {
            speedControlTween.Kill();
        }
        
        // Bắt đầu từ tốc độ 0
        if (currentPathTween != null)
        {
            currentPathTween.timeScale = 0f;
        }
        
        // Tăng timeScale từ 0 lên 1.0
        speedControlTween = DOTween.To(() => currentPathTween.timeScale, 
                                      x => currentPathTween.timeScale = x, 
                                      1f, 
                                      speedUpDuration)
            .SetEase(Ease.InQuad) // Tăng tốc mượt mà
            .OnComplete(() => {
//                Debug.Log("Đã tăng tốc lên 100%");
            });
    }

    /// <summary>
    /// Cập nhật rotation và tilt trong quá trình di chuyển
    /// </summary>
    private void UpdateRotationAndTilt()
    {
        // Lấy hướng di chuyển hiện tại
        Vector3 velocity = currentPathTween != null ? GetCurrentDirection() : Vector3.zero;
        if (velocity != Vector3.zero)
        {
            Vector3 lookTarget = TF.position + velocity;
            SmoothRotaWithTilt(TF, lookTarget, speedRot * 50f, 15f, 5f);
        }
    }

    /// <summary>
    /// Lấy hướng di chuyển hiện tại từ path
    /// </summary>
    private Vector3 GetCurrentDirection()
    {
        if (currentPathTween == null || pathPoints.Length <= 1)
            return Vector3.zero;

        // Ước tính hướng di chuyển dựa trên progress của path
        float progress = currentPathTween.ElapsedPercentage();
        int currentIndex = Mathf.FloorToInt(progress * (pathPoints.Length - 1));
        int nextIndex = Mathf.Min(currentIndex + 1, pathPoints.Length - 1);

        if (currentIndex < pathPoints.Length && nextIndex < pathPoints.Length)
        {
            return (pathPoints[nextIndex] - pathPoints[currentIndex]).normalized;
        }

        return Vector3.zero;
    }

    public override void UpdateState()
    {
        // Logic cũ không cần thiết nữa vì đã dùng DOPath
        // Giữ lại method này để tương thích với StateBase
    }
    
    private static float _currentTiltX = 0f;

    /// <summary>
    /// Tự xoay về targetPos và nghiêng thân khi xoay trái/phải
    /// </summary>
    /// <param name="body">Transform cần xoay và nghiêng</param>
    /// <param name="targetPos">Vị trí muốn hướng tới</param>
    /// <param name="rotationSpeed">Tốc độ xoay (deg/s)</param>
    /// <param name="maxTiltX">Độ nghiêng tối đa khi rẽ (góc X)</param>
    /// <param name="tiltSmooth">Độ mượt nghiêng</param>
    public static void SmoothRotaWithTilt(Transform body, Vector3 targetPos, float rotationSpeed = 180f, float maxTiltX = 15f, float tiltSmooth = 5f)
    {
        Vector3 dir = targetPos - body.position;
        dir.y = 0f; // chỉ quay theo phương ngang

        if (dir == Vector3.zero)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        body.rotation = Quaternion.RotateTowards(body.rotation, targetRot, rotationSpeed * Time.deltaTime);

        // Tính góc rẽ (góc chênh Y giữa hiện tại và mục tiêu)
        float angleDeltaY = Mathf.DeltaAngle(body.eulerAngles.y, targetRot.eulerAngles.y);

        // Tính nghiêng X theo độ rẽ, giới hạn từ -1 đến 1
        float normalizedTurn = Mathf.Clamp(angleDeltaY / 45f, -1f, 1f);
        float targetTiltX = normalizedTurn * maxTiltX;

        // Làm mượt nghiêng
        _currentTiltX = Mathf.Lerp(_currentTiltX, targetTiltX, Time.deltaTime * tiltSmooth);

        // Áp dụng xoay X nghiêng, giữ nguyên Y, Z
        Vector3 eul = body.localEulerAngles;
        body.localRotation = Quaternion.Euler(_currentTiltX, eul.y, eul.z);
    }
    
    public override void ExitState()
    {
        StopAllMovement();
    }

    private void OnDisable()
    {
        StopAllMovement();
    }

    /// <summary>
    /// Dừng tất cả movement và speed control
    /// </summary>
    private void StopAllMovement()
    {
        if (currentPathTween != null)
        {
            currentPathTween.Kill();
            currentPathTween = null;
        }
        
        if (speedControlTween != null)
        {
            speedControlTween.Kill();
            speedControlTween = null;
        }
        
        DOTween.Kill(TF);
    }

    public void GetPoint()
    {
        indexCurrent = 0;
        hasDropped = false;
        
        if (botIdentity?.AssignedPath?.points != null && botIdentity.AssignedPath.points.Count > 0)
        {
            pointLookat = botIdentity.AssignedPath.points[indexCurrent].position;
            pointLookat.y = TF.position.y;
            TF.LookAt(pointLookat);
            
            pathPoints = new Vector3[botIdentity.AssignedPath.points.Count];
            for (int i = 0; i < botIdentity.AssignedPath.points.Count; i++)
                pathPoints[i] = botIdentity.AssignedPath.points[i].position;
        }
    }

    // Thêm method để kiểm tra trạng thái
    public bool IsMoving()
    {
        return currentPathTween != null && currentPathTween.IsPlaying() && currentPathTween.timeScale > 0;
    }

    public bool HasDroppedTroops()
    {
        return hasDropped;
    }
}