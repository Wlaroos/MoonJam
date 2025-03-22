using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyDude : MonoBehaviour
{
    [SerializeField] private GameObject _gun;
    [SerializeField] private GameObject _bulletRef; // Assign your bullet prefab in the Inspector
    [SerializeField] private Transform _bulletSpawnPoint; // Assign the spawn point for bullets
    [SerializeField] private float _burstDelay = 1.0f; // Time between bursts
    [SerializeField] private int _burstAmount = 3; // Time between individual shots in a burst
    [SerializeField] private float _shotDelay = 0.2f; // Time between individual shots in a burst
    private List<GameObject> _enemies = new List<GameObject>();
    private bool _isShooting = false;
    private GameObject _cachedClosestEnemy;
    private float _closestEnemyUpdateInterval = 0.5f; // Update closest enemy every 0.5 seconds
    private float _closestEnemyTimer = 0f;
    private SpriteRenderer _gunSR;
    private SpriteRenderer _sr;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _sr.sprite = Resources.LoadAll<Sprite>("Army")[Random.Range(0, 7)];
        
        _gunSR = _gun.GetComponentInChildren<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        _enemies.RemoveAll(enemy => enemy == null); // Clean up null references

        if (other.gameObject.tag == "Enemy")
        {
            _enemies.Add(other.gameObject);
        }
    }

    private void Update()
    {
        // Update the closest enemy at intervals
        _closestEnemyTimer += Time.deltaTime;
        if (_closestEnemyTimer >= _closestEnemyUpdateInterval)
        {
            _cachedClosestEnemy = GetClosestEnemy();
            _closestEnemyTimer = 0f;
        }

        // If no closest enemy, return early
        if (_cachedClosestEnemy == null) return;

        Aim();

        if (_enemies.Count > 0 && !_isShooting)
        {
            StartCoroutine(ShootBurst());
        }
    }

    private void Aim()
    {
        GameObject closestEnemy = _cachedClosestEnemy;

        if (closestEnemy != null)
        {
            // Calculate the direction to the closest enemy
            Vector3 direction = closestEnemy.transform.position - _gun.transform.position;
            direction.z = 0; // Ensure the direction is in 2D space

            // Rotate the gun to face the closest enemy
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            _gun.transform.rotation = Quaternion.Euler(0, 0, angle);

            Vector3 localScale = _gunSR.transform.localScale;
            localScale.y = Mathf.Abs(localScale.y) * ((angle > 90 || angle < -90) ? -1f : 1f);
            _sr.flipX = angle > 90 || angle < -90;
            _gunSR.transform.localScale = localScale;
        }
        else
        {
            // Optionally, reset the gun's rotation when no enemies are present
            _gun.transform.rotation = Quaternion.identity;
        }
    }

    private GameObject GetClosestEnemy()
    {
        GameObject closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        // Clean up null references before finding the closest enemy
        _enemies.RemoveAll(enemy => enemy == null);

        foreach (GameObject enemy in _enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }

    private IEnumerator ShootBurst()
    {
        _isShooting = true;

        for (int i = 0; i < _burstAmount; i++) // _burstAmount shots in a burst
        {
            // Stop shooting if there are no more enemies
            if (_cachedClosestEnemy == null || _enemies.Count == 0)
            {
                _isShooting = false;
                yield break; // Exit the coroutine early
            }

            Shoot();
            yield return new WaitForSeconds(_shotDelay); // Wait between shots
        }

        yield return new WaitForSeconds(_burstDelay); // Wait between bursts
        _isShooting = false;
    }

    private void Shoot()
    {
        if (_bulletRef != null && _bulletSpawnPoint != null)
        {
            // Instantiate the bullet at the spawn point
            GameObject bulletInstance = Instantiate(_bulletRef, _bulletSpawnPoint.position, _gun.transform.rotation);

            // Calculate the direction vector based on the gun's rotation
            Vector3 direction = _gun.transform.right; // Use the gun's right vector for the forward direction

            // Get the angle from the gun's rotation
            float angle = _gun.transform.rotation.eulerAngles.z;

            // Set up the bullet
            BulletBase bullet = bulletInstance.GetComponent<BulletBase>();
            if (bullet != null)
            {
                bullet.BulletSetup(
                    direction,  // Pass the direction vector
                    angle,      // Pass the angle
                    30f,        // Speed
                    3,          // Damage
                    5f,         // Lifetime
                    0.5f,       // Size
                    3f          // Knockback
                );
            }
        }
    }
}
