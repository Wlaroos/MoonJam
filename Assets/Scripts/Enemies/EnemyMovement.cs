using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] protected float _speed = 3f;
    [SerializeField] protected float _startDelay = 1f;
    [SerializeField] protected float _knockbackMult = 1f;

    [Header("Flocking Settings")]
    [SerializeField] protected bool _enableFlocking = false;
    [SerializeField, Tooltip("Radius to detect nearby allies for flocking behavior.")]
    protected float _flockRadius = 5f;
    [SerializeField, Tooltip("Alignment: Match velocity.")]
    protected float _alignmentWeight = 1f;
    [SerializeField, Tooltip("Cohesion: Move towards the center of the group.")]
    protected float _cohesionWeight = 1f;
    [SerializeField, Tooltip("Separation: Avoid crowding.")]
    protected float _separationWeight = 1f;

    protected Transform _playerTransform;
    protected Rigidbody2D _rb;
    protected SpriteRenderer _headSr;
    protected SpriteRenderer _bodySr;
    protected EnemyHealth _enemyHealth;
    protected Animator _anim;
    protected bool _canMove = false;
    protected bool _isKnockback = false; // Track knockback state
    protected Vector2 _smoothedFlockingForce = Vector2.zero; // Store the smoothed force

    public static List<EnemyMovement> AllEnemies = new List<EnemyMovement>();

    protected virtual void Awake()
    {
        InitializeComponents();
        RandomizeSpeed();
    }

    protected virtual void Start()
    {
        StartCoroutine(StartDelay());
    }

    protected virtual void OnEnable()
    {
        AllEnemies.Add(this);
    }

    protected virtual void OnDisable()
    {
        AllEnemies.Remove(this);
    }

    protected virtual void FixedUpdate()
    {
        if (_enemyHealth.IsDowned || !_canMove || _isKnockback) return;

        if (_enableFlocking)
        {
            ApplyFlockingBehavior();
        }
        else
        {
            MoveTowardsPlayer();
        }
    }

    protected virtual void InitializeComponents()
    {
        Transform spriteHolder = transform.Find("SpriteHolder");

        _playerTransform = GameObject.FindWithTag("Player").transform;

        _rb = GetComponent<Rigidbody2D>();
        _enemyHealth = GetComponent<EnemyHealth>();
        _headSr = spriteHolder?.Find("Head")?.GetComponent<SpriteRenderer>() ?? spriteHolder?.Find("EnemySprite")?.GetComponent<SpriteRenderer>();
        _bodySr = spriteHolder?.Find("Body")?.GetComponent<SpriteRenderer>() ?? spriteHolder?.Find("EnemySprite")?.GetComponent<SpriteRenderer>();
        _anim = spriteHolder?.GetComponent<Animator>();
    }

    protected virtual void RandomizeSpeed()
    {
        _speed = Random.Range(_speed, _speed + 1.5f);
    }

    protected virtual void MoveTowardsPlayer()
    {
        Vector2 directionTowardsTarget = (_playerTransform.position - transform.position).normalized;

        // Flip both Head and Body based on movement direction
        _headSr.flipX = directionTowardsTarget.x < 0;
        _bodySr.flipX = directionTowardsTarget.x < 0;

        _rb.MovePosition(_rb.position + directionTowardsTarget * _speed * Time.fixedDeltaTime);
    }

    protected virtual IEnumerator StartDelay()
    {
        _canMove = false;
        yield return new WaitForSeconds(_startDelay);
        _canMove = true;
        _anim?.Play("Sway", 0, Random.value);
    }

    public virtual void Knockback(Vector2 force, float duration)
    {
        StartCoroutine(KnockbackStart(force, duration));
    }

    protected virtual IEnumerator KnockbackStart(Vector2 force, float duration)
    {
        _isKnockback = true;
        _rb.AddForce(force * _knockbackMult, ForceMode2D.Impulse);

        yield return new WaitForSeconds(duration);

        _isKnockback = false;
    }
    private void ApplyFlockingBehavior()
    {
        Vector2 alignment = Vector2.zero;
        Vector2 cohesion = Vector2.zero;
        Vector2 separation = Vector2.zero;

        // Limit the number of objects checked
        const int maxNearbyEnemies = 3; // Adjust this value as needed
        Collider2D[] nearbyEnemies = new Collider2D[maxNearbyEnemies];
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, _flockRadius, nearbyEnemies);

        int validCount = 0;

        for (int i = 0; i < count; i++)
        {
            var enemy = nearbyEnemies[i];
            if (enemy.gameObject != gameObject && enemy.gameObject.layer == LayerMask.NameToLayer("Enemy") && enemy.TryGetComponent(out EnemyMovement otherEnemy))
            {
                if (otherEnemy._enemyHealth.IsDowned || otherEnemy._isKnockback)
                    continue;

                Vector2 toOther = (Vector2)enemy.transform.position - (Vector2)transform.position;

                alignment += otherEnemy._rb.velocity;
                cohesion += (Vector2)enemy.transform.position;
                separation -= toOther.normalized / Mathf.Max(toOther.magnitude, 0.1f);

                validCount++;
            }
        }

        if (validCount > 0)
        {
            alignment /= validCount;
            cohesion = (cohesion / validCount - (Vector2)transform.position).normalized;
            separation /= validCount;
        }

        Vector2 directionTowardsPlayer = (_playerTransform.position - transform.position).normalized;

        Vector2 rawFlockingForce = (alignment * _alignmentWeight + cohesion * _cohesionWeight + separation * _separationWeight + directionTowardsPlayer).normalized;

        // Check for walls in the movement direction
        Collider2D wall = Physics2D.OverlapCircle(transform.position, _flockRadius / 3, LayerMask.GetMask("Wall"));
        if (wall != null)
        {
            // Rotate the direction by 90 degrees
            rawFlockingForce = new Vector2(-rawFlockingForce.y, rawFlockingForce.x).normalized;
        }

        _smoothedFlockingForce = Vector2.Lerp(_smoothedFlockingForce, rawFlockingForce, 0.1f);

        _rb.MovePosition(_rb.position + _smoothedFlockingForce * _speed * Time.fixedDeltaTime);

        // Flip both Head and Body based on flocking direction
        _headSr.flipX = _smoothedFlockingForce.x < 0;
        _bodySr.flipX = _smoothedFlockingForce.x < 0;
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HandlePlayerCollision(other);
        }
    }

    protected virtual void HandlePlayerCollision(Collider2D other)
    {
        if (other.GetComponent<PlayerHealth>() != null && !_enemyHealth.IsDowned)
        {
            Vector2 directionTowardsTarget = (_playerTransform.position - transform.position).normalized;
            other.GetComponent<PlayerHealth>().TakeDamage(directionTowardsTarget, 1);
        }
    }
}