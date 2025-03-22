using System.Collections.Generic;
using UnityEngine;
using System.Collections;

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
    private Transform playerRef; // Reference to the player's transform
    private float minDistanceFromPlayer = 2f; // Minimum distance from the player
    private float lastSpawnTime = 0f;
    private bool isHordeSpawningOver = true;
    public bool HordeSpawningOver => isHordeSpawningOver;


    private Enemy Zombie = new Enemy();
    private Enemy BigZombie = new Enemy();
    private Enemy SmallZombie = new Enemy();
    private List<Enemy> EnemyList = new List<Enemy>();
    public List<GameObject> LiveEnemyList = new List<GameObject>();

    private void Awake()
    {
        playerRef = FindObjectOfType<PlayerMovement>().transform;
        rand = new System.Random();

        UpdateEnemyList(); // Initialize the enemy list
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
                float minY = cam.transform.position.y - cam.orthographicSize - SpawnBorderBuffer; // Spawn outside the bottom
                float maxY = cam.transform.position.y + cam.orthographicSize + SpawnBorderBuffer; // Spawn outside the top
                float minX = cam.transform.position.x - cam.orthographicSize * cam.aspect - SpawnBorderBuffer; // Spawn outside the left
                float maxX = cam.transform.position.x + cam.orthographicSize * cam.aspect + SpawnBorderBuffer; // Spawn outside the right

                Vector2 spawnPosition;
                int maxAttempts = 10; // Limit attempts to find a valid spawn position
                int attempts = 0;

                do
                {
                    // Randomly decide whether to spawn on the top/bottom or left/right
                    bool spawnHorizontally = UnityEngine.Random.value > 0.5f;

                    if (spawnHorizontally)
                    {
                        // Spawn on the left or right side
                        float spawnX = UnityEngine.Random.value > 0.5f ? minX : maxX;
                        float spawnY = UnityEngine.Random.Range(minY, maxY);
                        spawnPosition = new Vector2(spawnX, spawnY);
                    }
                    else
                    {
                        // Spawn on the top or bottom side
                        float spawnX = UnityEngine.Random.Range(minX, maxX);
                        float spawnY = UnityEngine.Random.value > 0.5f ? minY : maxY;
                        spawnPosition = new Vector2(spawnX, spawnY);
                    }

                    attempts++;
                }
                while (Vector2.Distance(spawnPosition, playerRef.position) < minDistanceFromPlayer && attempts < maxAttempts);

                if (attempts >= maxAttempts) return; // Exit if no valid position is found

                if (EnemyList.Count == 0) return; // No enemies allowed to spawn

                Enemy Spawnee = EnemyList[rand.Next(EnemyList.Count)];
                if (MaxPoints >= Spawnee.cost)
                {
                    GameObject holder = Instantiate(Spawnee.prefab, spawnPosition, Quaternion.identity);

                    LiveEnemyList.Add(holder);

                    holder.GetComponent<EnemyHealth>().OnEnemyDowned.AddListener(() => RemoveFromLiveEnemyList(holder));
                    lastSpawnTime = Time.time;
                    MaxPoints -= Spawnee.cost;
                }
                else
                {
                    // Spawn one last zombie and remove all remaining points
                    Enemy lastZombie = EnemyList[0]; // Default to the first enemy in the list
                    GameObject holder = Instantiate(lastZombie.prefab, spawnPosition, Quaternion.identity);

                    LiveEnemyList.Add(holder);

                    holder.GetComponent<EnemyHealth>().OnEnemyDowned.AddListener(() => RemoveFromLiveEnemyList(holder));
                    lastSpawnTime = Time.time;
                    MaxPoints = 0; // Remove all remaining points
                    break;
                }
            }
        }
    }

    public void HordeSpawn(int hordeSize)
    {
        StartCoroutine(SpawnHorde(hordeSize));
    }

    private IEnumerator SpawnHorde(int hordeSize)
    {
        Camera cam = Camera.main;

        // Calculate the bottom of the screen
        float spawnY = cam.transform.position.y - cam.orthographicSize - SpawnBorderBuffer;
        float minX = cam.transform.position.x - cam.orthographicSize * cam.aspect + SpawnBorderBuffer;
        float maxX = cam.transform.position.x + cam.orthographicSize * cam.aspect - SpawnBorderBuffer;

        isHordeSpawningOver = false;

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < hordeSize; i++)
        {
            if (AllowZombie)
            {
                // Randomize the X position within the screen bounds
                float spawnX = UnityEngine.Random.Range(minX, maxX);
                Vector2 spawnPosition = new Vector2(spawnX, spawnY);

                // Spawn the zombie
                GameObject holder = Instantiate(ZombiePrefab, spawnPosition, Quaternion.identity);
                LiveEnemyList.Add(holder);

                holder.GetComponent<EnemyHealth>().OnEnemyDowned.AddListener(() => RemoveFromLiveEnemyList(holder));

                // Wait for 0.1 seconds before spawning the next zombie
                yield return new WaitForSeconds(0.1f);
            }
        }

        isHordeSpawningOver = true;
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

    private void RemoveFromLiveEnemyList(GameObject holder)
    {
        LiveEnemyList.Remove(holder);
    }

    public void UpdateEnemyList()
    {
        EnemyList.Clear();

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
}