using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PickupManager : MonoBehaviour
{
    [SerializeField] private float _pickupRadius = 1f; // Radius to check for nearby items.

    private void Awake()
    {

    }

    private void Update()
    {
        HandleConsumablePickup();
    }

    private void HandleConsumablePickup()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _pickupRadius);
            ConsumablePickup consumableToPickup = null;

            foreach (Collider2D col in colliders)
            {
                ConsumablePickup consumable = col.GetComponent<ConsumablePickup>();
                if (consumable != null)
                {
                    consumableToPickup = consumable;
                    break;
                }
            }

            if (consumableToPickup != null)
            {
                // Pick up the consumable.
                consumableToPickup.PickedUp();

                // Prevent weapon dropping by notifying the PlayerWeaponManager.
                PlayerWeaponManager weaponManager = FindObjectOfType<PlayerWeaponManager>();
                if (weaponManager != null)
                {
                    weaponManager.DisableWeaponDropTemporarily();
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _pickupRadius);
    }
}
