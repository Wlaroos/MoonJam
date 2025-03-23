using System;
using UnityEngine;

public class Kolibri : WeaponBase
{
    private bool _resetRotationDone = false;
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
            // Rotate the gun
            _gunSprite.Rotate(0, 0, 1750 * Time.deltaTime);
            _resetRotationDone = false;
        }
        else if (!_resetRotationDone)
        {
            // Reset the gun's rotation and position
            _gunSprite.localRotation = Quaternion.identity;
            _resetRotationDone = true;
        }
    }
}
