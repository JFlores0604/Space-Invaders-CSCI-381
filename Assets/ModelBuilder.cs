using UnityEngine;

// Small helper for building models out of Unity primitives at runtime.
// Used by AlienModel and JetModel so no external art assets are needed.
public static class ModelBuilder
{
    // Copies an existing material (so it uses the project's URP shader) and recolors it.
    public static Material MakeMaterial(Material template, Color color, float glow = 0f, float smoothness = 0.5f)
    {
        Material m = new Material(template);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
        if (m.HasProperty("_Color")) m.SetColor("_Color", color);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
        if (glow > 0f && m.HasProperty("_EmissionColor"))
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", color * glow);
        }
        return m;
    }

    // Adds one primitive shape as a child. Its collider is removed so only the
    // original gameplay colliders take part in physics.
    public static Transform Part(PrimitiveType type, Transform parent, Material mat,
                                 Vector3 position, Vector3 scale, float zAngle = 0f, float xAngle = 0f)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        // Disable first (takes effect immediately, even inside a collision callback), then remove.
        Collider collider = go.GetComponent<Collider>();
        collider.enabled = false;
        Object.Destroy(collider);
        Transform t = go.transform;
        t.SetParent(parent, false);
        t.localPosition = position;
        t.localScale = scale;
        t.localRotation = Quaternion.Euler(xAngle, 0f, zAngle);
        Renderer r = go.GetComponent<Renderer>();
        r.sharedMaterial = mat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return t;
    }

    public static Transform Group(string name, Transform parent, Vector3 position)
    {
        Transform t = new GameObject(name).transform;
        t.SetParent(parent, false);
        t.localPosition = position;
        return t;
    }

    // Removes the placeholder mesh but keeps the collider.
    public static void RemoveMesh(GameObject go)
    {
        Object.DestroyImmediate(go.GetComponent<MeshRenderer>());
        Object.DestroyImmediate(go.GetComponent<MeshFilter>());
    }
}
