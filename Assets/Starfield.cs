using UnityEngine;

// Replaces the plain blue background with a slowly scrolling starfield.
// Created automatically by GameManager; nothing to set up in the editor.
public class Starfield : MonoBehaviour
{
    public float scrollSpeed = 0.01f;
    Material material;

    public static void Create()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Dark space color, also used as a fallback if the shader can't be found.
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.06f);

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Texture");
        if (shader == null) return;

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "Starfield";
        Destroy(quad.GetComponent<Collider>());   // background must not collide with anything

        // Place it behind the game (the camera looks toward +z) and size it to cover the view.
        Vector3 camPos = cam.transform.position;
        quad.transform.position = new Vector3(camPos.x, camPos.y, 20f);
        float height = cam.orthographicSize * 2f * 1.1f;
        float width = height * cam.aspect * 1.1f;
        quad.transform.localScale = new Vector3(width, height, 1f);

        Texture2D stars = MakeStarTexture(512, 400);
        Material mat = new Material(shader);
        mat.mainTexture = stars;
        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", stars);
        mat.mainTextureScale = new Vector2(width / height, 1f);

        quad.GetComponent<Renderer>().material = mat;
        quad.AddComponent<Starfield>().material = mat;
    }

    static Texture2D MakeStarTexture(int size, int starCount)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Point;

        Color space = new Color(0.02f, 0.02f, 0.06f);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = space;

        for (int i = 0; i < starCount; i++)
        {
            int x = Random.Range(0, size);
            int y = Random.Range(0, size);
            float brightness = Random.Range(0.4f, 1f);
            pixels[y * size + x] = new Color(brightness, brightness, brightness);
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    void Update()
    {
        // Drift the stars downward so it feels like the ship is flying forward.
        Vector2 offset = material.mainTextureOffset;
        offset.y += scrollSpeed * Time.deltaTime;
        material.mainTextureOffset = offset;
    }
}
