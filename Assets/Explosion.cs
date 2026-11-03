using System.Collections.Generic;
using UnityEngine;

// A small burst: a bright flash plus debris cubes that fly outward, spin and shrink away.
public class Explosion : MonoBehaviour
{
    const float lifetime = 0.7f;

    static Shader shader;
    static readonly Dictionary<Color, Material> materials = new Dictionary<Color, Material>();

    Transform flash;
    float flashSize;
    Transform[] pieces;
    Vector3[] velocities;
    Vector3[] spins;
    Vector3[] startScales;
    float age;

    public static void Spawn(Vector3 position, Color color, float size, int pieceCount)
    {
        Explosion e = new GameObject("Explosion").AddComponent<Explosion>();
        e.transform.position = position;
        e.Build(color, size, pieceCount);
    }

    // Materials are cached per color so repeated explosions don't keep allocating new ones.
    static Material GetMaterial(Color color)
    {
        if (materials.TryGetValue(color, out Material cached) && cached != null) return cached;

        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");

        Material m = new Material(shader);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
        if (m.HasProperty("_Color")) m.SetColor("_Color", color);
        materials[color] = m;
        return m;
    }

    void Build(Color color, float size, int count)
    {
        Color hot = new Color(1f, 0.95f, 0.6f);
        flash = ModelBuilder.Part(PrimitiveType.Sphere, transform, GetMaterial(hot), Vector3.zero, Vector3.zero);
        flashSize = size * 1.6f;

        pieces = new Transform[count];
        velocities = new Vector3[count];
        spins = new Vector3[count];
        startScales = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            Material mat = GetMaterial(i % 3 == 0 ? hot : color);
            startScales[i] = Vector3.one * size * Random.Range(0.12f, 0.25f);
            pieces[i] = ModelBuilder.Part(PrimitiveType.Cube, transform, mat, Vector3.zero, startScales[i], Random.Range(0f, 360f));

            Vector2 dir = Random.insideUnitCircle.normalized;
            velocities[i] = new Vector3(dir.x, dir.y, 0f) * size * Random.Range(3f, 7f);
            spins[i] = Random.insideUnitSphere * 720f;
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        age += dt;
        float t = age / lifetime;
        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // Flash pops up quickly, then collapses.
        float f = t < 0.2f ? t / 0.2f : 1f - (t - 0.2f) / 0.3f;
        flash.localScale = Vector3.one * flashSize * Mathf.Max(0f, f);

        float drag = Mathf.Max(0f, 1f - 2.5f * dt);
        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] == null) continue;
            pieces[i].localPosition += velocities[i] * dt;
            velocities[i] *= drag;
            pieces[i].Rotate(spins[i] * dt);
            pieces[i].localScale = startScales[i] * (1f - t);
        }
    }
}
