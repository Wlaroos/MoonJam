using System.Collections;
using UnityEngine;

public class BulletBase : MonoBehaviour
{
    [SerializeField] private float _shotSpeed = 5;
    [SerializeField] private int _damage = 1;
    [SerializeField] private float _knockback = 3;
    [SerializeField] private float _size = 1;
    [SerializeField] private float _lifetime = 3;
    [SerializeField] private GameObject _psBloodDirectional;
    [SerializeField] private GameObject _psBloodCircle;
    [SerializeField] private GameObject _psBrainDirectional;

    private Rigidbody2D _rb;
    private bool _once = false;
    private SpriteRenderer _sr;
    private TrailRenderer _tr;
    private Vector3 _startPosition;
    private bool _isVisible = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _tr = GetComponent<TrailRenderer>();
        _sr.enabled = false; // Initially hide the bullet
        _tr.enabled = false; // Initially hide the trail

        _startPosition = transform.position;
    }

    private void Update()
    {
        if(_isVisible) return;
        if (Vector3.Distance(_startPosition, transform.position) >= 0.5f)
        {
            _sr.enabled = true; // Show the bullet after it has moved 1 unit
            //_tr.enabled = true; // Show the trail after it has moved 1 unit
            _isVisible = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("BulletBounds") && !_once)
        {
            Destroy();
        }
        else if (collision.GetComponent<EnemyHealth>() != null && collision.CompareTag("Enemy"))
        {
            HandleEnemyCollision(collision);
        }
        else if (collision.CompareTag("Soul"))
        {
            // The soul collider might be a child object; search upwards for an ancestor that has EnemyHealth
            Collider2D enemyCollider = FindAncestorColliderWithComponent<EnemyHealth>(collision.transform);
            if (enemyCollider != null)
            {
                HandleEnemyCollision(enemyCollider, 3);
            }
            else
            {
                // Fallback: try to find any ancestor collider
                Collider2D fallback = FindAncestorCollider(collision.transform);
                if (fallback != null)
                    HandleEnemyCollision(fallback, 3);
            }
        }
    }

    public void BulletSetup(Vector3 shootDir, float angle, float shotSpeed, int damage, float knockback, float size, float lifetime)
    {
        _shotSpeed = shotSpeed;
        _damage = damage;
        _knockback = knockback;
        _size = size;
        _lifetime = lifetime;

        _rb = GetComponent<Rigidbody2D>();
        transform.localScale = new Vector3(_size, _size, _size);
        transform.eulerAngles = new Vector3(0, 0, angle);

        _rb.AddForce(shootDir * _shotSpeed, ForceMode2D.Impulse);

        StartCoroutine(DelayedDestroy(_lifetime));
    }

    private void HandleEnemyCollision(Collider2D collision)
    {
        collision.GetComponent<EnemyHealth>().TakeDamage(_rb.velocity.normalized * _knockback, _damage);

        Vector3 hitPoint = collision.ClosestPoint(transform.position);
        Vector3 bulletDirection = _rb.velocity.normalized;
        float angle = Mathf.Atan2(bulletDirection.y, bulletDirection.x) * Mathf.Rad2Deg;

        Instantiate(_psBloodDirectional, hitPoint, Quaternion.Euler(0, 0, angle));
        Instantiate(_psBloodCircle, collision.transform.position, Quaternion.identity);

        if (Random.value < 0.2f)
        {
            if (_psBrainDirectional != null)
            {
                Instantiate(_psBrainDirectional, hitPoint, Quaternion.Euler(0, 0, angle));
            }
        }

        Destroy();
    }

        private void HandleEnemyCollision(Collider2D collision, int damageMultiplier)
    {
        collision.GetComponent<EnemyHealth>().TakeDamage(_rb.velocity.normalized * _knockback, _damage * damageMultiplier);

        Vector3 hitPoint = collision.ClosestPoint(transform.position);
        Vector3 bulletDirection = _rb.velocity.normalized;
        float angle = Mathf.Atan2(bulletDirection.y, bulletDirection.x) * Mathf.Rad2Deg;

        Instantiate(_psBloodDirectional, hitPoint, Quaternion.Euler(0, 0, angle));
        Instantiate(_psBloodCircle, collision.transform.position, Quaternion.identity);

        if (Random.value < 0.2f)
        {
            if (_psBrainDirectional != null)
            {
                Instantiate(_psBrainDirectional, hitPoint, Quaternion.Euler(0, 0, angle));
            }
        }

        Destroy();
    }

    // Helper: climb parents and return the first Transform whose GameObject has component T; then return its Collider2D
    private Collider2D FindAncestorColliderWithComponent<T>(Transform start) where T : Component
    {
        Collider2D lastFound = null;
        Transform t = start;
        while (t != null)
        {
            if (t.GetComponent<T>() != null)
            {
                var c = t.GetComponent<Collider2D>();
                if (c != null) lastFound = c;
            }
            t = t.parent;
        }
        return lastFound;
    }

    // Helper: climb parents and return the furthest (top-most) Collider2D found
    private Collider2D FindAncestorCollider(Transform start)
    {
        Collider2D lastFound = null;
        Transform t = start;
        while (t != null)
        {
            var c = t.GetComponent<Collider2D>();
            if (c != null) lastFound = c;
            t = t.parent;
        }
        return lastFound;
    }

    private IEnumerator DelayedDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy();
    }

    private void Destroy()
    {
        if (!_once)
        {
            _once = true;
            StopAllCoroutines();
            Destroy(gameObject);
        }
    }
}