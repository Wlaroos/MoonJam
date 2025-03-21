using UnityEngine;
using System;

public abstract class WeaponBase : MonoBehaviour
{
    public event Action Fired = delegate { };

    [Header("Weapon Properties")]
    [SerializeField] private string weaponName = "Default Weapon";
    public string WeaponName => weaponName;

    [SerializeField] private Sprite _weaponSprite; // Sprite for the weapon
    public Sprite WeaponSprite => _weaponSprite;

    public bool IsEquipped { get; private set; } = false;
    public Transform Owner { get; private set; }
    [SerializeField] private Transform shootTransform;

    [Header("Reload and Ammo Properties")]
    [SerializeField] private int _maxAmmo = 100; // Total ammo the weapon can hold.
    [SerializeField] private int _maxMagSize = 10; // Ammo capacity of the magazine.
    [SerializeField] private float _reloadTime = 2f; // Time it takes to reload.
    public float ReloadTime => _reloadTime;

    private int _currentAmmo; // Total ammo left.
    private int _currentMagAmmo; // Ammo left in the magazine.

    public int MaxAmmo => _maxAmmo;
    public int MaxMagSize => _maxMagSize;
    public int CurrentAmmo { get => _currentAmmo; set => _currentAmmo = value; }
    public int CurrentMagAmmo { get => _currentMagAmmo; set => _currentMagAmmo = value; }

    private bool _isReloading = false;
    public bool IsReloading { get => _isReloading; set => _isReloading = value; }

    [Header("Fire Type")]
    [SerializeField] private bool _isAutomatic = false;
    public bool IsAutomatic => _isAutomatic;

    [Tooltip("Bullets per second")]
    [SerializeField] private float _fireRate = 4f; // Default to 4 bullets per second.
    private float FireDelay => 1f / _fireRate; // Calculate delay dynamically.
    private float _lastFireTime;

    [Header("Bullet Properties")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private GameObject _bulletParticlePrefab;
    [SerializeField] private float _bulletSize = 1f;
    [SerializeField] private float _bulletSpeed = 20f;
    [SerializeField] private int _bulletDamage = 1;
    [SerializeField] private float _bulletKnockback = 1f;
    [SerializeField] private float _bulletLifetime = 3f;

    private Animator _anim;
    private SpriteRenderer _sr;
    private Rigidbody2D _rb;
    private Collider2D _col;

    private void Awake()
    {
        _currentAmmo = _maxAmmo;
        _currentMagAmmo = _maxMagSize;

        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _anim = GetComponentInChildren<Animator>();
        _sr = GetComponentInChildren<SpriteRenderer>();
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
        Transform gunSprite = transform.Find("GunSprite");
        if (gunSprite != null)
        {
            gunSprite.localRotation = Quaternion.identity;
            gunSprite.GetComponent<SpriteRenderer>().sortingOrder = 5;
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
        Transform gunSprite = transform.Find("GunSprite");
        if (gunSprite != null)
        {
            gunSprite.localRotation = Quaternion.identity; // Keep the sprite's local rotation consistent
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
    Transform gunSprite = transform.Find("GunSprite");
    if (gunSprite != null)
    {
        Vector3 localScale = gunSprite.localScale;
        localScale.y = Mathf.Abs(localScale.y) * ((angle > 90 || angle < -90) ? -1f : 1f);
        gunSprite.localScale = localScale;
    }
}

    public virtual void Shoot(Vector3 aimDirection)
    {
        if (Time.time < _lastFireTime + FireDelay) return;

        if (CanShoot())
        {
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
            Fired?.Invoke();
        }
    }

    public void Reload()
    {
        if (_isReloading || _currentAmmo <= 0 || _currentMagAmmo == _maxMagSize) return;

        _isReloading = true;

        int ammoNeeded = _maxMagSize - _currentMagAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, _currentAmmo);

        _currentMagAmmo += ammoToReload;
        _currentAmmo -= ammoToReload;
    }

    public void SetCurrentAmmo(int ammo)
    {
        _currentAmmo = Mathf.Clamp(ammo, 0, _maxAmmo);
    }
}