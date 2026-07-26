using UnityEngine;

public static class MaterialExtensions
{
    public static int FindFirstPropertyID(this Material m, string[] propNames)
    {
        foreach (string p in propNames)
            if (m.HasProperty(p))
                return Shader.PropertyToID(p);
        return -1;
    }

    public static Color GetColorIfExists(this Material m, string[] propNames, Color defaultColor)
    {
        foreach (string p in propNames)
            if (m.HasProperty(p))
                return m.GetColor(p);
        return defaultColor;
    }
}
