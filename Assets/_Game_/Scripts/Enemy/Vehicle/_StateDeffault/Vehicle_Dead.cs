using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Vehicle_Dead : StateBase
{
    [Header("Parts to Explode")]
    public GameObject _body;                // phần thân xe
    public GameObject parentExplosion;
    public Transform[] brokenParts;
    public ParticleSystem explodeParticles;
    [SerializeField]private Transform explosionCenterTransform = null;  // transform tâm vụ nổ (nếu có)
    
    [Header("Settings")]
    public float explosionForce = 10f;          // lực nổ ban đầu
    public float gravity = 30f;               // gia tốc trọng lực
    public float groundStopHeight = 0.05f;     // khoảng cách dừng khi gần mặt đất
    public LayerMask groundLayer;              // layer mặt đất
    public bool rotateWhenFlying = true;       // mảnh xoay khi bay
    

    private void Awake()
    {
        foreach (Transform part in brokenParts)
        {
            originalTransforms[part] = (part.localPosition, part.localRotation);
        }
    }

    public void OnInit()
    {
        // đã chuyển OnInit sang OnEnable
    }

    private void OnEnable()
    {
        parentExplosion.SetActive(false);
        _body.SetActive(true);
        ResetExplosionParts();
    }

    public override void EnterState()
    {    
        StopAllCoroutines();
        _body.SetActive(false);
        parentExplosion.SetActive(true);
        explodeParticles.Play();
        botContext.audioPlayable?.PlayAudio(GameConstants.AudioType.BotDeath);
        Explode(explosionCenterTransform);
        explosionCenterTransform = null; // Reset sau khi sử dụng
    }

    public override void UpdateState()
    {
        
    }

    public override void ExitState()
    {
        
    }
    public void Explode(Transform centerTransform = null)
    {
        List<Part> activeParts = new List<Part>();

        foreach (Transform part in brokenParts)
        {
            Vector3 dir;
            
            // Nếu có transform tâm thì vụ nổ tỏa theo tâm và rơi xuống
            if (centerTransform != null)
            {
                // Tính hướng từ tâm đến mảnh vỡ
                dir = (part.position - centerTransform.position).normalized;
                // Đảm bảo có thành phần rơi xuống mạnh hơn
                if (dir.y > 0)
                {
                    dir.y *= 0.3f; // Giảm mạnh thành phần bay lên
                }
                else
                {
                    dir.y -= 0.2f; // Thêm lực kéo xuống
                }
            }
            else
            {
                // Nếu không có thì vẫn nổ theo logic cũ
                dir = Random.onUnitSphere;
                dir.y = Mathf.Abs(dir.y); // chỉ bay lên hoặc ngang
            }

            float force = Random.Range(explosionForce * 0.6f, explosionForce * 1.2f);

            Part p = new Part
            {
                tf = part,
                velocity = dir * force,
                angularVelocity = rotateWhenFlying
                    ? new Vector3(
                        Random.Range(-90f, 90f),
                        Random.Range(-90f, 90f),
                        Random.Range(-90f, 90f)
                    )
                    : Vector3.zero
            };

            activeParts.Add(p);
        }

        StartCoroutine(SimulateParts(activeParts,5f));
        botContext.botNetwork.OnDespawn(5f);
    }


    IEnumerator SimulateParts(List<Part> parts, float timeStop)
    {
        float timer = 0f;
        while (parts.Count > 0)
        {
            timer += Time.deltaTime;
            if (timer >= timeStop)
                break; // dừng sau thời gian nhất định
            for (int i = parts.Count - 1; i >= 0; i--)
            {
                Part p = parts[i];
                Transform t = p.tf;

                // Gravity effect
                p.velocity += Vector3.down * gravity * Time.deltaTime;

                // Move part
                t.position += p.velocity * Time.deltaTime;

                // Optional: Rotate part
                if (rotateWhenFlying)
                    t.Rotate(p.angularVelocity * Time.deltaTime);

                // Ground check: raycast down or check height
                if (Physics.Raycast(t.position, Vector3.down, out RaycastHit hit, groundStopHeight, groundLayer))
                {
                    parts.RemoveAt(i);
                }
            }

            yield return null;
        }
    }
    
    private Dictionary<Transform, (Vector3 position, Quaternion rotation)> originalTransforms = new Dictionary<Transform, (Vector3, Quaternion)>();
    public void ResetExplosionParts()
    {
        parentExplosion.SetActive(false);
        _body.SetActive(true);

        foreach (Transform part in brokenParts)
        {
            if (originalTransforms.TryGetValue(part, out var original))
            {
                part.localPosition = original.position;
                part.localRotation = original.rotation;
            }
        }

        explodeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
    
    // Phương thức để đặt transform tâm vụ nổ trước khi chuyển state
    public void SetExplosionCenter(Transform centerTransform)
    {
        explosionCenterTransform = centerTransform;
    }
    
    // Phương thức để kích hoạt vụ nổ với transform tâm trực tiếp
    public void ExplodeFromCenter(Transform centerTransform)
    {
        explosionCenterTransform = centerTransform;
        // Kích hoạt enter state để xử lý vụ nổ
        EnterState();
    }
    
    // Phương thức để kích hoạt vụ nổ với vị trí Vector3 (để tương thích)
    public void ExplodeFromPosition(Vector3 position)
    {
        // Tạo một GameObject tạm thời làm tâm
        GameObject tempCenter = new GameObject("TempExplosionCenter");
        tempCenter.transform.position = position;
        explosionCenterTransform = tempCenter.transform;
        EnterState();
        // Xóa GameObject tạm sau khi dùng
        Destroy(tempCenter, 0.1f);
    }


}

public class Part
{
    public Transform tf;
    public Vector3 velocity;
    public Vector3 angularVelocity;
}
