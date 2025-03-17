using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int _maxHealth = 3;
    [SerializeField] private int _maxDownedHealth = 3;

    [Header("References")]
    [SerializeField] private Sprite[] _normalSprites;
    [SerializeField] private Sprite[] _downedSprites;
    [SerializeField] private GameObject _deathBloodParticles;
    [SerializeField] private GameObject _deathChunkParticles;

    [SerializeField] private float _knockbackDuration = 0.25f;

    public UnityEvent OnEnemyDowned;
    public UnityEvent OnEnemyDeath;

    private int _currentHealth;
    private int _currentDownHealth;
    private Rigidbody2D _rb;
    private CapsuleCollider2D _cc;
    private SpriteRenderer _sr;
    private Animator _anim;
    private EnemyMovement _enemyMovement;
    private int _spriteIndex;
    private const float knockbackMultiplier = 1.0f;
    private bool _isDowned = false;
    public bool IsDowned => _isDowned;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _cc = GetComponent<CapsuleCollider2D>();
        _sr = GetComponentInChildren<SpriteRenderer>();
        _anim = GetComponentInChildren<Animator>();
        _enemyMovement = GetComponent<EnemyMovement>();
        _currentHealth = _maxHealth;
        _currentDownHealth = _maxDownedHealth;
        _anim.SetBool("isMoving", true);
        _spriteIndex = Random.Range(0, _normalSprites.Length);
        _sr.sprite = _normalSprites[_spriteIndex];
    }

    public void TakeDamage(Vector2 force, int damage)
    {
        StopAllCoroutines();
        StartCoroutine(FlashRed());

        if (!_isDowned)
        {
            // Apply knockback and flash red
            _enemyMovement.Knockback(force * knockbackMultiplier, _knockbackDuration);

            _currentHealth -= damage;
            if (_currentHealth <= 0)
            {
                Downed();
                // Notify a spawner if present
                OnEnemyDowned?.Invoke();
            }
        }
        else
        {
            // Weaker knockback in downed state
            _enemyMovement.Knockback(force * (knockbackMultiplier / 2), _knockbackDuration);

            _currentDownHealth -= damage;
            if (_currentDownHealth <= 0)
            {
                Death();
                OnEnemyDeath?.Invoke();
            }
        }
        //SFXManager.Instance.PlayEnemyHitSFX();
    }

    private IEnumerator FlashRed()
    {
        _sr.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        _sr.color = Color.white;
    }

    private void Downed()
    {
        StopAllCoroutines();

        _isDowned = true;

        _sr.color = Color.white;
        _sr.sprite = _downedSprites[_spriteIndex];

        transform.rotation = Quaternion.Euler(0, 0, 90);

        _anim.SetBool("isMoving", false);

        gameObject.layer = 11;
        _sr.sortingOrder = -1;

        //SFXManager.Instance.PlayEnemyDownSFX();
    }

    private void Death()
    {
        Instantiate(_deathBloodParticles, transform.position, Quaternion.identity);
        Instantiate(_deathChunkParticles, transform.position, Quaternion.identity);

        _cc.enabled = false;
        DestroyEnemy();
        //StartCoroutine(StaticCoroutines.Fade(0.5f, _sr, DestroyEnemy));
        //SFXManager.Instance.PlayEnemyDownSFX();
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }
}