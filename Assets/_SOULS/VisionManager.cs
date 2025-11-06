using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(-100)] // ensure this runs early so Instance is available for other scripts' OnEnable
public class VisionManager : MonoBehaviour
{
    public static VisionManager Instance;

    private UnityEvent _soulVisionToggleEvent = new UnityEvent();
    public UnityEvent SoulVisionToggleEvent => _soulVisionToggleEvent;

    private bool _isSoulVisionActive = false;
    public bool IsSoulVisionActive => _isSoulVisionActive;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            SoulVisionToggle();
        }
    }

    void SoulVisionToggle()
    {
        _isSoulVisionActive = !_isSoulVisionActive;
        _soulVisionToggleEvent.Invoke();
    }
}
