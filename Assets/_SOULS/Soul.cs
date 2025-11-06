using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soul : MonoBehaviour
{
    private Vector3 _startPosition;
    public SoulVision ParentSoulVision { get; set; }

    [Header("Orbit (Eye)")]
    [SerializeField] private float _orbitSpeed = 90f; // degrees per second
    [SerializeField] private Vector3 _orbitLocalOffset = new Vector3(2f, 0f, 0f);
    private float _orbitAngle = 0f; // current orbit angle in degrees

    [SerializeField] private GameObject _shieldTop;
    [SerializeField] private GameObject _shieldLeft;
    [SerializeField] private GameObject _shieldBottom;
    [SerializeField] private GameObject _shieldRight;

    void Start()
    {
        _startPosition = transform.position;
        // Initialize orbit angle based on current local position relative to parent if available
        if (ParentSoulVision != null)
        {
            Vector3 localPos = ParentSoulVision.transform.InverseTransformPoint(transform.position);
            if (localPos.sqrMagnitude > 0.0001f)
            {
                _orbitAngle = Mathf.Atan2(localPos.y, localPos.x) * Mathf.Rad2Deg;
                // Keep distance stored in _orbitLocalOffset magnitude
                _orbitLocalOffset = new Vector3(localPos.magnitude, 0f, 0f);
            }
        }
    }

    void Update()
    {
        if (ParentSoulVision != null && ParentSoulVision._soulType == SoulVision.SoulType.Eye)
        {
            // Update angle (degrees)
            _orbitAngle += _orbitSpeed * Time.deltaTime;

            // Rotate the local offset by current angle (in local space) and convert to world space
            Vector3 rotatedLocal = Quaternion.Euler(0f, 0f, _orbitAngle) * _orbitLocalOffset;
            transform.position = ParentSoulVision.transform.TransformPoint(rotatedLocal);
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
