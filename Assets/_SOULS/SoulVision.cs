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

    private const string TAG_ENEMY = "Enemy";
    private const string TAG_PLAYER = "Player";
    private const string TAG_UNTAGGED = "Untagged";

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
        // Find the nearest parent collider (this object might be a child under a sprite holder)
    _col = GetComponentInParent<Collider2D>();
    _parentCol = FindFurthestAncestorCollider(transform);

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
                ApplySoulVisionState(true);
            }
        }
        else
        {
            _sr.sprite = _originalSprite;

            if (_soulType != SoulType.Skeleton && _soulType != SoulType.None && _soulType != SoulType.Player)
            {
                ApplySoulVisionState(false);
            }
            else if (_soulType == SoulType.Player)
            {
                ApplySoulVisionState(false);
            }
        }
    }

    // Helper: climb parents and return the furthest (top-most) Collider2D found
    private Collider2D FindFurthestAncestorCollider(Transform start)
    {
        Collider2D found = null;
        Transform t = start.parent;
        while (t != null)
        {
            var c = t.GetComponent<Collider2D>();
            if (c != null) found = c;
            t = t.parent;
        }
        return found;
    }

    private void ApplySoulVisionState(bool soulVisionActive)
    {
        // soulVisionActive: show soul visuals, disable main collider interactions
        if (_col != null)
        {
            _col.enabled = !soulVisionActive;
            _col.tag = soulVisionActive ? TAG_UNTAGGED : (_soulType == SoulType.Player ? TAG_PLAYER : TAG_ENEMY);
        }

        if (_parentCol != null)
        {
            _parentCol.tag = soulVisionActive ? TAG_UNTAGGED : (_soulType == SoulType.Player ? TAG_PLAYER : TAG_ENEMY);
        }

        if (_soul != null)
        {
            _soul.gameObject.SetActive(soulVisionActive);
        }
    }
}
