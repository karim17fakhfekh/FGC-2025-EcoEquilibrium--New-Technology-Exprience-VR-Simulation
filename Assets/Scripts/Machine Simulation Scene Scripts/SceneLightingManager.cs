using UnityEngine;

public class SceneLightingManager : MonoBehaviour
{
    [Header("Lighting Settings")]
    public Light directionalLight;
    public float lightIntensity = 1f;
    public Color lightColor = Color.white;
    public Vector3 lightRotation = new Vector3(50f, -30f, 0f);

    [Header("Ambient Settings")]
    public Color ambientSkyColor = new Color(0.2f, 0.2f, 0.2f);
    public Color ambientEquatorColor = new Color(0.5f, 0.5f, 0.5f);
    public Color ambientGroundColor = new Color(0.3f, 0.3f, 0.3f);
    public float ambientIntensity = 1f;

    [Header("Fog Settings (Optional)")]
    public bool enableFog = false;
    public Color fogColor = Color.gray;
    public float fogDensity = 0.01f;

    void Start()
    {
        SetupLighting();
    }

    void SetupLighting()
    {
        if (directionalLight == null)
        {
            directionalLight = FindObjectOfType<Light>();

            if (directionalLight == null || directionalLight.type != LightType.Directional)
            {
                GameObject lightGO = new GameObject("Directional Light");
                directionalLight = lightGO.AddComponent<Light>();
                directionalLight.type = LightType.Directional;
            }
        }

        directionalLight.transform.rotation = Quaternion.Euler(lightRotation);
        directionalLight.intensity = lightIntensity;
        directionalLight.color = lightColor;
        directionalLight.shadows = LightShadows.Soft;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = ambientSkyColor;
        RenderSettings.ambientEquatorColor = ambientEquatorColor;
        RenderSettings.ambientGroundColor = ambientGroundColor;
        RenderSettings.ambientIntensity = ambientIntensity;

        RenderSettings.fog = enableFog;
        if (enableFog)
        {
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
        }

        Debug.Log("Scene lighting has been configured");
    }

    [ContextMenu("Quick Bright Setup")]
    public void QuickBrightSetup()
    {
        lightIntensity = 1.2f;
        lightColor = Color.white;
        lightRotation = new Vector3(50f, -30f, 0f);
        ambientIntensity = 1f;
        ambientSkyColor = new Color(0.4f, 0.4f, 0.4f);
        SetupLighting();
    }

    [ContextMenu("Quick Sunset Setup")]
    public void QuickSunsetSetup()
    {
        lightIntensity = 0.8f;
        lightColor = new Color(1f, 0.8f, 0.6f);
        lightRotation = new Vector3(20f, -30f, 0f);
        ambientIntensity = 0.8f;
        ambientSkyColor = new Color(0.6f, 0.5f, 0.4f);
        SetupLighting();
    }
}