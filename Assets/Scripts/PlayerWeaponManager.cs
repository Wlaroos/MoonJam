using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private Transform _weaponHolder;
    [SerializeField] private float _pickupRadius = 1f;
    [SerializeField] private WeaponBase _starterWeapon; // Assign this in the Unity Inspector
    public WeaponBase StarterWeapon => _starterWeapon;

    private WeaponBase _secondaryWeapon; // Secondary weapon (picked up weapon)

    [Header("UI References")]
    private TextMeshPro _pickupText;
    private PlayerUI _playerUI;
    private SpriteRenderer _reloadBar;
    private SpriteRenderer _reloadBarBG;

    [Header("Events")]
    public UnityEvent<int> AmmoChangeEvent = new UnityEvent<int>();
    public UnityEvent<WeaponBase> WeaponChangeEvent = new UnityEvent<WeaponBase>();

    private WeaponBase _currentWeapon;
    public WeaponBase CurrentWeapon => _currentWeapon;
    private Camera _mainCamera;
    private Coroutine _reloadCoroutine;
    private bool _canDropWeapon = true;

    private float _globalAmmoPercentage = 1f;

    public float GlobalAmmoPercentage => _globalAmmoPercentage;

    public int MaxMagAmmo => _currentWeapon != null ? _currentWeapon.MaxMagSize : 0;
    public int CurrentAmmo => _currentWeapon != null ? _currentWeapon.CurrentAmmo : 0;

    private int _currentWeaponIndex = 0; // 0 for starter weapon, 1 for secondary weapon

    private bool _isInputEnabled = true;

    private void Awake()
    {
        InitializeReferences();
        InitializeUI();
    }

    void Start()
    {
        if (_starterWeapon != null)
        {
            EquipStarterWeapon();
        }
    }

    private void OnEnable()
{
    PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
    if (playerHealth != null)
    {
        playerHealth.PlayerDeathEvent.AddListener(DisableInput);
    }
}

private void OnDisable()
{
    PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
    if (playerHealth != null)
    {
        playerHealth.PlayerDeathEvent.RemoveListener(DisableInput);
    }
}

    private void Update()
    {
        if (!_isInputEnabled) return;

        UpdatePickupText();
        HandleWeaponPickupAndDrop();
        HandleWeaponSwitching();
        HandleAimingAndShooting();
        HandleReloading();
        UpdateAmmoUI();
    }

    public void DisableInput()
    {
        _isInputEnabled = false;
        HideReloadBar();
        _pickupText.gameObject.SetActive(false);
        _currentWeapon.gameObject.SetActive(false);
    }

    private void InitializeReferences()
    {
        _mainCamera = Camera.main;

        Transform pickupTextTransform = transform.Find("PickupText");
        if (pickupTextTransform != null)
        {
            _pickupText = pickupTextTransform.GetComponent<TextMeshPro>();
        }

        _playerUI = FindObjectOfType<PlayerUI>();
        if (_playerUI == null)
        {
            Debug.LogWarning("PlayerUI component not found in the scene. Ammo bar will not be updated.");
        }
    }

    private void InitializeUI()
    {
        _reloadBar = GameObject.Find("ReloadBar")?.GetComponent<SpriteRenderer>();
        _reloadBarBG = GameObject.Find("ReloadBarBG")?.GetComponent<SpriteRenderer>();

        HideReloadBar();
    }

    private void UpdatePickupText()
    {
        WeaponBase weaponInRange = GetWeaponInRange();
        if (_pickupText != null)
        {
            _pickupText.text = weaponInRange != null ? $"[E] Swap to {weaponInRange.WeaponName}" : "";
        }
    }

    private WeaponBase GetWeaponInRange()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _pickupRadius);
        foreach (Collider2D col in colliders)
        {
            WeaponBase weapon = col.GetComponent<WeaponBase>();
            if (weapon != null && !weapon.IsEquipped)
            {
                return weapon;
            }
        }
        return null;
    }

    private void HandleWeaponPickupAndDrop()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (IsOverPickup())
            {
                PickupWeapon();
                HideReloadBar();
            }
            else if (_secondaryWeapon != null && _canDropWeapon)
            {
                DropSecondaryWeapon();
                HideReloadBar();
            }
        }
    }

    private void PickupWeapon()
    {
        WeaponBase weaponToPickup = GetWeaponInRange();
        if (weaponToPickup != null)
        {
            StopReloadCoroutine();

            // If a secondary weapon already exists, drop it
            if (_secondaryWeapon != null)
            {
                _secondaryWeapon.Drop();
            }

            // Assign the new weapon to the secondary slot
            weaponToPickup.Pickup(_weaponHolder);
            int newAmmo = Mathf.FloorToInt(_globalAmmoPercentage * weaponToPickup.MaxAmmo);
            weaponToPickup.SetCurrentAmmo(newAmmo);

            _secondaryWeapon = weaponToPickup;

            // If the starter weapon is currently equipped, keep it equipped
            if (_currentWeapon == _starterWeapon)
            {
                //_currentWeaponIndex = 0; // Keep the starter weapon equipped
                _currentWeaponIndex = 1; // Keep the starter weapon equipped
            }
            else
            {
                _currentWeaponIndex = 1; // Switch to the new secondary weapon
            }

            UpdateCurrentWeapon();
        }
    }

    private void DropSecondaryWeapon()
    {
        StopReloadCoroutine();

        _secondaryWeapon.Drop();
        _secondaryWeapon = null;

        _currentWeapon = _starterWeapon; // Switch back to the starter weapon
        WeaponChangeEvent.Invoke(_currentWeapon);
        AmmoChangeEvent.Invoke(_currentWeapon.CurrentAmmo);
    }

    private void StopReloadCoroutine()
    {
        if (_reloadCoroutine != null)
        {
            StopCoroutine(_reloadCoroutine);
            _reloadCoroutine = null;
            if (_currentWeapon != null)
            {
                _currentWeapon.IsReloading = false;
            }
            HideReloadBar();
        }
    }

    private bool IsOverPickup()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _pickupRadius);
        foreach (Collider2D col in colliders)
        {
            if (col.GetComponent<ConsumablePickup>() != null || col.GetComponent<WeaponBase>() != null)
            {
                return true;
            }
        }
        return false;
    }

    private void HandleAimingAndShooting()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _currentWeapon = _starterWeapon;
            WeaponChangeEvent.Invoke(_currentWeapon);
            AmmoChangeEvent.Invoke(_currentWeapon.CurrentAmmo);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && _secondaryWeapon != null)
        {
            _currentWeapon = _secondaryWeapon;
            WeaponChangeEvent.Invoke(_currentWeapon);
            AmmoChangeEvent.Invoke(_currentWeapon.CurrentAmmo);
        }

        if (_currentWeapon == null) return;

        Vector3 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        _currentWeapon.Aim(mousePosition);

        if (CanShoot())
        {
            StopReloadCoroutine();

            Vector3 aimDirection = (mousePosition - _currentWeapon.transform.position).normalized;
            _currentWeapon.Shoot(aimDirection);
            UpdateAmmoUI();
        }
    }

    private bool CanShoot()
    {
        if (_currentWeapon == null || _currentWeapon.CurrentMagAmmo <= 0) return false;

        TestGun testGun = _currentWeapon as TestGun;
        if (testGun != null)
        {
            return testGun.IsAutomatic ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);
        }
        return Input.GetMouseButtonDown(0);
    }

