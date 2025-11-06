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
    private Collider2D _parentCol;

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

        if (transform.parent != null && transform.parent.parent != null)
        {
            _parentCol = transform.parent.parent.GetComponent<Collider2D>();
        }

        if (_soulType != SoulType.Skeleton && _soulType != SoulType.None)
        {
            _soul = Instantiate(_soulPrefab, transform.position, Quaternion.identity, transform).GetComponent<Soul>();

            _soul.ParentSoulVision = this;

            _soul.gameObject.SetActive(false);
        }
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

            if (_soulType != SoulType.Skeleton && _soulType != SoulType.None)
            {
                _col.enabled = false;
                _col.tag = "Untagged";

                if (_parentCol != null)
                    _parentCol.tag = "Untagged";

                _soul.gameObject.SetActive(true);
            }
        }
        else
        {
            _sr.sprite = _originalSprite;

            if (_soulType != SoulType.Skeleton && _soulType != SoulType.None && _soulType != SoulType.Player)
            {
                _col.enabled = true;
                _col.tag = "Enemy";

                if (_parentCol != null)
                    _parentCol.tag = "Enemy";

                _soul.gameObject.SetActive(false);
            }
            else if (_soulType == SoulType.Player)
            {
                _col.enabled = true;
                _col.tag = "Player";

                if (_parentCol != null)
                    _parentCol.tag = "Player";

                _soul.gameObject.SetActive(false);
            }
        }
    }
}
