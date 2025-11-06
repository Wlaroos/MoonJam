using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soul : MonoBehaviour
{
    private Vector3 _startPosition;
    public SoulVision ParentSoulVision { get; set; }

    [SerializeField] private GameObject _shieldTop;
    [SerializeField] private GameObject _shieldLeft;
    [SerializeField] private GameObject _shieldBottom;
    [SerializeField] private GameObject _shieldRight;

    void Start()
    {
        _startPosition = transform.position;
    }

    void Update()
    {
        if (ParentSoulVision != null)
        {
            // Future logic per soul-type can be handled here. For now no continuous per-frame behavior is required.
        }
    }

    void OnEnable()
    {
        if (ParentSoulVision != null && ParentSoulVision._soulType == SoulVision.SoulType.Armor)
        {
            _shieldLeft?.SetActive(true);
            _shieldRight?.SetActive(true);
        }
    }
}
