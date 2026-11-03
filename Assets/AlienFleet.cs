using UnityEngine;
using System.Collections.Generic;

public class AlienFleet : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject alienPrefab;
    public AlienCounter alienCounter;

    public int columns = 10;
    public int rows = 4;                 // was 3 (balance: more aliens)
    public float startSpeed = 0.06f;     // sideways movement per physics step at the start
    public float maxSpeed = 0.35f;       // speed when only the last alien is left
    public float dropDistance = 0.6f;    // how far the fleet drops when it hits an edge
    public float leftBound = -16f;
    public float rightBound = 16f;
    public float loseLineY = -5.5f;      // just above the ship (ship sits at y = -6.83)
    public float endScreenDelay = 1.2f;  // lets the final explosion play before the end screen

    [Header("Dynamic")]
    public List<Alien> aliens = new List<Alien>();

    int totalAliens;
    float currentSpeed;
    int direction = 1;                   // +1 = moving right, -1 = moving left
    Spaceship ship;

    void Awake()
    {
        GameManager.EnsureExists();
    }

    void Start()
    {
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                Vector3 position = new Vector3(2 * i - 12, 10 - 2 * j, 0);
                GameObject alienGO = Instantiate(alienPrefab, position, alienPrefab.transform.rotation);
                Alien alien = alienGO.GetComponent<Alien>();
                alien.fleet = this;
                aliens.Add(alien);
            }
        }

        ship = FindAnyObjectByType<Spaceship>();
        totalAliens = aliens.Count;
        currentSpeed = startSpeed;
        UpdateCounter();
    }

    void FixedUpdate()
    {
        if (aliens.Count == 0) return;

        // Move every alien sideways and find the fleet's edges in the same pass.
        float step = direction * currentSpeed;
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        foreach (Alien alien in aliens)
        {
            Vector3 position = alien.transform.position;
            position.x += step;
            alien.transform.position = position;
            minX = Mathf.Min(minX, position.x);
            maxX = Mathf.Max(maxX, position.x);
        }

        bool hitEdge = (direction > 0 && maxX >= rightBound) || (direction < 0 && minX <= leftBound);
        if (hitEdge)
        {
            direction = -direction;
            MoveDown();
        }
    }

    void MoveDown()
    {
        float lowestY = float.MaxValue;
        foreach (Alien alien in aliens)
        {
            Vector3 position = alien.transform.position;
            position.y -= dropDistance;
            alien.transform.position = position;
            lowestY = Mathf.Min(lowestY, position.y);
        }

        // Once the aliens reach the ship's row, the game can no longer be won.
        if (lowestY <= loseLineY)
        {
            if (ship != null) ship.Explode();
            GameManager.Instance.EndGame(false, endScreenDelay);
        }
    }

    // Called by an Alien when it is destroyed. Returns false if it was already removed.
    public bool RemoveAlien(Alien alien)
    {
        if (!aliens.Remove(alien)) return false;

        // Remaining aliens speed up as their compatriots are destroyed.
        float fractionKilled = 1f - (float)aliens.Count / totalAliens;
        currentSpeed = Mathf.Lerp(startSpeed, maxSpeed, fractionKilled);

        UpdateCounter();

        if (aliens.Count == 0)
        {
            GameManager.Instance.EndGame(true, endScreenDelay);
        }
        return true;
    }

    void UpdateCounter()
    {
        if (alienCounter != null)
        {
            alienCounter.SetCount(aliens.Count);
        }
    }
}
