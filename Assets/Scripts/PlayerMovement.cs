using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("Sprites")]
    [SerializeField] private Sprite[] _sprites;

    private Animator _anim;
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private PlayerHealth _ph;

    private Vector2 _moveDirection;
    private Vector3 _mousePos;
    private bool _isKnockback;

    private void Awake()
    {
        // Cache component references
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponentInChildren<SpriteRenderer>();
        _anim = GetComponentInChildren<Animator>();
        _ph = GetComponent<PlayerHealth>();
    }

    private void OnEnable()
    {
        // Subscribe to PlayerDeathEvent
        if (_ph != null)
        {
            _ph.PlayerDeathEvent.AddListener(PlayerDowned);
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from PlayerDeathEvent
        if (_ph != null)
        {
            _ph.PlayerDeathEvent.RemoveListener(PlayerDowned);
        }
    }

    private void Update()
    {
        if (_ph.IsDowned || _isKnockback) return;

        HandleMovementInput();
        Aim();
    }

    private void FixedUpdate()
    {
        if (_ph.IsDowned) return;

        HandleMovement();
        UpdateAnimation();
    }

    private void HandleMovementInput()
    {
        // Get movement input
        _moveDirection.x = Input.GetAxisRaw("Horizontal");
        _moveDirection.y = Input.GetAxisRaw("Vertical");
    }

    private void HandleMovement()
    {
        // Move the player
        _rb.MovePosition(_rb.position + _moveDirection * _moveSpeed * Time.fixedDeltaTime);
    }

    private void UpdateAnimation()
    {
        // Update movement animation
        _anim.SetBool("isMoving", _moveDirection != Vector2.zero);
    }

    private void Aim()
    {
        // Convert mouse position to world point
        _mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _mousePos.z = 0f;

        // Calculate aiming direction
        Vector3 aimDir = (_mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

        // Flip sprite based on aiming direction
        _sr.flipX = angle > 90 || angle < -90;
    }

    public void Knockback(Vector2 force, float duration)
    {
        StartCoroutine(KnockbackCoroutine(force, duration));
    }

    private IEnumerator KnockbackCoroutine(Vector2 force, float duration)
    {
        _isKnockback = true;
        _rb.AddForce(force * 10, ForceMode2D.Impulse);
        _sr.color = Color.red;

        yield return new WaitForSeconds(duration);

        _sr.color = Color.magenta;
        _isKnockback = false;
    }

    private void PlayerDowned()
    {
        // Freeze player movement and stop animations
        _rb.constraints = RigidbodyConstraints2D.FreezeAll;
        _anim.SetBool("isMoving", false);
    }
}