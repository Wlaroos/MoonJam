using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class PlayerWeaponManager : MonoBehaviour
{
    [SerializeField] private Transform weaponHold; // The transform where the weapon will be parented when picked up.
    [SerializeField] private float pickupRadius = 1f;  // Radius to check for nearby weapons.
    private TextMeshPro pickupText;

    public UnityEvent<int> AmmoChangeEvent = new UnityEvent<int>(); // Event for ammo changes
    public UnityEvent<WeaponBase> WeaponChangeEvent = new UnityEvent<WeaponBase>(); // Event for weapon changes

    private WeaponBase _currentWeapon;
    private List<WeaponBase> _weapons = new List<WeaponBase>();

    public int MaxMagAmmo => _currentWeapon != null ? _currentWeapon.MaxMagSize : 0;
    public int CurrentAmmo => _currentWeapon != null ? _currentWeapon.CurrentAmmo : 0;

    private Camera mainCamera;

    private float globalAmmoPercentage = 1f; // Global ammo percentage (1.0 = 100%).

    public float GlobalAmmoPercentage => globalAmmoPercentage;

    private PlayerUI playerUI; // Reference to PlayerUI for updating the ammo bar.

    private void Awake()
    {
        mainCamera = Camera.main;
        // Find the child object named "PickupText" and get its TMP_Text component.
        Transform pickupTextTransform = transform.Find("PickupText");
        if (pickupTextTransform != null)
        {
            pickupText = pickupTextTransform.GetComponent<TextMeshPro>();
        }

        playerUI = FindObjectOfType<PlayerUI>(); // Find the PlayerUI in the scene.
        if (playerUI == null)
        {
            Debug.LogWarning("PlayerUI component not found in the scene. Ammo bar will not be updated.");
        }
    }

    private void Update()
    {
        UpdatePickupText();
        HandleWeaponPickupAndDrop();
        HandleAimingAndShooting();
        HandleReloading();
        UpdateAmmoUI();
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
                if (_currentWeapon != null)
                {
                    _currentWeapon.Drop();
                }

                // Pickup the new weapon and apply global ammo percentage.
                weaponToPickup.Pickup(weaponHold);
                int newAmmo = Mathf.FloorToInt(globalAmmoPercentage * weaponToPickup.MaxAmmo);
                weaponToPickup.SetCurrentAmmo(newAmmo);

                _currentWeapon = weaponToPickup;
                WeaponChangeEvent.Invoke(_currentWeapon);
                AmmoChangeEvent.Invoke(_currentWeapon.CurrentAmmo);
            }
            else if (_currentWeapon != null)
            {
                // If no weapon is nearby, drop the current weapon.
                _currentWeapon.Drop();
                _currentWeapon = null;
                WeaponChangeEvent.Invoke(_currentWeapon);
                AmmoChangeEvent.Invoke(0);
            }
        }
    }
    
    // Aims the equipped weapon toward the mouse and fires when the appropriate shoot button is pressed.
    private void HandleAimingAndShooting()
    {
        if (_currentWeapon == null) return;

        // Convert mouse position to world space.
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        // Use the base Aim method.
        _currentWeapon.Aim(mousePosition);

        // Determine input type based on whether the weapon is automatic.
        bool canShoot = false;
        TestGun testGun = _currentWeapon as TestGun;
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
            Vector3 aimDirection = (mousePosition - _currentWeapon.transform.position).normalized;
            _currentWeapon.Shoot(aimDirection);
            UpdateAmmoUI();
        }
    }

    private void HandleReloading()
    {
        if (_currentWeapon != null && Input.GetKeyDown(KeyCode.R) && !_currentWeapon.IsReloading)
        {
            int ammoNeeded = _currentWeapon.MaxMagSize - _currentWeapon.CurrentMagAmmo;
            float availableAmmo = globalAmmoPercentage * _currentWeapon.MaxAmmo;

            if (ammoNeeded > 0 && availableAmmo > 0)
            {
                int ammoToReload = Mathf.Min(ammoNeeded, Mathf.FloorToInt(availableAmmo));
                Debug.Log("Reloading " + ammoToReload + " ammo.");
                _currentWeapon.Reload(ammoToReload);
                globalAmmoPercentage -= (float)ammoToReload / _currentWeapon.MaxAmmo;
                globalAmmoPercentage = Mathf.Clamp01(globalAmmoPercentage); // Ensure it stays between 0 and 1.
                globalAmmoPercentage = Mathf.Round(globalAmmoPercentage * 100f) / 100f; // Round to 2 decimals.
                UpdateAmmoUI(); // Update the ammo bar after reloading.
            }
            else
            {
                Debug.Log("Not enough ammo to reload.");
            }
        }
    }

    private void UpdateAmmoUI()
    {
        // Notify PlayerUI of ammo changes via events.
        if (_currentWeapon != null)
        {
            AmmoChangeEvent.Invoke(_currentWeapon.CurrentAmmo);

            int currentMagAmmo = _currentWeapon.CurrentMagAmmo;
            playerUI.UpdateAmmoBar(currentMagAmmo); // Pass the current magazine ammo to PlayerUI.
        }
        else
        {
            AmmoChangeEvent.Invoke(0);
        }
    }

    public void AddWeapon(WeaponBase weapon)
    {
        _weapons.Add(weapon);
        if (_currentWeapon == null)
        {
            EquipWeapon(weapon);
        }
    }

    public void EquipWeapon(WeaponBase weapon)
    {
        _currentWeapon = weapon;
        WeaponChangeEvent.Invoke(_currentWeapon);
        AmmoChangeEvent.Invoke(_currentWeapon.CurrentAmmo);
    }

    // (Optional) Visualize the pickup radius in the editor.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
