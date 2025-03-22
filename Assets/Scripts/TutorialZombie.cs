using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialZombie : MonoBehaviour
{

    [SerializeField] private GameObject _psShotDir;
    [SerializeField] private GameObject _psShotCir;
    [SerializeField] private GameObject _psDead;
    private int _health = 4;
    private SpriteRenderer _sr;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<BulletBase>() != null)
        {
            Destroy(other.gameObject);

            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Vector3 bulletDirection = other.GetComponent<Rigidbody2D>().velocity.normalized;
            float angle = Mathf.Atan2(bulletDirection.y, bulletDirection.x) * Mathf.Rad2Deg;

            Instantiate(_psShotDir, hitPoint, Quaternion.Euler(0, 0, angle));
            Instantiate(_psShotCir, other.transform.position, Quaternion.identity);

            StartCoroutine(FlashRed());

            _health--;

            if (_health <= 0)
            {
                Instantiate(_psDead, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }

        private IEnumerator FlashRed()
    {
        _sr.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        _sr.color = Color.white;
    }

}