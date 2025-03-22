using UnityEngine;
using System;
using System.Collections;

public abstract class WeaponBase : MonoBehaviour
{
    public event Action Fired = delegate { };

    [Header("Weapon Properties")]
    [SerializeField] protected string weaponName = "Default Weapon";
    public string WeaponName => weaponName;

    [SerializeField] protected Sprite _weaponSprite; // Sprite for the weapon
    public Sprite WeaponSprite => _weaponSprite;

    public bool IsEquipped { get; private set; } = false;
    public Transform Owner { get; private set; }
    [SerializeField] protected Transform shootTransform;

    [Header("Reload and Ammo Properties")]
    [SerializeField] protected int _maxAmmo = 100; // Total ammo the weapon can hold.
    [SerializeField] protected int _maxMagSize = 10; // Ammo capacity of the magazine.
    [SerializeField] protected float _reloadTime = 2f; // Time it takes to reload.
    public float ReloadTime => _reloadTime;

    protected int _currentAmmo; // Total ammo left.
    protected int _currentMagAmmo; // Ammo left in the magazine.

    public int MaxAmmo => _maxAmmo;
    public int MaxMagSize => _maxMagSize;
    public int CurrentAmmo { get => _currentAmmo; set => _currentAmmo = value; }
    public int CurrentMagAmmo { get => _currentMagAmmo; set => _currentMagAmmo = value; }

    protected bool _isReloading = false;
    public bool IsReloading { get => _isReloading; set => _isReloading = value; }

    [Header("Fire Type")]
    [SerializeField] protected bool _isAutomatic = false;
    public bool IsAutomatic => _isAutomatic;

    [Tooltip("Bullets per second")]
    [SerializeField] protected float _fireRate = 4f; // Default to 4 bullets per second.
    protected float FireDelay => 1f / _fireRate; // Calculate delay dynamically.
    protected float _lastFireTime;

    [Header("Bullet Properties")]
    [SerializeField] protected GameObject _bulletPrefab;
    [SerializeField] protected GameObject _bulletParticlePrefab;
    [SerializeField] protected float _bulletSize = 1f;
    [SerializeField] protected float _bulletSpeed = 20f;
    [SerializeField] protected int _bulletDamage = 1;
    [SerializeField] protected float _bulletKnockback = 1f;
    [SerializeField] protected float _bulletLifetime = 3f;

    protected Animator _anim;
    protected SpriteRenderer _sr;
    protected Rigidbody2D _rb;
    protected Collider2D _col;
    protected Transform _gunSprite;

    private Coroutine _reloadCoroutine; // Track the reload coroutine

    private void Awake()
    {
        _currentAmmo = _maxAmmo;
        _currentMagAmmo = _maxMagSize;

        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _anim = GetComponentInChildren<Animator>();
        _sr = GetComponentInChildren<SpriteRenderer>();
        _gunSprite = transform.Find("GunSprite");
        
        _weaponSprite = _sr.sprite;

        //if (!_rb) Debug.LogWarning($"Rigidbody2D component is missing on {gameObject.name}");
        if (!_col) Debug.LogWarning($"Collider2D component is missing on {gameObject.name}");
        if (!_anim) Debug.LogWarning($"Animator component is missing in children of {gameObject.name}");
        if (!_sr) Debug.LogWarning($"SpriteRenderer component is missing in children of {gameObject.name}");
    }

    public bool CanShoot()
    {
        return _currentMagAmmo > 0 && !_isReloading;
    }

    public virtual void Pickup(Transform newOwner)
    {
        Owner = newOwner;
        IsEquipped = true;

        // Parent the weapon to the new owner (weapon holder)
        transform.SetParent(newOwner);

        // Reset the weapon's local position and rotation relative to the weapon holder
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Disable physics and collision
        if (_rb != null) _rb.isKinematic = true;
        if (_col != null) _col.enabled = false;

        // Ensure the sprite child (GunSprite) has no local rotation
        if (_gunSprite != null)
        {
            _gunSprite.localRotation = Quaternion.identity;
            _gunSprite.GetComponent<SpriteRenderer>().sortingOrder = 5;
        }
    }

