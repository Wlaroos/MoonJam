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

    private float lastSpawnTime = 0f;

    Enemy Zombie = new Enemy();
    Enemy BigZombie = new Enemy();
    Enemy SmallZombie = new Enemy();
    List<Enemy> EnemyList = new List<Enemy>();
    List<GameObject> LiveEnemyList = new List<GameObject>();

    // State machine
    private enum GameState
    {
        Start,
        PreWave,
        DuringWave,
        PostWave,
        End,
        Paused
    }

    private GameState currentState;

    private void Awake()
    {
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

        // Initialize the state
        currentState = GameState.Start;
    }

    private void Update()
    {
        switch (currentState)
        {
            case GameState.Start:
                HandleStartState();
                break;
            case GameState.PreWave:
                HandlePreWaveState();
                break;
            case GameState.DuringWave:
                HandleDuringWaveState();
                break;
            case GameState.PostWave:
                HandlePostWaveState();
                break;
            case GameState.End:
                HandleEndState();
                break;
            case GameState.Paused:
                HandlePausedState();
                break;
        }
    }

    private void HandleStartState()
    {
        // Logic for the Start state
        Debug.Log("Game is in Start state.");
        // Transition to PreWave
        currentState = GameState.PreWave;
    }

    private void HandlePreWaveState()
    {
        // Logic for the PreWave state
        Debug.Log("Game is in PreWave state.");
        // Transition to DuringWave
        currentState = GameState.DuringWave;
    }

    private void HandleDuringWaveState()
    {
        // Logic for the DuringWave state
        Debug.Log("Game is in DuringWave state.");
        SpawnEnemies();

        // Example condition to transition to PostWave
        if (LiveEnemyList.Count == 0 && MaxPoints <= 0)
        {
            currentState = GameState.PostWave;
        }
    }

    private void HandlePostWaveState()
    {
        // Logic for the PostWave state
        Debug.Log("Game is in PostWave state.");
        // Transition to End or PreWave based on game logic
        currentState = GameState.End;
    }

    private void HandleEndState()
    {
        // Logic for the End state
        Debug.Log("Game is in End state.");
        // Game over logic here
    }

    private void HandlePausedState()
    {
        // Logic for the Paused state
        Debug.Log("Game is Paused.");
        // Wait for unpause
    }

    private void SpawnEnemies()
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

                float spawnY = UnityEngine.Random.Range(minY, maxY);
                float spawnX = UnityEngine.Random.Range(minX, maxX);
                Vector2 spawnPosition = new Vector2(spawnX, spawnY);

                if (EnemyList.Count == 0) return; // No enemies allowed to spawn

                Enemy Spawnee = EnemyList[rand.Next(EnemyList.Count)];
                if (MaxPoints >= Spawnee.cost)
                {
                    GameObject holder = Instantiate(Spawnee.prefab, spawnPosition, Quaternion.identity);

                    LiveEnemyList.Add(holder);

                    holder.GetComponent<EnemyHealth>().OnEnemyDowned.AddListener(() => RestorePoints(holder, Spawnee.cost));
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

    private void RestorePoints(GameObject holder, float points)
    {
        holder.GetComponent<EnemyHealth>().OnEnemyDowned.RemoveAllListeners();
        MaxPoints += points;
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