using UnityEngine;
using System;

public abstract class WeaponBase : MonoBehaviour
{
    public event Action Fired = delegate { };

    [Header("Weapon Properties")]
    [SerializeField] private string weaponName = "Default Weapon";
    public string WeaponName => weaponName;

    public bool IsEquipped { get; private set; } = false;
    public Transform Owner { get; private set; }
    [SerializeField] private Transform shootTransform;

    [Header("Fire Type")]
    [SerializeField] private bool isAutomatic = false;
    // Expose isAutomatic via a public property.
    public bool IsAutomatic => isAutomatic;

    // Change fireDelay to fireRate (bullets per second).
    [Tooltip("Bullets per second")]
    [SerializeField] private float fireRate = 4f; // Default to 4 bullets per second.
    private float FireDelay => 1f / fireRate; // Calculate delay dynamically.

    [Header("Bullet Properties")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject bulletParticlePrefab;
    [SerializeField] private float bulletSize = 1f;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private int bulletDamage = 1;
    [SerializeField] private float bulletKnockback = 1f;
    [SerializeField] private float bulletLifetime = 3f;

    private float lastFireTime;
    private Animator _anim;
    private SpriteRenderer _sr;
    private Rigidbody2D _rb;
    
    // Called to pick up the weapon. Re-parents it to the new owner’s hold position.
    public virtual void Pickup(Transform newOwner)
    {
        Owner = newOwner;
        IsEquipped = true;
        transform.SetParent(newOwner);
        
        transform.position = newOwner.position;

        // Optionally disable physics and collider for the equipped weapon.
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        transform.Find("GunSprite").localRotation = Quaternion.Euler(0, 0, 0);
    }

    // Called to drop the weapon back into the world.
    public virtual void Drop()
    {
        IsEquipped = false;
        Owner = null;
        transform.SetParent(null);

        // Optionally re-enable physics and collider.
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true;
        }

        // Set rotation to a random angle when dropped.
        float randomAngle = UnityEngine.Random.Range(0f, 360f);
        transform.Find("GunSprite").localRotation = Quaternion.Euler(0, 0, randomAngle);
    }

    
    // Virtual aiming method that rotates the weapon to face the target position.
    // Can be overridden by derived classes.
    public virtual void Aim(Vector3 targetPosition)
    {
        Vector3 aimDirection = (targetPosition - transform.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Flip sprite based on angle for natural visuals.
        Vector3 localScale = Vector3.one;
        localScale.y = (angle > 90 || angle < -90) ? -1f : 1f;
        transform.localScale = localScale;
    }
    
    // Abstract method to shoot the weapon.
    public virtual void Shoot(Vector3 aimDirection)
    {
        if (Time.time < lastFireTime + FireDelay) return;

        Vector3 spawnPosition = shootTransform.position;
        Transform bulletInstance = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity).transform;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        // Assuming BulletBase has a BulletSetup method that takes these parameters.
        BulletBase bullet = bulletInstance.GetComponent<BulletBase>();
        if (bullet != null)
        {
            bullet.BulletSetup(aimDirection, angle, bulletSpeed, bulletDamage, bulletKnockback, bulletSize, bulletLifetime);
        }

        lastFireTime = Time.time;
        Fired?.Invoke();
    }
}
