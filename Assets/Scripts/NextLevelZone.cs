using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextLevelZone : MonoBehaviour
{
    private GameStateManager _gsm;

    private void Awake()
    {
        _gsm = FindObjectOfType<GameStateManager>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _gsm.NextLevelTrigger();
        }
    }
}
