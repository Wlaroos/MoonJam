using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulVision : MonoBehaviour
{
    [SerializeField] private Sprite _originalSprite;
    [SerializeField] private Sprite _soulSprite;
    [SerializeField] private GameObject _soulPrefab;
    public enum SoulType { None, Player, Chub, Skeleton, Eye, Armor };
    public SoulType _soulType;
    private Soul _soul;


    private SpriteRenderer _sr;
    private Collider2D _col;

    void OnEnable()
    {
        VisionManager.Instance.SoulVisionToggleEvent.AddListener(ToggleSoulVision);
        SetSoulVisionSprite();
    }

    void OnDisable()
    {
        VisionManager.Instance.SoulVisionToggleEvent.RemoveListener(ToggleSoulVision);
    }

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _col = GetComponentInParent<Collider2D>();

        _soul = Instantiate(_soulPrefab, transform.position, Quaternion.identity, transform).GetComponent<Soul>();

        _soul.ParentSoulVision = this;

        _soul.gameObject.SetActive(false);
    }

    void ToggleSoulVision()
    {
        SetSoulVisionSprite();
    }

    void Update()
    {

    }

    void SetSoulVisionSprite()
    {
        if (VisionManager.Instance.IsSoulVisionActive)
        {
            _sr.sprite = _soulSprite;
            
            if (_soulType != SoulType.Skeleton || _soulType != SoulType.None)
            {
                _col.enabled = false;
                _soul.gameObject.SetActive(true);
            }
        }
        else
        {
            _sr.sprite = _originalSprite;
            _col.enabled = true;
            _soul.gameObject.SetActive(false);
        }
    }
}
