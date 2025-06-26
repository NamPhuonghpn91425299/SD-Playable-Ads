using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseDetector : MonoBehaviour
{
    
    private static readonly Dictionary<string, BaseDetector> _detectors = new Dictionary<string, BaseDetector>();
    public static IReadOnlyDictionary<string, BaseDetector> Detectors => _detectors;
    public string DetectorName { get; private set; }

    protected virtual void Awake()
    {
        DetectorName = gameObject.name;
    }

    protected virtual void OnEnable()
    {
        if(!string.IsNullOrEmpty(DetectorName) && !_detectors.ContainsKey(DetectorName))
        {
            _detectors[DetectorName] = this;
        }
        else
        {
            Debug.LogWarning($"Detector with name {DetectorName} already exists or is empty.");
        }
        
    }

    protected virtual void OnDisable()
    {
        if (!string.IsNullOrEmpty(DetectorName) && _detectors.ContainsKey(DetectorName))
        {
            _detectors.Remove(DetectorName);
        }
        else
        {
            Debug.LogWarning($"Detector with name {DetectorName} does not exist or is empty.");
        }
    }
    
    public static void HandleWeaknessDamageStatic(string targetWeaknessName, int damage)
    {
        // Tìm detector trong dictionary
        if (_detectors.TryGetValue(targetWeaknessName, out var detector))
        {
            detector.ApplyDamage(damage);
            Debug.Log($"Weakness {targetWeaknessName} took damage: {damage}");
        }
    }

    protected abstract void ApplyDamage(int damage);
    
}
