using System;
using System.Collections;
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

    [Header("Small Zombie")]
    public GameObject ZombiePrefab;
    public float ZombieCost = 10;

    [Header("Big Zombie")]
    public GameObject BigZombiePrefab;
    public float BigZombieCost = 10;

    [Header("Small Zombie")]
    public GameObject SmallZombiePrefab;
    public float SmallZombieCost = 20;

    Enemy Zombie = new Enemy();
    Enemy BigZombie = new Enemy();
    Enemy SmallZombie = new Enemy();
    List<Enemy> EnemyList = new List<Enemy>();
    List<GameObject> LiveEnemyList = new List<GameObject>();

    private void Awake()
    {
        rand = new System.Random();

        Zombie.prefab = ZombiePrefab;
        Zombie.cost = ZombieCost;
        EnemyList.Add(Zombie);

        BigZombie.prefab = BigZombiePrefab;
        BigZombie.cost = BigZombieCost;
        EnemyList.Add(BigZombie);

        SmallZombie.prefab = SmallZombiePrefab;
        SmallZombie.cost = SmallZombieCost;
        EnemyList.Add(SmallZombie);
    }

    [Space]
    public float MaxPoints = 100;
    public float SpawnDelay = 1;

    private float lastSpawnTime = 0f;

    private void Update()
    {
        while(MaxPoints > 0)
        {
            if (Time.time < lastSpawnTime + SpawnDelay)
            {
                return;
            }
            else
            {
                float spawnY = UnityEngine.Random.Range
                    (Camera.main.ScreenToWorldPoint(new Vector2(0, 0)).y, Camera.main.ScreenToWorldPoint(new Vector2(0, Screen.height)).y);
                float spawnX = UnityEngine.Random.Range
                    (Camera.main.ScreenToWorldPoint(new Vector2(0, 0)).x, Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, 0)).x);
                Vector2 spawnPosition = new Vector2(spawnX, spawnY);
                Enemy Spawnee = EnemyList[rand.Next(EnemyList.Count)];
                if (MaxPoints >= Spawnee.cost)
                {
                    GameObject holder = Instantiate(Spawnee.prefab, spawnPosition, Quaternion.identity);

                    LiveEnemyList.Add(holder);

                    holder.GetComponent<EnemyHealth>().OnEnemyDowned.AddListener(() => RestorePoints(holder, Spawnee.cost));
                    lastSpawnTime = Time.time;
                    MaxPoints -= Spawnee.cost;
                   //Debug.Log("Detracted " + Spawnee.cost + " Total: " + MaxPoints);
                }
                else { break; }
            }
        }
    }
    private void RestorePoints(GameObject holder, float points)
    {
        holder.GetComponent<EnemyHealth>().OnEnemyDowned.RemoveAllListeners();
        MaxPoints += points;
        //Debug.Log("Added " + Spawnee.cost + " Total: " + MaxPoints);
    }
}
