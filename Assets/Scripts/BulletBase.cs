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
            _tr.enabled = true; // Show the trail after it has moved 1 unit
            _isVisible = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("BulletBounds") && !_once)
        {
            Destroy();
        }
        else if (collision.GetComponent<EnemyHealth>() != null)
        {
            HandleEnemyCollision(collision);
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
            Instantiate(_psBrainDirectional, hitPoint, Quaternion.Euler(0, 0, angle));
        }

        Destroy();
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