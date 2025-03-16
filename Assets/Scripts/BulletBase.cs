using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBase : MonoBehaviour
{

    [SerializeField] private float _shotSpeed = 5;
    [SerializeField] private int _damage = 1;
    [SerializeField] private float _knockback = 3;
    [SerializeField] private float _size = 1;
    [SerializeField] private float _lifetime = 3;
    [SerializeField] private GameObject _ps;
    private Rigidbody2D _rb;

    private bool _once = false;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }
    
    public void BulletSetup(Vector3 shootDir, float angle, float shotSpeed, int damage, float knockback, float size, float lifetime)
    {
        _shotSpeed = shotSpeed;
        _damage = damage;
        _knockback = knockback;
        _size = size;
        _lifetime = lifetime;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        transform.localScale = new Vector3(_size, _size, _size);

        transform.eulerAngles = new Vector3(0, 0, angle);

        float vel = _shotSpeed;
        rb.AddForce(shootDir * vel, ForceMode2D.Impulse);
        
        StartCoroutine(DelayedDestroy(_lifetime));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {   
        if(collision.CompareTag("BulletBounds") && !_once)
        {
            Destroy();
        }

        // If the bullet hits an enemy
        else if (collision.GetComponent<EnemyHealth>() != null)
        {
            collision.GetComponent<EnemyHealth>().TakeDamage(_rb.velocity.normalized * _knockback, _damage);
            Destroy();
        }
    }
    private IEnumerator DelayedDestroy(float delay)
    {
        
        yield return new WaitForSeconds(delay);
        Destroy();
    }

    private void Destroy()
    {
        if (!_once)
        {
            if(_ps != null)
            {
                Instantiate(_ps,transform.position,Quaternion.identity);
            }

            //ADD SFX
            _once = true;
            StopAllCoroutines();
            Destroy(gameObject);
        }
    }
}
