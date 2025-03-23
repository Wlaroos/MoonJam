using System;
using UnityEngine;

public class TwelveGPuck : WeaponBase
{
    private bool _resetRotationDone = false;
    public override void Shoot(Vector3 aimDirection)
    {
        if (Time.time < _lastFireTime + FireDelay) return;

        if (CanShoot())
        {
            _currentMagAmmo--;

            Vector3 spawnPosition = shootTransform.position;
            float baseAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

            int bulletCount = 3; // Number of bullets in the spread
            float spreadAngle = 120f; // Total spread angle in degrees

            // Adjust spread angle based on missing ammo in the mag
            int missingAmmo = MaxMagSize - _currentMagAmmo;
            switch (missingAmmo)
            {
                case 1:
                    spreadAngle = 120f;
                    break;
                case 2:
                    spreadAngle = 60f;
                    break;
                case 3:
                    spreadAngle = 30f;
                    break;
                default:
                    spreadAngle = 5f;
                    break;
            }

            for (int i = 0; i < bulletCount; i++)
            {
                // Calculate the angle for each bullet in the spread
                float angleOffset = Mathf.Lerp(-spreadAngle / 2, spreadAngle / 2, (float)i / (bulletCount - 1));
                float bulletAngle = baseAngle + angleOffset;

                // Calculate the direction for the bullet
                Vector3 bulletDirection = new Vector3(Mathf.Cos(bulletAngle * Mathf.Deg2Rad), Mathf.Sin(bulletAngle * Mathf.Deg2Rad), 0);

                // Instantiate the bullet and set its direction
                BulletBase bullet = Instantiate(_bulletPrefab, spawnPosition, Quaternion.Euler(0, 0, bulletAngle)).GetComponent<BulletBase>();

                if (bullet != null)
                {
                    bullet.BulletSetup(bulletDirection, bulletAngle, _bulletSpeed, _bulletDamage, _bulletKnockback, _bulletSize, _bulletLifetime);
                }
            }

            _lastFireTime = Time.time;
            OnFired();
        }
    }

    public override void Reload()
    {
        if (IsReloading || _currentMagAmmo == MaxMagSize || CurrentAmmo <= 0) return;

        base.Reload(); // Start the reload coroutine in the base class.
    }

    public override void Aim(Vector3 targetPosition)
    {
        base.Aim(targetPosition); // Always aim normally; rotation is handled in Update.
    }

    void Update()
    {
        if (IsReloading)
        {
            float scale = Mathf.Clamp(_gunSprite.localScale.y, -1f, 1f);
            float angle = scale * 90f;
            _gunSprite.localRotation = Quaternion.Euler(0, 0, -angle);
            _resetRotationDone = false;
        }
        else if (!_resetRotationDone)
        {
            _gunSprite.localRotation = Quaternion.identity;
            _resetRotationDone = true;
        }
    }
}
