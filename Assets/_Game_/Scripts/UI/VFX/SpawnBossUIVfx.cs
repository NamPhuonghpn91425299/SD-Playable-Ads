using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpawnBossUIVfx : VFXBase
{
    [SerializeField] private Image _image_bossSpawn;
    private Coroutine _currentCoroutine;

    public override void Play<T>(T parameter)
    {
        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
        }
        
        float duration = parameter is float dur ? dur : 1f;
        _currentCoroutine = StartCoroutine(IEPlayBossSpawnVFXMultiple(duration, 3));
    }

    private IEnumerator IEPlayBossSpawnVFXMultiple(float duration, int repeatCount)
    {
        for (int i = 0; i < repeatCount; i++)
        {
            yield return StartCoroutine(IEPlayBossSpawnVFX(duration));
            
            // Optional: Add delay between repetitions
            if (i < repeatCount - 1) // Don't wait after the last iteration
            {
                yield return HelperCoroutine.GetWait(0.2f); // 0.2 second delay between repeats
            }
        }
        
        _currentCoroutine = null;
    }

    private IEnumerator IEPlayBossSpawnVFX(float duration)
    {
        Color color = _image_bossSpawn.color;
        color.a = 1f;
        _image_bossSpawn.color = color;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsed / duration);
            _image_bossSpawn.color = color;
            yield return null;
        }
        
        // Ensure alpha is exactly 0 at the end
        color.a = 0f;
        _image_bossSpawn.color = color;
    }
}

