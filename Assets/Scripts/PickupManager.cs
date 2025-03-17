using System.Collections.Generic;
using UnityEngine;

public class PickupManager : MonoBehaviour
{
    [SerializeField] private float _pickupRadius = 1f; // Radius to check for nearby items.

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPickupConsumable();
        }
    }

    private void TryPickupConsumable()
    {
        ConsumablePickup consumableToPickup = FindNearestConsumable();

        if (consumableToPickup != null)
        {
            PickupConsumable(consumableToPickup);
        }
    }

    private ConsumablePickup FindNearestConsumable()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _pickupRadius);

        foreach (Collider2D col in colliders)
        {
            ConsumablePickup consumable = col.GetComponent<ConsumablePickup>();
            if (consumable != null)
            {
                return consumable;
            }
        }

        return null;
    }

    private void PickupConsumable(ConsumablePickup consumable)
    {
        // Pick up the consumable.
        consumable.PickedUp();

        // Notify the PlayerWeaponManager to disable weapon dropping temporarily.
        PlayerWeaponManager weaponManager = FindObjectOfType<PlayerWeaponManager>();
        weaponManager?.DisableWeaponDropTemporarily();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _pickupRadius);
    }
}