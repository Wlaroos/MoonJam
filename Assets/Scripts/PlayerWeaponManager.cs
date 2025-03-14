using UnityEngine;
using TMPro;

public class PlayerWeaponManager : MonoBehaviour
{
    [SerializeField] private Transform weaponHold; // The transform where the weapon will be parented when picked up.
    [SerializeField] private float pickupRadius = 1f;  // Radius to check for nearby weapons.

    private WeaponBase currentWeapon;
    private Camera mainCamera;
    private TextMeshPro pickupText;

    private void Awake()
    {
        mainCamera = Camera.main;
        // Find the child object named "PickupText" and get its TMP_Text component.
        Transform pickupTextTransform = transform.Find("PickupText");
        if (pickupTextTransform != null)
        {
            pickupText = pickupTextTransform.GetComponent<TextMeshPro>();
        }
    }

    private void Update()
    {
        UpdatePickupText();
        HandleWeaponPickupAndDrop();
        HandleAimingAndShooting();
    }
    
    // Checks for a nearby weapon and updates the pickup prompt text accordingly.
    private void UpdatePickupText()
    {
        // Look for weapons in the pickup radius.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, pickupRadius);
        WeaponBase weaponInRange = null;

        foreach (Collider2D col in colliders)
        {
            WeaponBase weapon = col.GetComponent<WeaponBase>();
            if (weapon != null && !weapon.IsEquipped)
            {
                weaponInRange = weapon;
                break;
            }
        }

        if (pickupText != null)
        {
            if (weaponInRange != null)
                pickupText.text = "[E] Swap to " + weaponInRange.WeaponName;
            else
                pickupText.text = "";
        }
    }
    
    // Checks if the player presses the E key to pick up a nearby weapon or drop the current one.
    private void HandleWeaponPickupAndDrop()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Look for weapons in the pickup radius.
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, pickupRadius);
            WeaponBase weaponToPickup = null;

            foreach (Collider2D col in colliders)
            {
                WeaponBase weapon = col.GetComponent<WeaponBase>();
                if (weapon != null && !weapon.IsEquipped)
                {
                    weaponToPickup = weapon;
                    break;
                }
            }

            if (weaponToPickup != null)
            {
                // Drop current weapon if one exists.
                if (currentWeapon != null)
                {
                    currentWeapon.Drop();
                }

                // Pickup the new weapon.
                weaponToPickup.Pickup(weaponHold);
                currentWeapon = weaponToPickup;
            }
            else if (currentWeapon != null)
            {
                // If no weapon is nearby, drop the current weapon.
                currentWeapon.Drop();
                currentWeapon = null;
            }
        }
    }
    
    // Aims the equipped weapon toward the mouse and fires when the appropriate shoot button is pressed.
    private void HandleAimingAndShooting()
    {
        if (currentWeapon == null) return;

        // Convert mouse position to world space.
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        // Use the base Aim method.
        currentWeapon.Aim(mousePosition);

        // Determine input type based on whether the weapon is automatic.
        bool canShoot = false;
        TestGun testGun = currentWeapon as TestGun;
        if (testGun != null)
        {
            canShoot = testGun.IsAutomatic ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);
        }
        else
        {
            // Default to one shot per click if not a TestGun.
            canShoot = Input.GetMouseButtonDown(0);
        }

        if (canShoot)
        {
            Vector3 aimDirection = (mousePosition - currentWeapon.transform.position).normalized;
            currentWeapon.Shoot(aimDirection);
        }
    }

    // (Optional) Visualize the pickup radius in the editor.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
