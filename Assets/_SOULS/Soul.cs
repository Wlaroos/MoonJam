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
            switch (ParentSoulVision._soulType)
            {
                case SoulVision.SoulType.Player:
                    break;
                case SoulVision.SoulType.Chub:
                    break;
                case SoulVision.SoulType.Skeleton:
                    break;
                case SoulVision.SoulType.Eye:
                    break;
                case SoulVision.SoulType.Armor:
                    break;
            }
        }
    }

    void OnEnable()
    {
                if (ParentSoulVision != null)
        {
            switch (ParentSoulVision._soulType)
            {
                case SoulVision.SoulType.Player:
                    break;
                case SoulVision.SoulType.Chub:
                    break;
                case SoulVision.SoulType.Skeleton:
                    break;
                case SoulVision.SoulType.Eye:
                    break;
                case SoulVision.SoulType.Armor:
                    {
                        _shieldLeft.SetActive(true);
                        _shieldRight.SetActive(true); 
                    }
                    break;
            }
        }
    }
}
