using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumablePickup : MonoBehaviour
{
    public enum ConsumableType
    {
        Ammo,
        Health
    }

    [SerializeField] private Sprite[] _consumableSprite; // The sprite for the pickup.
    [SerializeField] private Sprite[] _consumableHoverSprite; // The sprite for the pickups when the player is near it.
    [SerializeField] private Sprite[] _consumableTextSprite; // Text (Optional)
    [SerializeField] private GameObject[] _consumableEffect; // The effect that plays when the pickup is used.
    [SerializeField] private ConsumableType _consumableType; // The type of consumable (e.g., health, ammo, etc.)

    [SerializeField] private int _ammoPercentAmount = 10; // The amount of ammo the pickup gives.
    [SerializeField] private int _healthAmount = 2; // The amount of health the pickup gives.

    private PlayerHealth _ph;
    private PlayerWeaponManager _pwm;

    private SpriteRenderer _sr;
    private SpriteRenderer _textSr;
    private Collider2D _col;

    private void Awake()
    {
        InitializeComponents();
        SetInitialSprite();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ShowHoverState();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ResetSprite();
        }
    }

    public void PickedUp()
    {
        ApplyEffect();
        DestroyPickup();
    }

    private void InitializeComponents()
    {
        _sr = GetComponent<SpriteRenderer>();
        _col = GetComponent<Collider2D>();
        _ph = FindObjectOfType<PlayerHealth>();
        _pwm = FindObjectOfType<PlayerWeaponManager>();
        _textSr = transform.GetChild(0).GetComponent<SpriteRenderer>();
        _textSr.color = Color.clear;
    }

    private void SetInitialSprite()
    {
        switch (_consumableType)
        {
            case ConsumableType.Ammo:
                _sr.sprite = _consumableSprite[0];
                break;
            case ConsumableType.Health:
                _sr.sprite = _consumableSprite[1];
                break;
        }
    }

    private void ShowHoverState()
    {
        switch (_consumableType)
        {
            case ConsumableType.Ammo:
                _sr.sprite = _consumableHoverSprite[0];
                _textSr.sprite = _consumableTextSprite[0];
                _textSr.color = Color.white;
                break;
            case ConsumableType.Health:
                _sr.sprite = _consumableHoverSprite[1];
                _textSr.sprite = _consumableTextSprite[1];
                _textSr.color = Color.white;
                break;
        }
    }

    private void ResetSprite()
    {
        switch (_consumableType)
        {
            case ConsumableType.Ammo:
                _sr.sprite = _consumableSprite[0];
                _textSr.color = Color.clear;
                break;
            case ConsumableType.Health:
                _sr.sprite = _consumableSprite[1];
                _textSr.color = Color.clear;
                break;
        }
    }

    private void ApplyEffect()
    {
        switch (_consumableType)
        {
            case ConsumableType.Ammo:
                Instantiate(_consumableEffect[0], transform.position, Quaternion.identity);
                _pwm.AddAmmo(_ammoPercentAmount);
                break;
            case ConsumableType.Health:
                Instantiate(_consumableEffect[1], transform.position, Quaternion.identity);
                _ph.Heal(_healthAmount);
                break;
        }
    }

    private void DestroyPickup()
    {
        Destroy(gameObject);
    }

    public void SetConsumableType(ConsumableType type)
    {
        _consumableType = type;

        // Update the sprite based on the new consumable type
        switch (_consumableType)
        {
            case ConsumableType.Ammo:
                _sr.sprite = _consumableSprite[0];
                _textSr.sprite = _consumableTextSprite[0];
                break;
            case ConsumableType.Health:
                _sr.sprite = _consumableSprite[1];
                _textSr.sprite = _consumableTextSprite[1];
                break;
        }

        // Ensure the hover state and text sprite are reset
        _textSr.color = Color.clear;
    }
}