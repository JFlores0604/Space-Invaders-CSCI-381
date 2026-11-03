using UnityEngine;

// Replaces the three capsules of the player ship with a fighter jet:
// pointed nose, cockpit canopy, swept wings, tail fins and a flickering engine flame.
public class JetModel : MonoBehaviour
{
    public float size = 0.85f;

    Transform flame;
    Vector3 flameScale;

    public static void Build(GameObject ship)
    {
        MeshRenderer[] placeholders = ship.GetComponentsInChildren<MeshRenderer>();
        if (placeholders.Length == 0) return;

        // Center the jet where the old capsules were, so the collision area still lines up.
        Bounds bounds = placeholders[0].bounds;
        foreach (MeshRenderer r in placeholders) bounds.Encapsulate(r.bounds);
        Vector3 center = ship.transform.InverseTransformPoint(bounds.center);

        Material template = placeholders[0].sharedMaterial;
        foreach (MeshRenderer r in placeholders) ModelBuilder.RemoveMesh(r.gameObject);   // colliders stay

        ship.AddComponent<JetModel>().Assemble(template, center);
    }

    void Assemble(Material template, Vector3 center)
    {
        Material hull = ModelBuilder.MakeMaterial(template, new Color(0.82f, 0.85f, 0.9f), 0f, 0.8f);
        Material dark = ModelBuilder.MakeMaterial(template, new Color(0.22f, 0.25f, 0.3f), 0f, 0.6f);
        Material red = ModelBuilder.MakeMaterial(template, new Color(0.9f, 0.12f, 0.12f), 0.3f);
        Material glass = ModelBuilder.MakeMaterial(template, new Color(0.3f, 0.75f, 1f), 0.6f, 0.95f);
        Material fire = ModelBuilder.MakeMaterial(template, new Color(1f, 0.5f, 0.1f), 3f);
        Material fireCore = ModelBuilder.MakeMaterial(template, new Color(1f, 0.95f, 0.6f), 3f);

        Transform jet = ModelBuilder.Group("Jet", transform, center);
        jet.localScale = Vector3.one * size;

        // Fuselage and nose (the jet points up the screen, +y).
        ModelBuilder.Part(PrimitiveType.Cylinder, jet, hull, new Vector3(0f, -0.05f, 0f), new Vector3(0.42f, 0.8f, 0.42f));
        ModelBuilder.Part(PrimitiveType.Sphere, jet, hull, new Vector3(0f, 0.75f, 0f), new Vector3(0.42f, 0.8f, 0.42f));
        ModelBuilder.Part(PrimitiveType.Sphere, jet, red, new Vector3(0f, 1.12f, 0f), new Vector3(0.12f, 0.2f, 0.12f));
        ModelBuilder.Part(PrimitiveType.Sphere, jet, glass, new Vector3(0f, 0.5f, -0.18f), new Vector3(0.26f, 0.5f, 0.2f));

        for (int side = -1; side <= 1; side += 2)
        {
            // Swept-back wing with a red tip and a missile pod underneath.
            ModelBuilder.Part(PrimitiveType.Cube, jet, hull, new Vector3(0.65f * side, -0.15f, 0f), new Vector3(1.3f, 0.42f, 0.08f), -22f * side);
            ModelBuilder.Part(PrimitiveType.Cube, jet, red, new Vector3(1.16f * side, -0.36f, -0.01f), new Vector3(0.25f, 0.44f, 0.1f), -22f * side);
            ModelBuilder.Part(PrimitiveType.Cylinder, jet, dark, new Vector3(0.75f * side, -0.38f, -0.07f), new Vector3(0.1f, 0.3f, 0.1f));

            // Tail fin.
            ModelBuilder.Part(PrimitiveType.Cube, jet, dark, new Vector3(0.32f * side, -0.78f, 0f), new Vector3(0.55f, 0.25f, 0.07f), -30f * side);
        }

        // Engine nozzle and flame.
        ModelBuilder.Part(PrimitiveType.Cylinder, jet, dark, new Vector3(0f, -0.9f, 0f), new Vector3(0.32f, 0.1f, 0.32f));
        flame = ModelBuilder.Group("Flame", jet, new Vector3(0f, -1.1f, 0f));
        ModelBuilder.Part(PrimitiveType.Sphere, flame, fire, new Vector3(0f, -0.05f, 0f), new Vector3(0.26f, 0.45f, 0.26f));
        ModelBuilder.Part(PrimitiveType.Sphere, flame, fireCore, new Vector3(0f, 0.02f, -0.05f), new Vector3(0.14f, 0.28f, 0.14f));
        flameScale = flame.localScale;
    }

    void Update()
    {
        // Flicker the engine flame (stops automatically while the game is paused).
        if (flame == null || Time.timeScale == 0f) return;
        float flicker = 0.85f + 0.3f * Mathf.PerlinNoise(Time.time * 15f, 0f);
        flame.localScale = new Vector3(flameScale.x, flameScale.y * flicker, flameScale.z);
    }
}
