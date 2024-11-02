using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageHit 
{
    /// <summary>
    /// Hàm kế thừa nhận damage xử lý mất máu
    /// </summary>
    /// <param name="damage"></param>
    void OnHit(int damage);
    
}
