using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;

public class PlayerWeaponManager : MonoBehaviour
{
    [SerializeField] private Transform _weaponHolder; // The transform where the weapon will be parented when picked up.
    [SerializeField] private float _pickupRadius = 1f;  // Radius to check for nearby weapons.
    private TextMeshPro _pickupText;

    public UnityEvent<int> AmmoChangeEvent = new UnityEvent<int>(); // Event for ammo changes
    public UnityEvent<WeaponBase> WeaponChangeEvent = new UnityEvent<WeaponBase>(); // Event for weapon changes

    private WeaponBase _currentWeapon;

    public int MaxMagAmmo => _currentWeapon != null ? _currentWeapon.MaxMagSize : 0;
    public int CurrentAmmo => _currentWeapon != null ? _currentWeapon.CurrentAmmo : 0;

    private Camera _mainCamera;

    private float _globalAmmoPercentage = 1f; // Global ammo percentage (1.0 = 100%).

    public float GlobalAmmoPercentage => _globalAmmoPercentage;

    private PlayerUI _playerUI; // Reference to PlayerUI for updating the ammo bar.

    [SerializeField] private Transform _reloadBar; // The Transform of the ReloadBar sprite.

    private Coroutine _reloadCoroutine; // To track the reload coroutine.

    private bool _canDropWeapon = true;

    private void Awake()
    {
        _mainCamera = Camera.main;
        // Find the child object named "PickupText" and get its TMP_Text component.
        Transform pickupTextTransform = transform.Find("PickupText");
        if (pickupTextTransform != null)
        {
            _pickupText = pickupTextTransform.GetComponent<TextMeshPro>();
        }

        _playerUI = FindObjectOfType<PlayerUI>(); // Find the PlayerUI in the scene.
        if (_playerUI == null)
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

    private void UpdatePickupText()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _pickupRadius);
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

        if (_pickupText != null)
        {
            if (weaponInRange != null)
                _pickupText.text = "[E] Swap to " + weaponInRange.WeaponName;
            else
                _pickupText.text = "";
        }
    }

    private void HandleWeaponPickupAndDrop()
    {
        if (!_canDropWeapon) return; // Prevent weapon dropping if disabled.

        if (Input.GetKeyDown(KeyCode.E))
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _pickupRadius);
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
                if (_currentWeapon != null)
                {
                    _currentWeapon.Drop();
                }

                weaponToPickup.Pickup(_weaponHolder);
                int newAmmo = Mathf.FloorToInt(_globalAmmoPercentage * weaponToPickup.MaxAmmo);
                weaponToPickup.SetCurrentAmmo(newAmmo);

                _currentWeapon = weaponToPickup;
                WeaponChangeEvent.Invoke(_currentWeapon);
                AmmoChangeEvent.Invoke(_currentWeapon.CurrentAmmo);
            }
            else if (_currentWeapon != null)
            {
                _currentWeapon.Drop();
                _currentWeapon = null;
                WeaponChangeEvent.Invoke(_currentWeapon);
                AmmoChangeEvent.Invoke(0);
            }
        }
    }

    private void HandleAimingAndShooting()
    {
        if (_currentWeapon == null) return;

        Vector3 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        _currentWeapon.Aim(mousePosition);

        bool canShoot = false;
        TestGun testGun = _currentWeapon as TestGun;
        if (testGun != null)
        {
            canShoot = testGun.IsAutomatic ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);
        }
        else
        {
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
            float availableAmmo = _globalAmmoPercentage * _currentWeapon.MaxAmmo;
            int ammoToReload = Mathf.Min(ammoNeeded, Mathf.FloorToInt(availableAmmo));

            if (ammoNeeded > 0 && availableAmmo > 0 && ammoToReload > 0)
            {
                
                Debug.Log("Reloading " + ammoToReload + " ammo.");
                UpdateAmmoUI(); // Update the ammo bar after reloading.


                if (_reloadCoroutine != null)
                {
                    StopCoroutine(_reloadCoroutine);
                }
                _reloadCoroutine = StartCoroutine(ReloadWithBar(_currentWeapon.ReloadTime, ammoToReload));
            }
            else
            {
                Debug.Log("Not enough ammo to reload.");
            }
        }
    }

    private IEnumerator ReloadWithBar(float reloadTime, int ammoToReload)
    {
        if (_reloadBar != null)
        {
            _reloadBar.localScale = new Vector3(0, _reloadBar.localScale.y, _reloadBar.localScale.z); // Reset bar.
            _reloadBar.gameObject.SetActive(true); // Show the bar.
        }

        float elapsedTime = 0f;

        while (elapsedTime < reloadTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / reloadTime;

            if (_reloadBar != null)
            {
                // Scale the bar from the middle.
                _reloadBar.localScale = new Vector3(progress, _reloadBar.localScale.y, _reloadBar.localScale.z);
            }

            yield return null;
        }

        if (_reloadBar != null)
        {
            _reloadBar.localScale = new Vector3(1, _reloadBar.localScale.y, _reloadBar.localScale.z); // Ensure bar is full.
            _reloadBar.gameObject.SetActive(false); // Hide the bar after reload.
        }

        _globalAmmoPercentage -= (float)ammoToReload / _currentWeapon.MaxAmmo;
        _globalAmmoPercentage = Mathf.Clamp01(_globalAmmoPercentage); // Ensure it stays between 0 and 1.
        _globalAmmoPercentage = Mathf.Round(_globalAmmoPercentage * 100f) / 100f; // Round to 2 decimals.

        // Complete the reload process.
        _currentWeapon.Reload();
        UpdateAmmoUI(); // Update the ammo UI after reloading.
    }

    private void UpdateAmmoUI()
    {
        if (_currentWeapon != null)
        {
            AmmoChangeEvent.Invoke(_currentWeapon.CurrentAmmo);

            int currentMagAmmo = _currentWeapon.CurrentMagAmmo;
            _playerUI.UpdateAmmoBar(currentMagAmmo);
        }
        else
        {
            AmmoChangeEvent.Invoke(0);
        }
    }

    public void EquipWeapon(WeaponBase weapon)
    {
        _currentWeapon = weapon;
        WeaponChangeEvent.Invoke(_currentWeapon);
        AmmoChangeEvent.Invoke(_currentWeapon.CurrentAmmo);
    }

    public void AddAmmo(int percentAmount)
    {
        // Convert the percentage amount to a decimal and add it to _globalAmmoPercentage.
        _globalAmmoPercentage += percentAmount / 100f;

        // Clamp the value to ensure it stays between 0 and 1.
        _globalAmmoPercentage = Mathf.Clamp01(_globalAmmoPercentage);

        // If there's a current weapon, update its ammo based on the new global ammo percentage.
        if (_currentWeapon != null)
        {
            int newAmmo = Mathf.FloorToInt(_globalAmmoPercentage * _currentWeapon.MaxAmmo);
            _currentWeapon.SetCurrentAmmo(newAmmo);
            UpdateAmmoUI(); // Update the ammo UI to reflect the new ammo count.
            AmmoChangeEvent.Invoke(_currentWeapon.CurrentAmmo);
        }
    }

    public void DisableWeaponDropTemporarily()
    {
        _canDropWeapon = false;
        StartCoroutine(ReenableWeaponDrop());
    }

    private IEnumerator ReenableWeaponDrop()
    {
        yield return new WaitForSeconds(1f); // Adjust the duration as needed.
        _canDropWeapon = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _pickupRadius);
    }
}
