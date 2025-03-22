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
    [SerializeField] private GameObject _deathBloodParticles;
    [SerializeField] private GameObject _deathChunkParticles;
    [SerializeField] private float _knockbackDuration = 0.25f;

    public UnityEvent OnEnemyDowned;
    public UnityEvent OnEnemyDeath;

    private int _currentHealth;
    private int _currentDownHealth;
    private Rigidbody2D _rb;
    private CapsuleCollider2D _cc;
    private SpriteRenderer _headSr;
    private SpriteRenderer _bodySr;
    private Animator _anim;
    private EnemyMovement _enemyMovement;
    private int _spriteIndex;
    private bool _isDowned = false;
    public bool IsDowned => _isDowned;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _cc = GetComponent<CapsuleCollider2D>();
        _enemyMovement = GetComponent<EnemyMovement>();

        Transform spriteHolder = transform.Find("SpriteHolder");
        _headSr = spriteHolder?.Find("Head")?.GetComponent<SpriteRenderer>() ?? spriteHolder?.Find("EnemySprite")?.GetComponent<SpriteRenderer>();
        _bodySr = spriteHolder?.Find("Body")?.GetComponent<SpriteRenderer>() ?? spriteHolder?.Find("EnemySprite")?.GetComponent<SpriteRenderer>();
        _anim = spriteHolder?.GetComponent<Animator>(); // Animator for Head

        _currentHealth = _maxHealth;
        _currentDownHealth = _maxDownedHealth;
        _anim.SetBool("isMoving", true);
    }

    public void TakeDamage(Vector2 force, int damage)
    {
        StopAllCoroutines();
        StartCoroutine(FlashRed());

        if (!_isDowned)
        {
            // Apply knockback and flash red
            _enemyMovement.Knockback(force, _knockbackDuration);

            _currentHealth -= damage;
            if (_currentHealth <= 0)
            {
                Downed();
            }
        }
        else
        {
            // Weaker knockback in downed state
            _enemyMovement.Knockback(force / 2, _knockbackDuration);

            _currentDownHealth -= damage;
            if (_currentDownHealth <= 0)
            {
                Death();
            }
        }
        //SFXManager.Instance.PlayEnemyHitSFX();
    }

    private IEnumerator FlashRed()
    {
        _headSr.color = Color.red;
        _bodySr.color = Color.red;
        yield return new WaitForSeconds(0.2f);

        if(_isDowned)
        {
            _headSr.color = Color.grey;
            _bodySr.color = Color.grey;
        }
        else
        {
            _headSr.color = Color.white;
            _bodySr.color = Color.white;
        }
    }

    private void Downed()
    {
        StopAllCoroutines();

        _isDowned = true;

        _headSr.color = Color.grey;
        _bodySr.color = Color.grey;

        transform.rotation = Quaternion.Euler(0, 0, 90);

        _anim.SetBool("isMoving", false);

        gameObject.layer = 11;
        _headSr.sortingOrder = -1;
        _bodySr.sortingOrder = -1;

        OnEnemyDowned?.Invoke();

        // Start a coroutine to automatically kill the enemy after 3 seconds
        StartCoroutine(AutoDeathAfterDowned());

        //SFXManager.Instance.PlayEnemyDownSFX();
    }

    private IEnumerator AutoDeathAfterDowned()
    {
        yield return new WaitForSeconds(8f);
        Death();
    }

    private void Death()
    {
        Instantiate(_deathBloodParticles, transform.position, Quaternion.identity);
        Instantiate(_deathChunkParticles, transform.position, Quaternion.identity);

        _cc.enabled = false;

        OnEnemyDeath?.Invoke();

        DestroyEnemy();
        //StartCoroutine(StaticCoroutines.Fade(0.5f, _sr, DestroyEnemy));
        //SFXManager.Instance.PlayEnemyDownSFX();
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);

    }
}