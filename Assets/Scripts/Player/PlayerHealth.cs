using System;
using System.Collections;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int _maxHealth = 3;
    [SerializeField] private int _startingHealth = 3;
    public int MaxHealth => _maxHealth;

    private int _currentHealth;
    public int CurrentHealth => _currentHealth;

    [SerializeField] private float _stunTime = 0.25f;
    [SerializeField] private float _iFrameDuration = 1f;

    private SpriteRenderer _sr;

    private bool _isInvincible = false;
    private bool _isDowned = false;
    public bool IsDowned => _isDowned;

    [SerializeField] private Sprite _downedSprite;

    public UnityEvent HealthChangeEvent;
    public UnityEvent PlayerDeathEvent;

    private void Awake()
    {
        _sr = GetComponentInChildren<SpriteRenderer>();
        _currentHealth = _startingHealth;
    }

    private void OnEnable()
    {
        // Add any event subscriptions here
    }

    private void OnDisable()
    {
        // Remove any event subscriptions here
    }

    private void Update()
    {
        // For testing purposes: Press "T" to take 1 damage.
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(Vector2.zero, 1); // Apply 1 damage with no knockback.
        }
    }

    public void TakeDamage(Vector2 force, int damage)
    {
        if (!_isInvincible)
        {
            // SFXManager.Instance.PlayPlayerHurtSFX();

            // Knockback
            GetComponent<PlayerMovement>().Knockback(force, _stunTime);

            // Subtract Health
            _currentHealth -= damage;

            // Check for death, if not, add iFrames
            if (_currentHealth <= 0 && !_isDowned)
            {
                Death();
            }
            else
            {
                StopCoroutine(IFrames(_iFrameDuration));
                StartCoroutine(IFrames(_iFrameDuration));
            }

            HealthChangeEvent.Invoke();
        }
    }

    public void Heal(int amount)
    {
        // SFXManager.Instance.PlayHealSFX();

        _currentHealth += amount;
        if (_currentHealth > _maxHealth)
        {
            _currentHealth = _maxHealth;
        }

        HealthChangeEvent.Invoke();
    }

    private IEnumerator IFrames(float duration)
    {
        _isInvincible = true;
        yield return new WaitForSeconds(duration);
        _sr.color = Color.white;
        _isInvincible = false;
    }

    private void Death()
    {
        // Instantiate(_ps, transform.position, Quaternion.identity);
        // int random = UnityEngine.Random.Range(0, 3);

        // SFXManager.Instance.PlayGameOverSFX();

        _isDowned = true;
        _sr.sortingOrder = -1;
        _sr.sprite = _downedSprite;
        _sr.color = Color.white;

        transform.rotation = Quaternion.Euler(0, 0, 90);

        // MusicManager.Instance.SwapTrack(false);

        PlayerDeathEvent.Invoke();
    }
}