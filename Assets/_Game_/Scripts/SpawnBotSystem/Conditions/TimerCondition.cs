
using UnityEngine;

public class TimerCondition : ISpawnCondition
{
    private float timer;
    private float targetTime;
    
    private bool isMet = false;
    
    public TimerCondition(float time) 
    { 
        targetTime = time; 
    }
    
    /// <summary>
    /// Hàm mới để tái khởi tạo điều kiện khi được lấy từ pool.
    /// </summary>
    public void Reinitialize(float newTime)
    {
        this.targetTime = newTime;
        // Gọi lại Reset() để đảm bảo các biến trạng thái được đặt lại.
        Reset();
    }
    public bool IsMet() 
    { 
        if (!isMet)
        {
            timer += Time.deltaTime;
            isMet = timer >= targetTime;
        }
        return isMet;
    }
    
    public void Reset() 
    { 
        timer = 0f;
        isMet = false;
    }
    
    public void Terminate() 
    {
        // No cleanup needed for timer
    }
}