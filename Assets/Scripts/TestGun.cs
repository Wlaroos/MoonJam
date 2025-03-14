using System;
using UnityEngine;

public class TestGun : WeaponBase
{
    public event Action Fired = delegate { };

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform shootTransform;
    
    [SerializeField] private bool isAutomatic = false;
    // Expose isAutomatic via a public property.
    public bool IsAutomatic => isAutomatic;
    
    [SerializeField] private float fireDelay = 0.25f;
    [SerializeField] private float bulletSize = 1f;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private int bulletDamage = 1;
    [SerializeField] private float bulletKnockback = 1f;
    [SerializeField] private float bulletLifetime = 3f;

    private float lastFireTime;
    
    // Implements the shooting logic.
    public override void Shoot(Vector3 aimDirection)
    {
        if (Time.time < lastFireTime + fireDelay) return;

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
