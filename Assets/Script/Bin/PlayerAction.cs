using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAction : MonoBehaviour
{
    [SerializeField] private PlayerGunSelector gunSelector;
    public float shootCooldown = 0.5f; // Thời gian giữa các lần bắn (đơn vị giây)
    private float lastShootTime = 0f;  // Thời điểm cuối cùng bắn

    void Update()
    {
        // Kiểm tra nếu bấm chuột trái và có súng được chọn
        if (Input.GetMouseButton(0) && gunSelector.activeGun != null)
        {
            // Kiểm tra cooldown
            if (Time.time >= lastShootTime + shootCooldown)
            {
                // Bắn và cập nhật thời gian bắn cuối cùng
                gunSelector.activeGun.Shoot();
                lastShootTime = Time.time;
            }
        }
    }


}
