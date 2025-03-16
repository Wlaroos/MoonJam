using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Ensure TextMeshPro is included.

public class PlayerUI : MonoBehaviour
{
    private Image _hpIcon;
    private Image _heartIcon;
    private Image _weaponIcon;
    private Image _bulletIcon;
    private Image _healthBarStart;
    private Image _healthBarMid;
    private Image _healthBarEnd;

    private Image _ammoBarStart;
    private Image _ammoBarMid;
    private Image _ammoBarEnd;

    private Color _hpColor = Color.red;
    private Color _emptyColor = Color.gray;
    private Color _ammoColor = new Color(1f, 0.5f, 0f, 1f);

    private Transform _healthBarContainer; // Parent container for the health bar
    private Transform _ammoBarContainer; // Parent container for the ammo bar
    private TextMeshProUGUI _weaponText; // UI element for total ammo.
    private TextMeshProUGUI _ammoText; // UI element for total ammo.

    private PlayerHealth _playerHealth;
    private PlayerWeaponManager _playerWeaponManager; // Reference to PlayerAmmo component
    private List<Image> _healthBarMids = new List<Image>();
    private List<Image> _ammoBarMids = new List<Image>();

    [SerializeField] private bool _showWeaponIcon = true; // Toggle to show weapon icon
    [SerializeField] private bool _showWeaponName = true; // Toggle to show weapon name
    [SerializeField] private bool _isIconFirst = true; // Toggle to show weapon icon first

