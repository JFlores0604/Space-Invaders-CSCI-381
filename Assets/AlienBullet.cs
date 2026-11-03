using UnityEngine;

public class AlienBullet : MonoBehaviour
{
    [Header("Inscribed")]
    public float speed = .15f;   // was .1 (balance: faster alien bullets)

    void FixedUpdate()
    {
        Vector3 position = transform.position;
        position.y -= speed;
        transform.position = position;
        if (position.y < -10)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        Spaceship ship = coll.collider.GetComponentInParent<Spaceship>();
        if (ship != null)
        {
            ship.Hit();   // costs a life instead of destroying the ship outright
        }
        else
        {
            Destroy(coll.gameObject);
        }
        Destroy(gameObject);
    }
}
