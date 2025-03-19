using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
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
    private Animator _anim;
    private bool _canMove = false;
    private bool _isKnockback = false; // Track knockback state
    private Vector2 _smoothedFlockingForce = Vector2.zero; // Store the smoothed force

    private void Awake()
    {
        InitializeComponents();
        RandomizeSpeed();
    }

    private void Start()
    {
        StartCoroutine(StartDelay());
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

    private void OnTriggerStay2D(Collider2D other)
    {
        HandlePlayerCollision(other);
    }

    private void OnDrawGizmosSelected()
    {
        if (_enableFlocking)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _flockRadius);
        }
    }

    private void InitializeComponents()
    {
        _playerTransform = GameObject.FindWithTag("Player").transform;
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponentInChildren<SpriteRenderer>();
        _enemyHealth = GetComponent<EnemyHealth>();
        _anim = GetComponentInChildren<Animator>();
    }

    private void RandomizeSpeed()
    {
        _speed = Random.Range(_speed - 1, _speed + 1);
    }

    private void MoveTowardsPlayer()
    {
        Vector2 directionTowardsTarget = (_playerTransform.position - transform.position).normalized;
        
        _sr.flipX = directionTowardsTarget.x < 0;
        _rb.MovePosition(_rb.position + directionTowardsTarget * _speed * Time.fixedDeltaTime);
    }

    private IEnumerator StartDelay()
    {
        _canMove = false;
        yield return new WaitForSeconds(_startDelay);
        _canMove = true;
        _anim.Play("Sway", 0, Random.value);
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
    private bool Please = false;
    private void ApplyFlockingBehavior()
    {
        Vector2 alignment = Vector2.zero;
        Vector2 cohesion = Vector2.zero;
        Vector2 separation = Vector2.zero;

        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, _flockRadius);
        int count = 0;

        foreach (var enemy in nearbyEnemies)
        {
            if (enemy.gameObject != gameObject && enemy.gameObject.layer == LayerMask.NameToLayer("Enemy") && enemy.TryGetComponent(out EnemyMovement otherEnemy))
            {
                if (otherEnemy._enemyHealth.IsDowned || otherEnemy._isKnockback)
                    continue;

                Vector2 toOther = (Vector2)enemy.transform.position - (Vector2)transform.position;

                alignment += otherEnemy._rb.velocity;
                cohesion += (Vector2)enemy.transform.position;
                separation -= toOther.normalized / Mathf.Max(toOther.magnitude, 0.1f);

                count++;
            }
        }

        if (count > 0)
        {
            alignment /= count;
            cohesion = (cohesion / count - (Vector2)transform.position).normalized;
            separation /= count;
        }

        Vector2 directionTowardsPlayer = (_playerTransform.position - transform.position).normalized;
       
        Vector2 rawFlockingForce = (alignment * _alignmentWeight + cohesion * _cohesionWeight + separation * _separationWeight + directionTowardsPlayer).normalized;

        _smoothedFlockingForce = Vector2.Lerp(_smoothedFlockingForce, rawFlockingForce, 0.1f);

        _rb.MovePosition(_rb.position + _smoothedFlockingForce * _speed * Time.fixedDeltaTime);
        _sr.flipX = _smoothedFlockingForce.x < 0;
    }

    private void HandlePlayerCollision(Collider2D other)
    {
        if (other.GetComponent<PlayerHealth>() != null && !_enemyHealth.IsDowned)
        {
            Vector2 directionTowardsTarget = (_playerTransform.position - transform.position).normalized;
            other.GetComponent<PlayerHealth>().TakeDamage(directionTowardsTarget, 1);
        }
    }
}