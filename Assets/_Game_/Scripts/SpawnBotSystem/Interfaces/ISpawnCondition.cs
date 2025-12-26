
using UnityEngine;

/// <summary>
/// Interface cho các điều kiện spawn.
/// </summary>
public interface ISpawnCondition
{
    bool IsMet();
    void Reset();
    void Terminate(); // Dọn dẹp listener sự kiện
}

