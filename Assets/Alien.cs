using UnityEngine;

public class Alien : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject bulletPrefab;
    public AlienFleet fleet;

    static float bulletSpawnRate = .0016f;  // was .002 (slightly fewer shots, since bullets are now faster and there are more aliens)
    static float shotCooldown = .1f;
    float lastShot = 0;
    AlienModel model;

    void Awake()
    {
        model = AlienModel.Build(gameObject);   // flying-saucer alien instead of a plain sphere
    }

    void FixedUpdate()
    {
        if (Random.value < bulletSpawnRate && Time.time - lastShot > shotCooldown)
        {
            Instantiate(bulletPrefab, transform.position, bulletPrefab.transform.rotation);
            lastShot = Time.time;
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        // fleet is already an AlienFleet, so no GetComponent call is needed.
        if (!fleet.RemoveAlien(this)) return;
        Color color = model != null ? model.SkinColor : Color.green;
        Explosion.Spawn(transform.position, color, 0.9f, 12);
        Destroy(gameObject);
        Destroy(coll.gameObject);
    }
}
