using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class WoodChipMorph : MonoBehaviour
{
    [Header("Liquid Appearance")]
    public Color targetLiquidColor = new Color(0.2f, 0.6f, 1f, 1f);
    public GameObject liquidPrefab;
    public bool spawnLiquidWhenDone = false;

    [Header("Morph Settings")]
    public bool useGlobalTransition = true;
    [Range(0f, 1f)] public float manualProgress = 0f;
    public bool destroyWhenDone = true;
    public float doneThreshold = 0.995f;

    [Header("Dissolve / Fade")]
    public float colorBlendPower = 1f;
    public float dissolveStart = 0.0f;
    public float dissolveEnd = 1.0f;

    int colorID = -1;
    int alphaID = -1;
    int dissolveID = -1;

    Renderer rend;
    Material matInstance;
    Color originalColor;
    bool transformed = false;

    static readonly string[] possibleDissolveProps = { "_Dissolve", "_DissolveAmount", "_Alpha", "_Cutoff", "_Opacity" };
    static readonly string[] possibleColorProps = { "_BaseColor", "_Color", "_TintColor", "_MainColor" };

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend == null) return;

        matInstance = rend.material;
        if (matInstance == null) return;

        originalColor = matInstance.HasProperty("_Color") ? matInstance.color : matInstance.GetColorIfExists(possibleColorProps, Color.white);

        colorID = matInstance.FindFirstPropertyID(possibleColorProps);

        dissolveID = matInstance.FindFirstPropertyID(possibleDissolveProps);
        alphaID = matInstance.HasProperty("_Color") ? Shader.PropertyToID("_Color") : -1;
    }

    void OnEnable()
    {
        if (useGlobalTransition)
            TransitionEvents.OnTransitionProgressChanged += OnGlobalProgress;
    }

    void OnDisable()
    {
        if (useGlobalTransition)
            TransitionEvents.OnTransitionProgressChanged -= OnGlobalProgress;
    }

    void OnGlobalProgress(float p)
    {
        ApplyProgress(Mathf.Clamp01(p));
    }

    void Update()
    {
        if (!useGlobalTransition)
        {
            ApplyProgress(Mathf.Clamp01(manualProgress));
        }
    }

    void ApplyProgress(float progress)
    {
        if (matInstance == null || transformed) return;

        float t = Mathf.Pow(progress, colorBlendPower);
        Color blended = Color.Lerp(originalColor, targetLiquidColor, t);
        if (colorID != -1)
            matInstance.SetColor(colorID, blended);
        else if (matInstance.HasProperty("_Color"))
            matInstance.SetColor("_Color", blended);

        if (dissolveID != -1)
        {
            float d = Mathf.InverseLerp(dissolveStart, dissolveEnd, progress);
            matInstance.SetFloat(dissolveID, d);
        }
        else if (matInstance.HasProperty("_Color"))
        {
            Color c = blended;
            float d = Mathf.InverseLerp(dissolveStart, dissolveEnd, progress);
            c.a = Mathf.Lerp(originalColor.a, 0f, d);
            matInstance.SetColor(colorID != -1 ? colorID : Shader.PropertyToID("_Color"), c);
        }

        if (progress >= doneThreshold)
        {
            transformed = true;
            OnFullyTransformed();
        }
    }

    void OnFullyTransformed()
    {
        if (spawnLiquidWhenDone && liquidPrefab != null)
        {
            Instantiate(liquidPrefab, transform.position, Quaternion.identity, null);
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);

        Collider col = GetComponent<Collider>();
        if (col != null) Destroy(col);

        if (destroyWhenDone)
        {
            Destroy(gameObject);
        }
        else
        {
            if (rend != null) rend.enabled = false;
        }
    }
}
