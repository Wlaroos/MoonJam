using System.Collections;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float _speed = 50f;
    [SerializeField] private float _startDelay = 1f;

    private Transform _playerTransform;
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private EnemyHealth _enemyHealth;
    private Vector2 _direction;
    private bool _canMove = false;
    private bool _isKnockback = false; // Track knockback state

    private void Awake()
    {
        // Use tag lookup to find the player (ensure your player GameObject has the "Player" tag)
        _playerTransform = GameObject.FindWithTag("Player").transform;
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponentInChildren<SpriteRenderer>();
        _enemyHealth = GetComponent<EnemyHealth>();
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
            // Calculate direction and move
            Vector2 directionTowardsTarget = (_playerTransform.position - transform.position).normalized;
            _sr.flipX = directionTowardsTarget.x < 0;
            _rb.MovePosition(_rb.position + directionTowardsTarget * _speed * Time.fixedDeltaTime);
        }
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
}
