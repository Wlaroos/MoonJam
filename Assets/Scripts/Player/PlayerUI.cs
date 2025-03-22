using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [Header("UI Elements")]
    private Image _hpIcon, _heartIcon, _weaponIcon, _bulletIcon;
    private Image _healthBarStart, _healthBarMid, _healthBarEnd;
    private Image _ammoBarStart, _ammoBarMid, _ammoBarEnd;
    private TextMeshProUGUI _weaponText, _ammoText;

    [Header("Containers")]
    private Transform _healthBarContainer, _ammoBarContainer;

    [Header("Colors")]
    private readonly Color _hpColor = Color.red;
    private readonly Color _emptyColor = Color.gray;
    private readonly Color _ammoColor = new Color(1f, 0.5f, 0f, 1f);

    [Header("References")]
    private PlayerHealth _playerHealth;
    private PlayerWeaponManager _playerWeaponManager;

    private List<Image> _healthBarMids = new List<Image>();
    private List<Image> _ammoBarMids = new List<Image>();

    [Header("Settings")]
    [SerializeField] private bool _showWeaponIcon = true;
    [SerializeField] private bool _showWeaponName = true;
    [SerializeField] private bool _isIconFirst = true;

    private void Awake()
    {
        InitializeUIElements();
        InitializePlayerReferences();
        ConfigureWeaponDisplay();
    }

    void Start()
    {
        UpdateAmmoUI(0);
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void InitializeUIElements()
    {
        _hpIcon = FindUIElement<Image>("HP_Icon");
        _heartIcon = FindUIElement<Image>("Heart_Icon");
        _weaponIcon = FindUIElement<Image>("Weapon_Icon");
        _healthBarStart = FindUIElement<Image>("HealthBar_Start");
        _healthBarMid = FindUIElement<Image>("HealthBar_Mid");
        _healthBarEnd = FindUIElement<Image>("HealthBar_End");
        _ammoBarStart = FindUIElement<Image>("AmmoBar_Start");
        _ammoBarMid = FindUIElement<Image>("AmmoBar_Mid");
        _ammoBarEnd = FindUIElement<Image>("AmmoBar_End");
        _healthBarContainer = FindUIElement<Transform>("HP_Bar_Container");
        _ammoBarContainer = FindUIElement<Transform>("Ammo_Bar_Container");
        _weaponText = FindUIElement<TextMeshProUGUI>("Weapon_Text");
        _ammoText = FindUIElement<TextMeshProUGUI>("Ammo_Text");

        SetInitialColors();
    }

    private void InitializePlayerReferences()
    {
        _playerHealth = FindObjectOfType<PlayerHealth>();
        _playerWeaponManager = FindObjectOfType<PlayerWeaponManager>();

        if (_playerHealth == null)
            Debug.LogWarning("PlayerHealth component not found. Health bar will not be displayed.");

        if (_playerWeaponManager == null)
            Debug.LogWarning("PlayerWeaponManager component not found. Ammo bar will not be displayed.");
    }

    private void SetInitialColors()
    {
        SetImageColor(_hpIcon, _hpColor);
        SetImageColor(_heartIcon, _hpColor);
        SetImageColor(_weaponIcon, _ammoColor);
    }

    private void ConfigureWeaponDisplay()
    {
        if (_isIconFirst)
            _weaponIcon.transform.SetAsFirstSibling();
        else
            _weaponText.transform.SetAsFirstSibling();

        _weaponIcon.gameObject.SetActive(_showWeaponIcon);
        _weaponText.gameObject.SetActive(_showWeaponName);
    }

    private void SubscribeToEvents()
    {
        if (_playerHealth != null)
        {
            InitializeHealthBar();
            _playerHealth.HealthChangeEvent.AddListener(UpdateHealthBar);
        }

        if (_playerWeaponManager != null)
        {
            InitializeAmmoBar();
            _playerWeaponManager.AmmoChangeEvent.AddListener(UpdateAmmoBar);
            _playerWeaponManager.AmmoChangeEvent.AddListener(UpdateAmmoUI);
            _playerWeaponManager.WeaponChangeEvent.AddListener(OnWeaponChanged);
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (_playerHealth != null)
            _playerHealth.HealthChangeEvent.RemoveListener(UpdateHealthBar);

        if (_playerWeaponManager != null)
        {
            _playerWeaponManager.AmmoChangeEvent.RemoveListener(UpdateAmmoBar);
            _playerWeaponManager.AmmoChangeEvent.RemoveListener(UpdateAmmoUI);
            _playerWeaponManager.WeaponChangeEvent.RemoveListener(OnWeaponChanged);
        }
    }

    private void InitializeHealthBar()
    {
        ClearBarSegments(_healthBarMids);
        _healthBarStart.gameObject.SetActive(true);
        _healthBarMid.gameObject.SetActive(_playerHealth.MaxHealth > 1);
        _healthBarEnd.gameObject.SetActive(true);

        if (_playerHealth.MaxHealth > 1)
        {
            AddBarSegments(_healthBarMids, _healthBarMid, _healthBarContainer, _playerHealth.MaxHealth - 2);
            _healthBarEnd.transform.SetAsLastSibling();
        }

        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        int currentHealth = _playerHealth.CurrentHealth;

        _healthBarStart.color = currentHealth > 0 ? _hpColor : _emptyColor;

        for (int i = 0; i < _healthBarMids.Count; i++)
        {
            _healthBarMids[i].color = i < currentHealth - 1 ? _hpColor : _emptyColor;
        }
    }

private void InitializeAmmoBar()
{
    ClearBarSegments(_ammoBarMids);

    // Add mid segments if there is more than one bullet in the magazine
    AddBarSegments(_ammoBarMids, _ammoBarMid, _ammoBarContainer, _playerWeaponManager.MaxMagAmmo - 2);
    _ammoBarEnd.transform.SetAsLastSibling();
    _ammoBarMid.gameObject.SetActive(true); // Ensure midBar is active

    if (_playerWeaponManager.MaxMagAmmo == 1)
    {
      // Disable the midBar if there is only one bullet in the magazine
        _ammoBarMid.gameObject.SetActive(false);
    }
}

    public void UpdateAmmoBar(int currentMagAmmo)
    {
        _ammoBarStart.color = currentMagAmmo > 0 ? _ammoColor : _emptyColor;

        for (int i = 0; i < _ammoBarMids.Count; i++)
        {
            _ammoBarMids[i].color = i < currentMagAmmo - 1 ? _ammoColor : _emptyColor;
        }
    }

    private void UpdateAmmoUI(int currentAmmo)
    {
        if (_ammoText != null)
        {
            if (_playerWeaponManager != null && _playerWeaponManager.CurrentWeapon == null)
            {
                // Hide ammo UI if no weapon is equipped
                _ammoText.text = "";
                SetCanvasGroupAlpha(_ammoBarContainer, 0f); // Hide the ammo bar container
                SetCanvasGroupAlpha(_ammoText.transform.parent, 0f); // Hide the horizontal container
            }
            else
            {
                // Show ammo UI and update the text
                SetCanvasGroupAlpha(_ammoBarContainer, 1f); // Show the ammo bar container
                SetCanvasGroupAlpha(_ammoText.transform.parent, 1f); // Show the horizontal container

                if (_playerWeaponManager.CurrentWeapon == _playerWeaponManager.StarterWeapon)
                {
                    _ammoText.text = "xINF"; // Display infinite ammo for the starter weapon
                }
                else
                {
                    _ammoText.text = $"x{currentAmmo}"; // Display regular ammo count for other weapons
                }
            }
        }
    }

    private void OnWeaponChanged(WeaponBase newWeapon)
    {
        InitializeAmmoBar();

        if (_showWeaponIcon && _weaponIcon != null)
        {
            _weaponIcon.sprite = newWeapon.WeaponSprite;
            _weaponIcon.color = newWeapon != null ? Color.white : Color.clear;

            if (newWeapon.WeaponSprite != null)
            {
            AdjustIconAspectRatio(_weaponIcon, newWeapon.WeaponSprite);

            // Clamp the sizeDelta to prevent extreme scaling
            RectTransform rectTransform = _weaponIcon.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(
                Mathf.Clamp(rectTransform.sizeDelta.x, 6f, 12f),
                Mathf.Clamp(rectTransform.sizeDelta.y, 6f, 6f)
            );
            }
        }

        if (_showWeaponName && _weaponText != null)
            _weaponText.text = newWeapon?.WeaponName ?? "";
    }

    private T FindUIElement<T>(string name) where T : Component
    {
        Transform element = FindInChildren(transform, name);
        return element != null ? element.GetComponent<T>() : null;
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

    private void SetImageColor(Image image, Color color)
    {
        if (image != null)
            image.color = color;
    }

    private void SetCanvasGroupAlpha(Transform container, float alpha)
    {
        if (container != null)
        {
            CanvasGroup canvasGroup = container.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                canvasGroup.alpha = alpha;
        }
    }

    private void AdjustIconAspectRatio(Image icon, Sprite sprite)
    {
        RectTransform rectTransform = icon.GetComponent<RectTransform>();
        float aspectRatio = (float)sprite.texture.width / sprite.texture.height;
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.y * aspectRatio, rectTransform.sizeDelta.y);
    }

    private void ClearBarSegments(List<Image> barSegments)
    {
        for (int i = 1; i < barSegments.Count; i++)
            Destroy(barSegments[i].gameObject);

        barSegments.Clear();
    }

    private void AddBarSegments(List<Image> barSegments, Image template, Transform container, int count)
    {
        barSegments.Add(template);

        for (int i = 0; i < count; i++)
        {
            Image newSegment = Instantiate(template, container);
            newSegment.gameObject.SetActive(true);
            barSegments.Add(newSegment);
        }
    }
}