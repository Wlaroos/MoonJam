using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soul : MonoBehaviour
{
    [SerializeField] private float _floatAmplitude = 0.25f;
    [SerializeField] private float _floatFrequency = 1f;
    [SerializeField] private float _rotationSpeed = 50f;
    private Vector3 _startPosition;
    public SoulVision ParentSoulVision { get; set; }

    private GameObject _shieldTop;
    private GameObject _shieldLeft;
    private GameObject _shieldBottom;
    private GameObject _shieldRight;

    void Start()
    {
        _startPosition = transform.position;
        _shieldTop = transform.GetChild(0).gameObject;
        _shieldLeft = transform.GetChild(1).gameObject;
        _shieldBottom = transform.GetChild(2).gameObject;
        _shieldRight = transform.GetChild(3).gameObject;
    }

    void Update()
    {
        // Floating effect
        float newY = _startPosition.y + Mathf.Sin(Time.time * _floatFrequency) * _floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Rotation effect
        transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);

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