    void Awake()
    {
        _hpIcon = FindInChildren(transform, "HP_Icon").GetComponent<Image>();
        _heartIcon = FindInChildren(transform, "Heart_Icon").GetComponent<Image>();
        _weaponIcon = FindInChildren(transform, "Weapon_Icon").GetComponent<Image>();
        //_bulletIcon = FindInChildren(transform, "Bullet_Icon").GetComponent<Image>();
        _healthBarStart = FindInChildren(transform, "HealthBar_Start").GetComponent<Image>();
        _healthBarMid = FindInChildren(transform, "HealthBar_Mid").GetComponent<Image>();
        _healthBarEnd = FindInChildren(transform, "HealthBar_End").GetComponent<Image>();

        _ammoBarStart = FindInChildren(transform, "AmmoBar_Start").GetComponent<Image>();
        _ammoBarMid = FindInChildren(transform, "AmmoBar_Mid").GetComponent<Image>();
        _ammoBarEnd = FindInChildren(transform, "AmmoBar_End").GetComponent<Image>();

        _healthBarContainer = FindInChildren(transform, "HP_Bar_Container");
        _ammoBarContainer = FindInChildren(transform, "Ammo_Bar_Container");
        _weaponText = FindInChildren(transform, "Weapon_Text").GetComponent<TextMeshProUGUI>();
        _ammoText = FindInChildren(transform, "Ammo_Text").GetComponent<TextMeshProUGUI>();

        _playerHealth = FindObjectOfType<PlayerHealth>(); // Get reference to PlayerHealth
        _playerWeaponManager = FindObjectOfType<PlayerWeaponManager>(); // Get reference to PlayerWeaponManager

        if (_hpIcon != null)
            _hpIcon.color = _hpColor;

        if (_heartIcon != null)
            _heartIcon.color = _hpColor;

        if (_weaponIcon != null)
            _weaponIcon.color = _ammoColor;

        if (_bulletIcon != null)    
            _bulletIcon.color = _ammoColor;

        if (_playerHealth == null)
            Debug.LogWarning("PlayerHealth component not found in scene. Health bar will not be displayed.");

        if (_healthBarContainer == null)
            Debug.LogWarning("Bar container not assigned. Health bar will not be displayed.");

        if (_playerWeaponManager == null)
            Debug.LogWarning("PlayerAmmo component not found in scene. Ammo bar will not be displayed.");

        if (_ammoBarContainer == null)
            Debug.LogWarning("Ammo bar container not assigned. Ammo bar will not be displayed.");

        if (_isIconFirst)
        {
            _weaponIcon.transform.SetAsFirstSibling();
        }
        else
        {
            _weaponText.transform.SetAsFirstSibling();
        }

        if(!_showWeaponIcon)
        {
            _weaponIcon.gameObject.SetActive(false);
        }
        if(!_showWeaponName)
        {
            _weaponText.gameObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        if (_playerHealth != null)
        {
            InitializeHealthBar();
            _playerHealth.HealthChangeEvent.AddListener(UpdateHealthBar); // Listen for health changes
        }

        if (_playerWeaponManager != null)
        {
            InitializeAmmoBar();
            _playerWeaponManager.AmmoChangeEvent.AddListener(UpdateAmmoBar); // Listen for ammo changes
            _playerWeaponManager.AmmoChangeEvent.AddListener(UpdateAmmoUI); // Listen for ammo changes.
            _playerWeaponManager.WeaponChangeEvent.AddListener(OnWeaponChanged); // Listen for weapon changes
        }
    }

    void OnDisable()
    {
        if (_playerHealth != null)
        {
            _playerHealth.HealthChangeEvent.RemoveListener(UpdateHealthBar); // Stop listening for health changes
        }

        if (_playerWeaponManager != null)
        {
            _playerWeaponManager.AmmoChangeEvent.RemoveListener(UpdateAmmoBar); // Stop listening for ammo changes
            _playerWeaponManager.AmmoChangeEvent.RemoveListener(UpdateAmmoUI); // Stop listening for ammo changes.
            _playerWeaponManager.WeaponChangeEvent.RemoveListener(OnWeaponChanged); // Stop listening for weapon changes
        }
    }

    private Transform FindInChildren(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform result = FindInChildren(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

    private void InitializeHealthBar()
    {
        // Clear existing dynamically created barMids
        for (int i = 1; i < _healthBarMids.Count; i++) // Start from index 1 to skip the original _barMid
        {
            Destroy(_healthBarMids[i].gameObject);
        }

        _healthBarMids.Clear();

        // Ensure _barStart and _barEnd are active and _barMid is inactive
        _healthBarStart.gameObject.SetActive(true);
        _healthBarMid.gameObject.SetActive(_playerHealth.MaxHealth > 1); // Hide _barEnd if MaxHealth is 1
        _healthBarEnd.gameObject.SetActive(true);

        if (_playerHealth.MaxHealth > 1)
        {
            _healthBarMids.Add(_healthBarMid); // Keep the original _barMid as the first element

            // Add barMid segments based on max health
            int midCount = Mathf.Max(0, _playerHealth.MaxHealth - 2); // Subtract 2 for _barStart and _barEnd
            for (int i = 0; i < midCount; i++)
            {
                Image newBarMid = Instantiate(_healthBarMid, _healthBarContainer);
                newBarMid.gameObject.SetActive(true);
                _healthBarMids.Add(newBarMid);
            }

            // Ensure _barEnd is always at the end of the horizontal group
            _healthBarEnd.transform.SetAsLastSibling();
        }

        UpdateHealthBar(); // Initialize the health bar colors
    }

    private void UpdateHealthBar()
    {
        int currentHealth = _playerHealth.CurrentHealth;

        // Update _barStart color
        _healthBarStart.color = currentHealth > 0 ? _hpColor : _emptyColor;

        // Update _barMid colors
        for (int i = 0; i < _healthBarMids.Count; i++)
        {
            _healthBarMids[i].color = i < currentHealth - 1 ? _hpColor : _emptyColor; // Offset by 1 for _barStart
        }
    }

    private void InitializeAmmoBar()
    {
        // Clear existing dynamically created ammoBarMids
        for (int i = 1; i < _ammoBarMids.Count; i++) // Start from index 1 to skip the original _ammoBarMid
        {
            Destroy(_ammoBarMids[i].gameObject);
        }

        _ammoBarMids.Clear();

        if (_playerWeaponManager.MaxMagAmmo > 1)
        {
            _ammoBarMids.Add(_ammoBarMid); // Keep the original _ammoBarMid as the first element

            // Add ammoBarMid segments based on max ammo
            int midCount = Mathf.Max(0, _playerWeaponManager.MaxMagAmmo - 2); // Subtract 2 for _ammoBarStart and _ammoBarEnd
            for (int i = 0; i < midCount; i++)
            {
                Image newAmmoBarMid = Instantiate(_ammoBarMid, _ammoBarContainer);
                _ammoBarMids.Add(newAmmoBarMid);
            }

            // Ensure _ammoBarEnd is always at the end of the horizontal group
            _ammoBarMid.GetComponent<Image>().color = _ammoColor;
            _ammoBarEnd.transform.SetAsLastSibling();
        }
        else
        {
            _ammoBarMid.GetComponent<Image>().color = Color.clear;
            _ammoBarMid.transform.SetAsLastSibling();
        }
    }

    public void UpdateAmmoBar(int currentMagAmmo)
    {
        // Update _ammoBarStart color
        _ammoBarStart.color = currentMagAmmo > 0 ? _ammoColor : _emptyColor;

        // Update _ammoBarMid colors
        for (int i = 0; i < _ammoBarMids.Count; i++)
        {
            _ammoBarMids[i].color = i < currentMagAmmo - 1 ? _ammoColor : _emptyColor; // Offset by 1 for _ammoBarStart
        }
    }

    private void OnWeaponChanged(WeaponBase newWeapon)
    {
        InitializeAmmoBar(); // Reinitialize the ammo bar for the new weapon

            if (_showWeaponIcon)
            {
                // Show the weapon icon
                _weaponIcon.sprite = newWeapon != null ? newWeapon.WeaponSprite : null;
                _weaponIcon.color = newWeapon != null ? Color.white : Color.clear; // Show or hide the icon

                // Adjust the width to match the height using the sprite's aspect ratio
                if (newWeapon != null && newWeapon.WeaponSprite != null)
                {
                    float aspectRatio = (float)newWeapon.WeaponSprite.texture.width / newWeapon.WeaponSprite.texture.height;
                    RectTransform rectTransform = _weaponIcon.GetComponent<RectTransform>();
                    rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.y * aspectRatio, rectTransform.sizeDelta.y);
                }
            }
            if (_showWeaponName)
            {
                if (_weaponText != null)
                {
                    _weaponText.text = newWeapon != null ? newWeapon.WeaponName : ""; // Display weapon name
                }
            }
    }

    private void UpdateAmmoUI(int currentAmmo)
    {
        if (_playerWeaponManager != null && _playerWeaponManager.MaxMagAmmo > 0)
        {
            // Update ammo text.
            if (_ammoText != null)
            {
                _ammoText.text = $"x{currentAmmo}";
            }

            // Update magazine bar visibility.
            if (_ammoBarContainer != null)
            {
                _ammoBarContainer.GetComponent<CanvasGroup>().alpha = 1f;
            }
        }
        else
        {
            // Clear ammo text and hide magazine bar if no weapon is equipped.
            if (_ammoText != null)
            {
                _ammoText.text = "";
            }

            if (_ammoBarContainer != null)
            {
                _ammoBarContainer.GetComponent<CanvasGroup>().alpha = 0f;
            }
        }
    }
}
