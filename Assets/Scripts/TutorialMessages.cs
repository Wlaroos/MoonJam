using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMessages : MonoBehaviour
{
    [SerializeField] private GameObject _gunMessage;
    [SerializeField] private GameObject _dummyMessage;
    [SerializeField] private GameObject _hpMessage;
    [SerializeField] private GameObject _buttonMessage; 
    [SerializeField] private GameObject _runMessage;

    private bool _gunBool = false;
    private bool _dummyBool = false;
    private bool _hpBool = false;
    private bool _buttonBool = false;

    private PlayerWeaponManager _pwm;
    private PlayerHealth _ph;
    private TutorialZombie _tz;
    private TutorialButton _tb;

    void Awake()
    {
        _gunMessage.SetActive(false);
        _dummyMessage.SetActive(false);
        _hpMessage.SetActive(false);
        _buttonMessage.SetActive(false);
        _runMessage.SetActive(false);

        _pwm = FindObjectOfType<PlayerWeaponManager>();
        _ph = FindObjectOfType<PlayerHealth>();
        _tz = FindObjectOfType<TutorialZombie>();
        _tb = FindObjectOfType<TutorialButton>();
    }

    void Update()
    {
        if(_pwm.CurrentWeapon != null && !_gunBool)
        {
            if (_gunMessage != null) _gunMessage.GetComponent<ParticleOnShot>().Explode();
            if (_dummyMessage != null) _dummyMessage.SetActive(true);
            _gunBool = true;
        }

        if(_tz == null && !_dummyBool)
        {

            if (_dummyMessage != null) _dummyMessage.GetComponent<ParticleOnShot>().Explode();
            if (_hpMessage != null) _hpMessage.SetActive(true);
            _dummyBool = true;
        }

        if(_ph.CurrentHealth == _ph.MaxHealth && !_hpBool)
        {
            if (_hpMessage != null) _hpMessage.GetComponent<ParticleOnShot>().Explode();
            if (_buttonMessage != null) _buttonMessage.SetActive(true);
            _hpBool = true;
        }

        if(_tb == null && !_buttonBool)
        {

            if (_buttonMessage != null) _buttonMessage.GetComponent<ParticleOnShot>().Explode();
            if (_runMessage != null) _runMessage.SetActive(true);
            _buttonBool = true;
        }
    }

    public void ActivateGunMessage()
    {
        if (_gunMessage != null) _gunMessage.SetActive(true);
    }
}