private void HandleReloading()
{
    if (_currentWeapon == null || !Input.GetKeyDown(KeyCode.R) || _currentWeapon.IsReloading || _reloadCoroutine != null) return;

    if (_currentWeapon == _starterWeapon)
    {
        // Reload starter weapon without affecting global ammo
        if (_currentWeapon.CurrentMagAmmo < _currentWeapon.MaxMagSize)
        {
            Debug.Log("Reloading starter weapon.");
            _reloadCoroutine = StartCoroutine(ReloadWithBar(_currentWeapon.ReloadTime, _currentWeapon.MaxMagSize - _currentWeapon.CurrentMagAmmo));
        }
    }
    else
    {
        // Reload secondary weapon using global ammo
        int ammoNeeded = _currentWeapon.MaxMagSize - _currentWeapon.CurrentMagAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, Mathf.FloorToInt(_globalAmmoPercentage * _currentWeapon.MaxAmmo));

        if (ammoNeeded > 0 && ammoToReload > 0)
        {
            Debug.Log($"Reloading {ammoToReload} ammo.");
            UpdateAmmoUI();

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
        ShowReloadBar();

        float elapsedTime = 0f;
        while (elapsedTime < reloadTime)
        {
            elapsedTime += Time.deltaTime;
            UpdateReloadBar(elapsedTime / reloadTime);

            if (_currentWeapon == null)
            {
                Debug.LogWarning("Weapon was dropped or swapped during reload. Stopping reload coroutine.");
                yield break;
            }

            yield return null;
        }

        CompleteReload(ammoToReload);
    }

    private void HideReloadBar()
    {
        if (_reloadBar != null) _reloadBar.enabled = false;
        if (_reloadBarBG != null) _reloadBarBG.enabled = false;
    }
    
    private void ShowReloadBar()
    {
        if (_reloadBar != null)
        {
            _reloadBar.transform.localScale = new Vector3(0, _reloadBar.transform.localScale.y, _reloadBar.transform.localScale.z);
            _reloadBar.enabled = true;
        }
        if (_reloadBarBG != null) _reloadBarBG.enabled = true;
    }

    private void UpdateReloadBar(float progress)
    {
        if (_reloadBar != null)
        {
            _reloadBar.transform.localScale = new Vector3(progress, _reloadBar.transform.localScale.y, _reloadBar.transform.localScale.z);
        }
    }

private void CompleteReload(int ammoToReload)
{
    HideReloadBar();

    if (_currentWeapon != null)
    {
        if (_currentWeapon == _starterWeapon)
        {
            // Reload starter weapon without affecting global ammo
            _currentWeapon.CurrentMagAmmo += ammoToReload;
            _currentWeapon.CurrentMagAmmo = Mathf.Clamp(_currentWeapon.CurrentMagAmmo, 0, _currentWeapon.MaxMagSize);
        }
        else
        {
            // Reload secondary weapon and update global ammo
            _globalAmmoPercentage -= (float)ammoToReload / _currentWeapon.MaxAmmo;
            _globalAmmoPercentage = Mathf.Clamp01(_globalAmmoPercentage);
            _globalAmmoPercentage = Mathf.Round(_globalAmmoPercentage * 100f) / 100f;

            _currentWeapon.Reload();
        }

        _currentWeapon.IsReloading = false;
        UpdateAmmoUI();
    }
}

private void UpdateAmmoUI()
{
    if (_currentWeapon != null)
    {
        AmmoChangeEvent.Invoke(_currentWeapon.CurrentAmmo);
        _playerUI?.UpdateAmmoBar(_currentWeapon.CurrentMagAmmo);
    }
    else
    {
        AmmoChangeEvent.Invoke(0);
    }
}

    public void AddAmmo(int percentAmount)
    {
        _globalAmmoPercentage = Mathf.Clamp01(_globalAmmoPercentage + percentAmount / 100f);
        _globalAmmoPercentage = Mathf.Round(_globalAmmoPercentage * 100f) / 100f;

        if (_currentWeapon != null)
        {
            int newAmmo = Mathf.FloorToInt(_globalAmmoPercentage * _currentWeapon.MaxAmmo);
            _currentWeapon.SetCurrentAmmo(newAmmo);
            UpdateAmmoUI();
        }
    }

    public void DisableWeaponDropTemporarily()
    {
        _canDropWeapon = false;
        StartCoroutine(ReenableWeaponDrop());
    }

    private IEnumerator ReenableWeaponDrop()
    {
        yield return new WaitForSeconds(1f);
        _canDropWeapon = true;
    }

private void EquipStarterWeapon()
{
    _starterWeapon.Pickup(_weaponHolder);

    // Set infinite ammo for the starter weapon
    _starterWeapon.SetCurrentAmmo(int.MaxValue); // Infinite ammo
    _starterWeapon.CurrentMagAmmo = _starterWeapon.MaxMagSize; // Limited mag size

    _currentWeapon = _starterWeapon; // Default to starter weapon
    _currentWeaponIndex = 0; // Ensure starter weapon is selected
    UpdateCurrentWeapon();
}

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _pickupRadius);
    }

    // Add a new method to handle weapon switching with the mouse wheel
    private void HandleWeaponSwitching()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // Scroll up
        {
            SwitchToNextWeapon();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // Scroll down
        {
            SwitchToPreviousWeapon();
        }
    }

    private void SwitchToNextWeapon()
    {
        if (_secondaryWeapon == null) return; // No secondary weapon to switch to

        _currentWeaponIndex = (_currentWeaponIndex + 1) % 2; // Toggle between 0 and 1
        UpdateCurrentWeapon();
    }

    private void SwitchToPreviousWeapon()
    {
        if (_secondaryWeapon == null) return; // No secondary weapon to switch to

        _currentWeaponIndex = (_currentWeaponIndex - 1 + 2) % 2; // Toggle between 0 and 1
        UpdateCurrentWeapon();
    }

    private void UpdateCurrentWeapon()
    {
        if (_currentWeaponIndex == 0)
        {
            _currentWeapon = _starterWeapon;
        }
        else if (_currentWeaponIndex == 1)
        {
            _currentWeapon = _secondaryWeapon;
        }

        // Update weapon visibility
        UpdateWeaponVisibility();

        // Trigger events for UI updates
        WeaponChangeEvent.Invoke(_currentWeapon);
        AmmoChangeEvent.Invoke(_currentWeapon.CurrentAmmo);
        StopReloadCoroutine();
    }

    private void UpdateWeaponVisibility()
    {
        if (_starterWeapon != null)
        {
            _starterWeapon.gameObject.SetActive(_currentWeaponIndex == 0);
        }

        if (_secondaryWeapon != null)
        {
            _secondaryWeapon.gameObject.SetActive(_currentWeaponIndex == 1);
        }
    }
}