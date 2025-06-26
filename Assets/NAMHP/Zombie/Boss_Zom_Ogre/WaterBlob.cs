using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaterBlob : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
    [SerializeField] private float _blobSpeed1 = 7;
    [SerializeField] private float _blobSpeed2 = 7;
    [SerializeField] private float _rotaSpeed = 100;
    
    private void OnValidate()
    {
        _skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
    }

    private void OnDrawGizmos()
    {
        Update();
    }

    void Update()
    {
        _skinnedMeshRenderer.SetBlendShapeWeight(0, Mathf.Sin(Time.time * _blobSpeed1) * 50 + 50);
        _skinnedMeshRenderer.SetBlendShapeWeight(1, Mathf.Sin(Time.time * _blobSpeed2) * 50 + 50);
        
        transform.Rotate(new Vector3(Time.time,-Time.time,Time.time).normalized, _rotaSpeed * Time.deltaTime);
    }
}