    public virtual void Drop()
    {
        IsEquipped = false;
        Owner = null;

        // Unparent the weapon from the weapon holder
        transform.SetParent(null);

        // Re-enable physics and collision
        if (_rb != null)
        {
            _rb.isKinematic = false; // Allow the weapon to interact with physics
        }
        if (_col != null)
        {
            _col.enabled = true; // Enable the collider
        }

        // Reset the weapon's rotation for a natural drop
        float randomAngle = UnityEngine.Random.Range(0f, 360f);
        transform.rotation = Quaternion.Euler(0, 0, randomAngle);

        // Ensure the sprite child (GunSprite) rotates naturally with the weapon
        if (_gunSprite != null)
        {
            _gunSprite.localRotation = Quaternion.identity; // Keep the sprite's local rotation consistent
        }

        // Ensure the weapon is visible and active
        gameObject.SetActive(true);
    }

public virtual void Aim(Vector3 targetPosition)
{
    
    Vector3 aimDirection = (targetPosition - transform.position).normalized;
    float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

    // Rotate the weapon holder to aim at the target
    transform.rotation = Quaternion.Euler(0, 0, angle);

    // Flip the sprite vertically if aiming backward
    if (_gunSprite != null)
    {
        Vector3 localScale = _gunSprite.localScale;
        localScale.y = Mathf.Abs(localScale.y) * ((angle > 90 || angle < -90) ? -1f : 1f);
        _gunSprite.localScale = localScale;
    }
}

    public virtual void Shoot(Vector3 aimDirection)
    {
        if (Time.time < _lastFireTime + FireDelay) return;

        if (CanShoot())
        {
            // Stop reload if currently reloading
            if (_isReloading)
            {
                StopReload();
            }

            _currentMagAmmo--;
            Vector3 spawnPosition = shootTransform.position;
            Transform bulletInstance = Instantiate(_bulletPrefab, spawnPosition, Quaternion.identity).transform;
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

            BulletBase bullet = bulletInstance.GetComponent<BulletBase>();
            if (bullet != null)
            {
                bullet.BulletSetup(aimDirection, angle, _bulletSpeed, _bulletDamage, _bulletKnockback, _bulletSize, _bulletLifetime);
            }

            _lastFireTime = Time.time;
            OnFired();
        }
    }

    public virtual void Reload()
    {
        // Allow reloading if the magazine is not full and there is reserve ammo.
        if (_isReloading || _currentMagAmmo == _maxMagSize || _currentAmmo <= 0) return;

        // Start the reload coroutine and store its reference
        _reloadCoroutine = StartCoroutine(ReloadCoroutine());
    }

    public IEnumerator ReloadCoroutine()
    {
        _isReloading = true; // Set reloading state to true.

        // Simulate reload time.
        yield return new WaitForSeconds(_reloadTime);

        int ammoNeeded = _maxMagSize - _currentMagAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, _currentAmmo);

        _currentMagAmmo += ammoToReload;
        _currentAmmo -= ammoToReload;

        _isReloading = false; // Reset reloading state after reload is complete.
        _reloadCoroutine = null; // Clear the coroutine reference
    }

    public void SetCurrentAmmo(int ammo)
    {
        _currentAmmo = Mathf.Clamp(ammo, 0, _maxAmmo);
    }

    protected void OnFired()
    {
        Fired?.Invoke();
    }

    public void StopReload()
    {
        if (_isReloading)
        {
            // Stop the reload coroutine if it is running
            if (_reloadCoroutine != null)
            {
                StopCoroutine(_reloadCoroutine);
                _reloadCoroutine = null;
            }

            _isReloading = false; // Reset the reloading state
        }
    }
}