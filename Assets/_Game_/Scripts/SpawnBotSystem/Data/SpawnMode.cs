using System;

/// <summary>
/// Enum định nghĩa các chế độ spawn đường đi khác nhau
/// </summary>
[Serializable]
public enum SpawnMode
{
    /// <summary>
    /// Spawn ngẫu nhiên (chế độ mặc định cũ)
    /// </summary>
    Random = 0,
    
    /// <summary>
    /// Spawn tuần tự từ index 0 đến cuối
    /// </summary>
    Sequential = 1,
    
    /// <summary>
    /// Spawn ngược từ index cuối về 0
    /// </summary>
    Reverse = 2,
    
    /// <summary>
    /// Spawn tuần tự từ 0 đến cuối, sau đó ngược lại từ cuối về 0 (ping-pong)
    /// Ví dụ: 0->1->2->3->2->1->0->1->2...
    /// </summary>
    PingPong = 3,
    
    /// <summary>
    /// Spawn tuần tự và lặp lại từ đầu khi hết (cycle)
    /// Ví dụ: 0->1->2->3->0->1->2->3...
    /// </summary>
    Cycle = 4
}
