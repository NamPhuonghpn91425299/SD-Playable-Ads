using UnityEngine;

public class MoveWave : MonoBehaviour
{
    [Header("Wave Renderer")]
    [SerializeField] Renderer wave;
    [Header("Scale Value")]
    [SerializeField] Vector2 tiling=new Vector2(20,20);
    [Header("Move Speed")]
    [SerializeField] float waveSpeed;
    public static readonly int MainTex = Shader.PropertyToID("_MainTex");
    
    private void OnEnable()
    {
        if (!wave)
            wave = GetComponent<Renderer>();
        
        if (!wave || !wave.material || !wave.material.HasProperty(MainTex))
        {
            enabled = false;
            return;
        }

        wave.material.SetTextureScale(MainTex, tiling);

    }
    void Update()
    {
        wave.material.SetTextureOffset(MainTex, new Vector2(0, Time.time * waveSpeed));
    }
}
