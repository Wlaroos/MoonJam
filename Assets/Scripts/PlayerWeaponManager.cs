using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private Transform _weaponHolder;
    [SerializeField] private float _pickupRadius = 1f;

    [Header("UI References")]
    private TextMeshPro _pickupText;
    private PlayerUI _playerUI;
    private SpriteRenderer _reloadBar;
    private SpriteRenderer _reloadBarBG;

    [Header("Events")]
    public UnityEvent<int> AmmoChangeEvent = new UnityEvent<int>();
    public UnityEvent<WeaponBase> WeaponChangeEvent = new UnityEvent<WeaponBase>();

    private WeaponBase _currentWeapon;
    private Camera _mainCamera;
    private Coroutine _reloadCoroutine;
    private bool _canDropWeapon = true;

    private float _globalAmmoPercentage = 1f;
    public float GlobalAmmoPercentage => _globalAmmoPercentage;

    public int MaxMagAmmo => _currentWeapon != null ? _currentWeapon.MaxMagSize : 0;
    public int CurrentAmmo => _currentWeapon != null ? _currentWeapon.CurrentAmmo : 0;

    private void Awake()
    {
        InitializeReferences();
        InitializeUI();
    }

    private void Update()
    {
        UpdatePickupText();
        HandleWeaponPickupAndDrop();
        HandleAimingAndShooting();
        HandleReloading();
        UpdateAmmoUI();
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
        if (!_canDropWeapon) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (IsOverPickup())
            {
                PickupWeapon();
                HideReloadBar();
            }
            else if (_currentWeapon != null)
            {
                DropCurrentWeapon();
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
    }

    private void DropCurrentWeapon()
    {
        StopReloadCoroutine();

        _currentWeapon.Drop();
        _currentWeapon = null;

        WeaponChangeEvent.Invoke(null);
        AmmoChangeEvent.Invoke(0);
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
        if (_currentWeapon == null) return;

        Vector3 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        _currentWeapon.Aim(mousePosition);

        if (CanShoot())
        {
            StopReloadCoroutine();
            HideReloadBar();

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
            _globalAmmoPercentage -= (float)ammoToReload / _currentWeapon.MaxAmmo;
            _globalAmmoPercentage = Mathf.Clamp01(_globalAmmoPercentage);

            _currentWeapon.Reload();
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _pickupRadius);
    }
}