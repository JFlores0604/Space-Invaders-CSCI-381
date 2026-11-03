using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Spaceship : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject dartPrefab;
    public float shotCooldown = .35f;     // was .05 (balance: no more machine-gunning)
    public float speed = 10f;
    public float edgeMargin = 1.2f;       // keeps the whole ship model on screen
    public float invulnerableTime = 1.5f; // after being hit

    float lastShot = -Mathf.Infinity;
    bool invulnerable = false;
    bool destroyed = false;
    Coroutine blink;
    Camera cam;
    Renderer[] renderers;

    void Awake()
    {
        cam = Camera.main;
        JetModel.Build(gameObject);   // fighter jet instead of three capsules (must run before caching renderers)
        renderers = GetComponentsInChildren<Renderer>();

        // Bullets hitting the ship shouldn't push it around: lock its physics body in place.
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (destroyed || (gm != null && !gm.IsPlaying)) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        // One movement calculation instead of duplicated left/right blocks.
        float direction = 0f;
        if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) direction += 1f;
        if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) direction -= 1f;

        Vector3 position = transform.position;
        position.x += direction * speed * Time.deltaTime;

        // Prevent the spaceship from moving offscreen.
        if (cam != null)
        {
            float halfWidth = cam.orthographicSize * cam.aspect - edgeMargin;
            float centerX = cam.transform.position.x;
            position.x = Mathf.Clamp(position.x, centerX - halfWidth, centerX + halfWidth);
        }
        transform.position = position;

        if (keyboard.spaceKey.wasPressedThisFrame && Time.time - lastShot > shotCooldown)
        {
            Instantiate(dartPrefab, transform.position, dartPrefab.transform.rotation);
            lastShot = Time.time;
        }
    }

    // Called by an AlienBullet that hits the ship.
    public void Hit()
    {
        if (invulnerable || destroyed) return;

        GameManager gm = GameManager.Instance;
        if (gm == null) return;
        gm.LoseLife();

        if (gm.Lives > 0)
        {
            blink = StartCoroutine(Blink());
        }
        else
        {
            Explode();   // out of lives: the ship blows up
        }
    }

    // Blows the ship up. Also called by AlienFleet when the aliens reach the ship's row.
    public void Explode()
    {
        if (destroyed) return;
        destroyed = true;
        if (blink != null) StopCoroutine(blink);

        // Explode at the middle of the jet model rather than the root's pivot.
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers) bounds.Encapsulate(r.bounds);
        SetVisible(false);

        Explosion.Spawn(bounds.center, new Color(1f, 0.5f, 0.1f), 1.6f, 24);
        Explosion.Spawn(bounds.center, new Color(0.8f, 0.82f, 0.88f), 1.2f, 10);   // bits of hull
    }

    IEnumerator Blink()
    {
        invulnerable = true;
        float endTime = Time.time + invulnerableTime;
        bool visible = true;
        while (Time.time < endTime)
        {
            visible = !visible;
            SetVisible(visible);
            yield return new WaitForSeconds(0.1f);
        }
        SetVisible(true);
        invulnerable = false;
    }

    void SetVisible(bool visible)
    {
        foreach (Renderer r in renderers)
        {
            r.enabled = visible;
        }
    }
}
