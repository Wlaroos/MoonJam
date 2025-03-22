using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleOnShot : MonoBehaviour
{
    [SerializeField] private GameObject _ps;

    private void OnEnable()
    {
        StartCoroutine(DelayedExplode(2f));
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<BulletBase>() != null)
        {
            Explode();
        }
    }

    private IEnumerator DelayedExplode(float delay)
    {
        yield return new WaitForSeconds(delay);
        Explode();
    }

    public void Explode()
    {
        Instantiate(_ps, transform.position, Quaternion.identity);
        Instantiate(_ps, transform.position, Quaternion.Euler(0, 0, 180));
        Destroy(gameObject);
    }
}
