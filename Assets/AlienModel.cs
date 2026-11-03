using UnityEngine;

// Replaces the plain green sphere with a little alien riding a flying saucer.
// Each row gets its own skin color, and the alien bobs and wobbles as it flies.
public class AlienModel : MonoBehaviour
{
    static readonly Color[] skinColors =
    {
        new Color(0.45f, 1f, 0.3f),    // green
        new Color(0.7f, 0.35f, 1f),    // purple
        new Color(0.2f, 0.9f, 1f),     // cyan
        new Color(1f, 0.4f, 0.75f),    // pink
    };

    // Shared by every alien so 40 aliens don't create 40 sets of materials.
    static Material[] skins;
    static Material eyeMat, shineMat, saucerMat, lightMat;

    public Color SkinColor { get; private set; }

    Transform body;
    float phase;

    public static AlienModel Build(GameObject alien)
    {
        MeshRenderer placeholder = alien.GetComponent<MeshRenderer>();
        if (placeholder == null) return alien.GetComponent<AlienModel>();   // already built

        // Materials made at runtime are destroyed when Play mode ends, so check for that too.
        if (skins == null || skins[0] == null) CreateMaterials(placeholder.sharedMaterial);
        ModelBuilder.RemoveMesh(alien);

        // Rows are spawned at y = 10, 8, 6, 4 (see AlienFleet), so pick a color per row.
        int row = Mathf.RoundToInt((10f - alien.transform.position.y) / 2f);
        row = Mathf.Clamp(row, 0, skins.Length - 1);

        AlienModel model = alien.AddComponent<AlienModel>();
        model.SkinColor = skinColors[row];
        model.Assemble(skins[row]);
        return model;
    }

    static void CreateMaterials(Material template)
    {
        skins = new Material[skinColors.Length];
        for (int i = 0; i < skinColors.Length; i++)
        {
            skins[i] = ModelBuilder.MakeMaterial(template, skinColors[i], 0.25f, 0.6f);
        }
        eyeMat = ModelBuilder.MakeMaterial(template, new Color(0.02f, 0.02f, 0.03f), 0f, 0.9f);
        shineMat = ModelBuilder.MakeMaterial(template, Color.white, 1f);
        saucerMat = ModelBuilder.MakeMaterial(template, new Color(0.6f, 0.63f, 0.7f), 0f, 0.85f);
        lightMat = ModelBuilder.MakeMaterial(template, new Color(1f, 0.85f, 0.25f), 2f);
    }

    void Assemble(Material skin)
    {
        // The camera looks toward +z, so the front of the model faces -z.
        body = ModelBuilder.Group("AlienBody", transform, Vector3.zero);

        // Saucer: a tilted disc with a rounded underside and three running lights.
        ModelBuilder.Part(PrimitiveType.Cylinder, body, saucerMat, new Vector3(0f, -0.15f, 0f), new Vector3(1.7f, 0.07f, 1.7f), 0f, -15f);
        ModelBuilder.Part(PrimitiveType.Sphere, body, saucerMat, new Vector3(0f, -0.24f, 0f), new Vector3(0.95f, 0.28f, 0.95f));
        ModelBuilder.Part(PrimitiveType.Sphere, body, lightMat, new Vector3(0f, -0.37f, -0.82f), Vector3.one * 0.14f);
        ModelBuilder.Part(PrimitiveType.Sphere, body, lightMat, new Vector3(-0.5f, -0.32f, -0.64f), Vector3.one * 0.13f);
        ModelBuilder.Part(PrimitiveType.Sphere, body, lightMat, new Vector3(0.5f, -0.32f, -0.64f), Vector3.one * 0.13f);

        // Head with big slanted eyes.
        ModelBuilder.Part(PrimitiveType.Sphere, body, skin, new Vector3(0f, 0.2f, 0f), new Vector3(0.75f, 0.8f, 0.75f));
        for (int side = -1; side <= 1; side += 2)
        {
            ModelBuilder.Part(PrimitiveType.Sphere, body, eyeMat, new Vector3(0.16f * side, 0.27f, -0.32f), new Vector3(0.2f, 0.3f, 0.12f), -25f * side);
            ModelBuilder.Part(PrimitiveType.Sphere, body, shineMat, new Vector3(0.13f * side, 0.34f, -0.39f), Vector3.one * 0.06f);

            // Antenna with a glowing tip.
            ModelBuilder.Part(PrimitiveType.Cylinder, body, skin, new Vector3(0.2f * side, 0.72f, 0f), new Vector3(0.04f, 0.2f, 0.04f), -25f * side);
            ModelBuilder.Part(PrimitiveType.Sphere, body, lightMat, new Vector3(0.285f * side, 0.9f, 0f), Vector3.one * 0.13f);
        }

        phase = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        // Gentle hover animation. Only the visual moves; the alien's collider stays where AlienFleet put it.
        float t = Time.time;
        body.localPosition = new Vector3(0f, Mathf.Sin(t * 3f + phase) * 0.08f, 0f);
        body.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 2f + phase) * 8f);
    }
}
