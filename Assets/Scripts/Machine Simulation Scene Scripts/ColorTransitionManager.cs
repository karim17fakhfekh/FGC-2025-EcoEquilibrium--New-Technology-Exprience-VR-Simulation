using UnityEngine;
using System;

public class ColorTransitionManager : MonoBehaviour
{
    [Header("Gradient Settings")]
    public Gradient colorGradient;
    public float transitionSpeed = 1.0f;

    [Header("Shader Graph Property")]
    [Tooltip("This must match the exposed Color property reference in your Shader Graph (e.g. _BaseColor).")]
    public string colorPropertyName = "_BaseColor";

    public static event Action<float> OnTransitionProgress;

    private Material material;
    private int colorPropertyID;
    private float time;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null || renderer.sharedMaterial == null)
        {
            Debug.LogError("No Renderer/Material found on object!");
            return;
        }

        material = renderer.material;

        colorPropertyID = Shader.PropertyToID(colorPropertyName);

        if (!material.HasProperty(colorPropertyID))
        {
            Debug.LogError(
                $"No suitable color property found on {material.shader.name}! " +
                $"Expected '{colorPropertyName}'. Make sure your Shader Graph has a Color property " +
                $"with that exact Reference name."
            );
        }
        enabled = false;
    }
    void Update()
    {
        if (material == null || !material.HasProperty(colorPropertyID)) return;

        time += Time.deltaTime * transitionSpeed;
        float t = Mathf.Clamp01(time);

        OnTransitionProgress?.Invoke(t);

        Color targetColor = colorGradient.Evaluate(t);
        material.SetColor(colorPropertyID, targetColor);
    }
    public void StartTransition()
    {
        time = 0f;
        enabled = true;
    }

    void OnDestroy()
    {
        if (material != null && Application.isPlaying)
        {
            Destroy(material);
        }
    }
}
