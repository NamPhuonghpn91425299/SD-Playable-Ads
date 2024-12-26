using UnityEngine;

[CreateAssetMenu(menuName = "Attributes/Fog Setting")]
public class FogSetting : ScriptableObject
{
    public bool Enable;
    public Color Color;
    public FogMode Mode;
    public float Density;
    public float Start;
    public float End;
    public bool RealtimeEdit;

    public void CopyFromActiveScene()
    {
        Enable = RenderSettings.fog;
        Color = RenderSettings.fogColor;
        Mode = RenderSettings.fogMode;
        Density = RenderSettings.fogDensity;
        Start = RenderSettings.fogStartDistance;
        End = RenderSettings.fogEndDistance;
    }

    private FogSetting()
    {
        Enable = true;
        Color = Color.white;
        Mode = FogMode.Linear;
        Density = 0.01f;
        Start = 2;
        End = 300;
    }

    private void OnValidate()
    {
        if (RealtimeEdit)
            Apply();
    }

    public void Apply()
    {
        RenderSettings.fog = Enable;
        RenderSettings.fogColor = Color;
        RenderSettings.fogMode = Mode;
        RenderSettings.fogDensity = Density;
        RenderSettings.fogStartDistance = Start;
        RenderSettings.fogEndDistance = End;
    }
}