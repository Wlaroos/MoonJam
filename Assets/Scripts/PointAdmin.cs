using System.Collections.Generic;
using UnityEngine;

public class Enemy
{
    public GameObject prefab;
    public float cost;
}

public class PointAdmin : MonoBehaviour
{
    private System.Random rand;

    [Header("Zombie")]
    public bool AllowZombie = true; // Toggle for Zombie
    public GameObject ZombiePrefab;
    public float ZombieCost = 10;

    [Header("Big Zombie")]
    public bool AllowBigZombie = true; // Toggle for Big Zombie
    public GameObject BigZombiePrefab;
    public float BigZombieCost = 10;

    [Header("Small Zombie")]
    public bool AllowSmallZombie = true; // Toggle for Small Zombie
    public GameObject SmallZombiePrefab;
    public float SmallZombieCost = 20;

    [Header("Spawn Settings")]
    public float MaxPoints = 100;
    public float SpawnDelay = 1;
    public float SpawnBorderBuffer = 1f; // Buffer for spawn area

    private Transform _playerRef; // Reference to the player's transform
    private float _minDistanceFromPlayer = 2f; // Minimum distance from the player

    private float lastSpawnTime = 0f;

    private Enemy Zombie = new Enemy();
    private Enemy BigZombie = new Enemy();
    private Enemy SmallZombie = new Enemy();
    private List<Enemy> EnemyList = new List<Enemy>();
    public List<GameObject> LiveEnemyList = new List<GameObject>();

    private void Awake()
    {
        _playerRef = FindObjectOfType<PlayerMovement>().transform;
        rand = new System.Random();

        if (AllowZombie)
        {
            Zombie.prefab = ZombiePrefab;
            Zombie.cost = ZombieCost;
            EnemyList.Add(Zombie);
        }

        if (AllowBigZombie)
        {
            BigZombie.prefab = BigZombiePrefab;
            BigZombie.cost = BigZombieCost;
            EnemyList.Add(BigZombie);
        }

        if (AllowSmallZombie)
        {
            SmallZombie.prefab = SmallZombiePrefab;
            SmallZombie.cost = SmallZombieCost;
            EnemyList.Add(SmallZombie);
        }
    }

    public void SpawnEnemies()
    {
        while (MaxPoints > 0)
        {
            if (Time.time < lastSpawnTime + SpawnDelay)
            {
                return;
            }
            else
            {
                Camera cam = Camera.main;

                // Calculate the camera's world boundaries
                float minY = cam.transform.position.y - cam.orthographicSize + SpawnBorderBuffer;
                float maxY = cam.transform.position.y + cam.orthographicSize - SpawnBorderBuffer;
                float minX = cam.transform.position.x - cam.orthographicSize * cam.aspect + SpawnBorderBuffer;
                float maxX = cam.transform.position.x + cam.orthographicSize * cam.aspect - SpawnBorderBuffer;

                Vector2 spawnPosition;
                int maxAttempts = 10; // Limit attempts to find a valid spawn position
                int attempts = 0;

                do
                {
                    float spawnY = UnityEngine.Random.Range(minY, maxY);
                    float spawnX = UnityEngine.Random.Range(minX, maxX);
                    spawnPosition = new Vector2(spawnX, spawnY);
                    attempts++;
                }
                while (Vector2.Distance(spawnPosition, _playerRef.position) < _minDistanceFromPlayer && attempts < maxAttempts);

                if (attempts >= maxAttempts) return; // Exit if no valid position is found

                if (EnemyList.Count == 0) return; // No enemies allowed to spawn

                Enemy Spawnee = EnemyList[rand.Next(EnemyList.Count)];
                if (MaxPoints >= Spawnee.cost)
                {
                    GameObject holder = Instantiate(Spawnee.prefab, spawnPosition, Quaternion.identity);

                    LiveEnemyList.Add(holder);

                    holder.GetComponent<EnemyHealth>().OnEnemyDowned.AddListener(() => RemoveFromList(holder));
                    //holder.GetComponent<EnemyHealth>().OnEnemyDowned.AddListener(() => RestorePoints(holder, Spawnee.cost));
                    lastSpawnTime = Time.time;
                    MaxPoints -= Spawnee.cost;
                }
                else
                {
                    break;
                }
            }
        }
    }

    public void AddPoints(float points)
    {
        MaxPoints += points;
    }

    // private void RestorePoints(GameObject holder, float points)
    // {
    //     holder.GetComponent<EnemyHealth>().OnEnemyDowned.RemoveAllListeners();
    //     MaxPoints += points;
    // }

    private void RemoveFromList(GameObject holder)
    {
        LiveEnemyList.Remove(holder);
    }

    private void OnDrawGizmos()
    {
        // Ensure the main camera exists
        if (Camera.main == null) return;

        Camera cam = Camera.main;

        // Calculate the camera's world boundaries
        float minY = cam.transform.position.y - cam.orthographicSize + SpawnBorderBuffer;
        float maxY = cam.transform.position.y + cam.orthographicSize - SpawnBorderBuffer;
        float minX = cam.transform.position.x - cam.orthographicSize * cam.aspect + SpawnBorderBuffer;
        float maxX = cam.transform.position.x + cam.orthographicSize * cam.aspect - SpawnBorderBuffer;

        // Draw the spawn area as a rectangle
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(minX, minY, 0), new Vector3(maxX, minY, 0));
        Gizmos.DrawLine(new Vector3(maxX, minY, 0), new Vector3(maxX, maxY, 0));
        Gizmos.DrawLine(new Vector3(maxX, maxY, 0), new Vector3(minX, maxY, 0));
        Gizmos.DrawLine(new Vector3(minX, maxY, 0), new Vector3(minX, minY, 0));
    }
}