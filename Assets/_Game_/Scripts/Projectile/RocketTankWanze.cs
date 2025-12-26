using System.Collections;
using System.Collections.Generic;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;

public class RocketTankWanze : GameUnit<GameConstants.ProjectileEnemy>
{
    [Header("Damage")] 
    [SerializeField] private int damage;

    [Header("Reference")]
    Vector3 target;
    [SerializeField] private GameObject _body;
    [SerializeField] private float speed;
    [SerializeField] private float arcHeight;

    [Header("Rotation")]
    [Tooltip("Tốc độ lerp xoay để rocket mượt theo đường đi")]
    [SerializeField] private float rotationLerpSpeed = 10f;
    [Tooltip("Offset xoay nếu model không nhìn theo trục +Z")]
    [SerializeField] private Vector3 rotationOffsetEuler = Vector3.zero;
    
    [SerializeField] private ParticleSystem vfxExplosion;

    [Header("Audio")] [SerializeField] private AudioSource audioSource;
    public void OnInit(Vector3 _target)
    {
        if (GameController.Instance.CurrentGameState != GameConstants.GameState.InGame) 
            audioSource.enabled = false;
        else
            audioSource.enabled = true;
        // Đảm bảo body và vfx ở trạng thái khởi tạo đúng
        if (_body != null) _body.SetActive(true);
        if (vfxExplosion != null) vfxExplosion.Stop();
        
        target = _target;
        float duration = Vector3.Distance(TF.position, target) / speed;
        StartCoroutine(IEParabolMove(target,duration,arcHeight));
    }

    private IEnumerator IEParabolMove(Vector3 targetPosition, float duration, float height)
    {
        Vector3 startPoint = TF.position;
        float elapsed = 0f;
        Vector3 prevPos = startPoint;
        bool hasExploded = false;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            
            Vector3 linearPoint = Vector3.Lerp(startPoint, targetPosition, t);
            float yOffset = height * 4 * t * (1 - t);
            
            Vector3 newPos = new Vector3(linearPoint.x, linearPoint.y + yOffset, linearPoint.z);

            // Tính hướng bay theo tiếp tuyến quỹ đạo giữa 2 khung hình và xoay rocket theo hướng đó
            Vector3 delta = newPos - prevPos;
            if (delta.sqrMagnitude > 0.000001f)
            {
                Vector3 dir = delta.normalized;
                Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up) * Quaternion.Euler(rotationOffsetEuler);
                Transform rotTf = _body != null ? _body.transform : TF;
                rotTf.rotation = Quaternion.Slerp(rotTf.rotation, targetRot, rotationLerpSpeed * Time.deltaTime);
            }

            TF.position = newPos;
            prevPos = TF.position;

            elapsed += Time.deltaTime;
            
            // Kiểm tra nếu đã đến gần mục tiêu
            if (Vector3.Distance(TF.position, targetPosition) <= 0.5f)
            {
                hasExploded = true;
                break;
            }
            
            yield return null;
        }
        
        // Đảm bảo luôn nổ khi hết duration hoặc đến gần mục tiêu
        if (!hasExploded)
        {
            // Di chuyển đến điểm cuối cùng nếu chưa đến
            TF.position = targetPosition;
        }
        
        // Thực hiện vụ nổ
        if (_body != null) _body.SetActive(false);
        if (vfxExplosion != null) 
        {
            vfxExplosion.Play();
            yield return new WaitForSeconds(1f);
        }
        
        OnDespawn();
    }

    private void OnDespawn()
    {
        EventManager.Instance?.Publish(new PlayerHealthChangedEvent(damage: damage, state: "OnlyDamage"));
        EventManager.Instance?.Publish(new CamShakeEvent(new CamShakeData{duration = .3f,strength = .6f,vibrato = 15,randomness = 45}));
        SimplePool<GameConstants.ProjectileEnemy>.Despawn(this);
    }
}
