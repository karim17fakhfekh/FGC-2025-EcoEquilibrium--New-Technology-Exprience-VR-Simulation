using UnityEngine;

public class MaterialPropertyController : MonoBehaviour
{
    [Header("Material Assignment")]
    public Material targetMaterial;

    [Header("Visual Settings")]
    public Color waterColor = new Color(0.2f, 0.6f, 1f, 0.8f);
    public float fillHeight = 0.5f;
    public float smoothness = 0.9f;
    public float metallic = 0.1f;

    [Header("Animation Settings")]
    public float animationSpeed = 1f;
    public bool animateFillHeight = false;
    public float minFillHeight = 0.1f;
    public float maxFillHeight = 0.9f;

    private Renderer objectRenderer;
    private Material materialInstance;

    void Start()
    {
        InitializeMaterial();
    }

    void InitializeMaterial()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            objectRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        if (targetMaterial != null)
        {
            materialInstance = new Material(targetMaterial);
            objectRenderer.material = materialInstance;
        }
        else
        {
            materialInstance = new Material(Shader.Find("Standard"));
            objectRenderer.material = materialInstance;
            Debug.Log("Created default material");
        }

        ApplyMaterialProperties();
    }

    void Update()
    {
        if (animateFillHeight)
        {
            fillHeight = Mathf.PingPong(Time.time * animationSpeed, maxFillHeight - minFillHeight) + minFillHeight;
            ApplyMaterialProperties();
        }
    }

    public void ApplyMaterialProperties()
    {
        if (materialInstance == null) return;

        SetMaterialProperty("_FillHeight", fillHeight);
        SetMaterialProperty("_WaterLevel", fillHeight);
        SetMaterialProperty("_FillLevel", fillHeight);
        SetMaterialProperty("_Level", fillHeight);
        SetMaterialProperty("_Height", fillHeight);

        SetMaterialProperty("_Color", waterColor);
        SetMaterialProperty("_BaseColor", waterColor);
        SetMaterialProperty("_MainColor", waterColor);
        SetMaterialProperty("_WaterColor", waterColor);

        SetMaterialProperty("_Smoothness", smoothness);
        SetMaterialProperty("_Metallic", metallic);
    }

    void SetMaterialProperty(string propertyName, float value)
    {
        if (materialInstance.HasProperty(propertyName))
        {
            materialInstance.SetFloat(propertyName, value);
        }
    }

    void SetMaterialProperty(string propertyName, Color value)
    {
        if (materialInstance.HasProperty(propertyName))
        {
            materialInstance.SetColor(propertyName, value);
        }
    }

    public void SetFillHeight(float height)
    {
        fillHeight = Mathf.Clamp01(height);
        ApplyMaterialProperties();
    }

    public void SetWaterColor(Color color)
    {
        waterColor = color;
        ApplyMaterialProperties();
    }

    [ContextMenu("List All Properties")]
    void ListAllProperties()
    {
        if (materialInstance == null) return;

        Debug.Log("=== MATERIAL PROPERTIES ===");
        for (int i = 0; i < materialInstance.shader.GetPropertyCount(); i++)
        {
            string propName = materialInstance.shader.GetPropertyName(i);
            string propType = materialInstance.shader.GetPropertyType(i).ToString();
            Debug.Log($"{propName} ({propType})");
        }
    }

    void OnDestroy()
    {
        if (materialInstance != null && Application.isPlaying)
        {
            Destroy(materialInstance);
        }
    }
}