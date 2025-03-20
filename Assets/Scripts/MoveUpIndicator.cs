using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveUpIndicator : MonoBehaviour
{
    [SerializeField] private GameObject _ps;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<BulletBase>() != null)
        {
            Instantiate(_ps, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
