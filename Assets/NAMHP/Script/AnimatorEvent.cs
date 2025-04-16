using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorEvent : MonoBehaviour
{
    // Setup thay đạn cho bazooka, lính MG thì k cần setup những giá trị bên dưới
    [SerializeField] GameObject WarheadOnWeapon;
    [SerializeField] GameObject WarheadOnHand;
    [SerializeField] Transform weaponRota;          // cho xoay bazooka khi chạy anim reload bazooka
    [SerializeField] Vector3 weaponLocalRotation;
    [SerializeField] float zValueStandReload;
    // Start is called before the first frame update
    private void OnEnable()
    {
        ResetValue();
    }
    private void OnDisable()
    {
        ResetValue();
    }
   public void SetRocketOnWeapon()
    {
        if (WarheadOnWeapon != null) 
            WarheadOnWeapon.SetActive(true);
    }
    public void SetRocketOnHand()
    {
        if (WarheadOnHand != null)
            WarheadOnHand.SetActive(true);
    }
    // Gán hàm này vào Animation Event
    public void PlayStandRotaWeapon()
    {
        StartCoroutine(StandRotaWeapon());
    }

    public IEnumerator StandRotaWeapon()
    {
        if (weaponRota == null) yield break;

        // Giai đoạn 1: Xoay sang góc reload
        Quaternion startRot = weaponRota.localRotation;
        Quaternion targetRot = Quaternion.Euler(weaponRota.localEulerAngles.x, weaponRota.localEulerAngles.y, zValueStandReload);
        float duration1 = 0.5f;
        float elapsed1 = 0f;

        while (elapsed1 < duration1)
        {
            elapsed1 += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed1 / duration1);
            weaponRota.localRotation = Quaternion.Lerp(startRot, targetRot, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.3f); // Tạm dừng một chút giống như LeanTween làm

        // Giai đoạn 2: Xoay về lại vị trí ban đầu
        Quaternion endRot = Quaternion.Euler(weaponLocalRotation);
        float duration2 = 0.4f;
        float elapsed2 = 0f;
        Quaternion currentRot = weaponRota.localRotation;

        while (elapsed2 < duration2)
        {
            elapsed2 += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed2 / duration2);
            weaponRota.localRotation = Quaternion.Lerp(currentRot, endRot, t);
            yield return null;
        }
    }
    public void ResetValue()
    {
        if (WarheadOnHand != null) WarheadOnHand.SetActive(false);
        if (WarheadOnWeapon != null) WarheadOnWeapon.SetActive(true);
    }
}
