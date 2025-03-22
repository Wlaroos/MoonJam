using System;
using UnityEngine;

public class Schofield : WeaponBase
{
    private bool _horizontal;

    public override void Shoot(Vector3 aimDirection)
    {
        if (Time.time < _lastFireTime + FireDelay) return;

        if (CanShoot())
        {
            _currentMagAmmo--;

            Vector3 spawnPosition = shootTransform.position;
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

            for (int i = -1; i <= 1; i++)
            {
                Vector3 offset = _horizontal 
                    ? new Vector3(0, i * 0.5f, 0) // Side by side (horizontal)
                    : new Vector3(i * 0.5f, 0, 0); // In front of each other (vertical)

                Transform bulletInstance = Instantiate(_bulletPrefab, spawnPosition + offset, Quaternion.identity).transform;

                BulletBase bullet = bulletInstance.GetComponent<BulletBase>();
                if (bullet != null)
                {
                    bullet.BulletSetup(aimDirection, angle, _bulletSpeed, _bulletDamage, _bulletKnockback, _bulletSize, _bulletLifetime);
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
        _horizontal = !_horizontal; // Toggle the horizontal/vertical firing mode.
    }

    public override void Aim(Vector3 targetPosition)
    {
        base.Aim(targetPosition); // Always aim normally; rotation is handled in Update.
    }

    private bool _resetRotationDone = false;

    void Update()
    {
        if (IsReloading)
        {
            _gunSprite.Rotate(0, 0, 3000 * Time.deltaTime);
            _resetRotationDone = false;
        }
        else if (!_resetRotationDone)
        {
            _gunSprite.localRotation = Quaternion.identity;
            _resetRotationDone = true;
        }
    }
}
