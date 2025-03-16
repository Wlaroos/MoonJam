using System.Collections;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float _speed = 3f;
    [SerializeField] private float _startDelay = 1f;

    [Header("Flocking Settings")]
    [SerializeField] private bool _enableFlocking = false;
    [SerializeField, Tooltip("Radius to detect nearby allies for flocking behavior.")]
    private float _flockRadius = 5f;
    [SerializeField, Tooltip("Alignment: Match velocity.")]
    private float _alignmentWeight = 1f;
    [SerializeField, Tooltip("Cohesion: Move towards the center of the group.")]
    private float _cohesionWeight = 1f;
    [SerializeField, Tooltip("Separation: Avoid crowding.")]
    private float _separationWeight = 1f;

    private Transform _playerTransform;
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private EnemyHealth _enemyHealth;
    private bool _canMove = false;
    private bool _isKnockback = false; // Track knockback state

    private void Awake()
    {
        // Use tag lookup to find the player (ensure your player GameObject has the "Player" tag)
        _playerTransform = GameObject.FindWithTag("Player").transform;
        
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponentInChildren<SpriteRenderer>();
        _enemyHealth = GetComponent<EnemyHealth>();

        _speed = Random.Range(_speed - 1, _speed + 1);
    }

    private void Start()
    {
        StartCoroutine(StartDelay());
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.GetComponent<PlayerHealth>() != null && !_enemyHealth.IsDowned)
        {
            Vector2 directionTowardsTarget = (_playerTransform.position - transform.position).normalized;
            other.GetComponent<PlayerHealth>().TakeDamage(directionTowardsTarget, 1);
        }
    }

    private void FixedUpdate()
    {
        if (!_enemyHealth.IsDowned && _canMove && !_isKnockback)
        {
            if (_enableFlocking)
            {
                ApplyFlockingBehavior();
            }
            else
            {
                MoveTowardsPlayer();
            }
        }
    }

    private void MoveTowardsPlayer()
    {
        // Calculate direction and move
        Vector2 directionTowardsTarget = (_playerTransform.position - transform.position).normalized;
        _sr.flipX = directionTowardsTarget.x < 0;
        _rb.MovePosition(_rb.position + directionTowardsTarget * _speed * Time.fixedDeltaTime);
    }
    private IEnumerator StartDelay()
    {
        _canMove = false;
        yield return new WaitForSeconds(_startDelay);
        _canMove = true;
    }

    public void Knockback(Vector2 force, float duration)
    {
        StartCoroutine(KnockbackStart(force, duration));
    }

    private IEnumerator KnockbackStart(Vector2 force, float duration)
    {
        _isKnockback = true;
        _rb.AddForce(force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(duration);

        _isKnockback = false;
    }

    private void ApplyFlockingBehavior()
    {
        Vector2 alignment = Vector2.zero;
        Vector2 cohesion = Vector2.zero;
        Vector2 separation = Vector2.zero;

        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, _flockRadius);
        int count = 0;

        foreach (var enemy in nearbyEnemies)
        {
            if (enemy.gameObject != gameObject && enemy.TryGetComponent(out EnemyMovement otherEnemy))
            {
                // Ignore downed enemies or enemies in knockback state
                if (otherEnemy._enemyHealth.IsDowned || otherEnemy._isKnockback)
                    continue;

                Vector2 toOther = (Vector2)enemy.transform.position - (Vector2)transform.position;

                // Alignment: Match velocity
                alignment += otherEnemy._rb.velocity;

                // Cohesion: Move towards the center of the group
                cohesion += (Vector2)enemy.transform.position;

                // Separation: Avoid crowding
                separation -= toOther.normalized / Mathf.Max(toOther.magnitude, 0.1f); // Avoid division by zero

                count++;
            }
        }

        if (count > 0)
        {
            alignment /= count;
            cohesion = (cohesion / count - (Vector2)transform.position).normalized;
            separation /= count;
        }

        // Add a vector toward the player to make the group chase the player
        Vector2 directionTowardsPlayer = (_playerTransform.position - transform.position).normalized;

        // Combine flocking forces with the direction toward the player
        Vector2 flockingForce = (alignment * _alignmentWeight + cohesion * _cohesionWeight + separation * _separationWeight + directionTowardsPlayer).normalized;

        // Move the enemy using the combined force
        _rb.MovePosition(_rb.position + flockingForce * _speed * Time.fixedDeltaTime);

        // Flip sprite based on movement direction
        _sr.flipX = flockingForce.x < 0;
    }

    private void OnDrawGizmosSelected()
    {
        if (_enableFlocking)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _flockRadius);
        }
    }
}
