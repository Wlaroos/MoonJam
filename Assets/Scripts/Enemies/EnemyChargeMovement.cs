using System.Collections;
using UnityEngine;

public class EnemyChargeMovement : EnemyMovement
{
    [Header("Charge Attack Settings")]
    [SerializeField] private float _chargeSpeed = 10f; // Speed during the charge attack
    [SerializeField] private float _chargeDuration = 1f; // Duration of the charge attack
    [SerializeField] private float _chargeCooldown = 5f; // Cooldown between charge attacks
    [SerializeField] private float _chargeStartDelay = 1f; // Initial delay before the charge starts
    [SerializeField] private float _jumpHeight = 0.5f; // Height of the jump during the delay
    [SerializeField] private int _jumpCount = 3; // Number of jumps during the delay
    [SerializeField] private GameObject _psGround; // Prefab to spawn at the enemy's feet

    private bool _isCharging = false;
    private float _chargeCooldownTimer = 0f;

    protected override void Awake()
    {
        base.Awake();
        _chargeCooldownTimer = _chargeCooldown; // Set initial cooldown timer
    }

    protected override void FixedUpdate()
    {
        if (_enemyHealth.IsDowned || !_canMove || _isKnockback) return;

        // Handle charge cooldown
        if (_chargeCooldownTimer > 0)
        {
            _chargeCooldownTimer -= Time.fixedDeltaTime;
        }

        if (_isCharging)
        {
            // Continue charging
            return;
        }

        if (_chargeCooldownTimer <= 0)
        {
            StartCoroutine(PerformChargeAttack());
        }
        else
        {
            base.FixedUpdate(); // Use base movement logic
        }
    }

    private IEnumerator PerformChargeAttack()
{
    _isCharging = true;

    // Stop movement during the initial delay
    _canMove = false;

    float elapsedTime = 0f;
    Vector3 originalPosition = transform.position;

    // Jump up and down during the delay
    for (int i = 0; i < _jumpCount; i++)
    {
        // Jump up
        float jumpElapsed = 0f;
        while (jumpElapsed < _chargeStartDelay / (_jumpCount * 2))
        {
            jumpElapsed += Time.deltaTime;
            float verticalOffset = Mathf.Lerp(0, _jumpHeight, jumpElapsed / (_chargeStartDelay / (_jumpCount * 2)));
            transform.position = originalPosition + new Vector3(0, verticalOffset, 0);
            yield return null;
        }

        // Jump down
        jumpElapsed = 0f;
        while (jumpElapsed < _chargeStartDelay / (_jumpCount * 2))
        {
            jumpElapsed += Time.deltaTime;
            float verticalOffset = Mathf.Lerp(_jumpHeight, 0, jumpElapsed / (_chargeStartDelay / (_jumpCount * 2)));
            transform.position = originalPosition + new Vector3(0, verticalOffset, 0);
            yield return null;
        }
        Instantiate(_psGround, transform.position, Quaternion.identity);
    }

    // Reset position after the delay
    transform.position = originalPosition;

    // Resume movement for the charge attack
    Vector2 chargeDirection = (_playerTransform.position - transform.position).normalized;

    elapsedTime = 0f;
    while (elapsedTime < _chargeDuration)
    {
        elapsedTime += Time.fixedDeltaTime;

        // Check for collision with the "PlayerWall" layer
        RaycastHit2D hit = Physics2D.Raycast(transform.position, chargeDirection, _chargeSpeed * Time.fixedDeltaTime, LayerMask.GetMask("PlayerWall"));
        if (hit.collider != null)
        {
            // Stop the charge if a collision is detected
            break;
        }

        // Move the enemy in the charge direction
        _rb.MovePosition(_rb.position + chargeDirection * _chargeSpeed * Time.fixedDeltaTime);

        yield return new WaitForFixedUpdate();
    }

    _isCharging = false;
    _chargeCooldownTimer = _chargeCooldown; // Reset cooldown
    _canMove = true; // Allow movement again
}
}