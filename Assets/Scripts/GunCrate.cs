using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunCrate : MonoBehaviour
{
    [SerializeField] private GameObject[] _weaponPrefabs;
    [SerializeField] private GameObject _pickupPrefab;
    [SerializeField] private GameObject _psLanded;
    [SerializeField] private GameObject _psOpened;
    [SerializeField] private GameObject _arrowHover;
    [SerializeField] private GameObject _nextLevelZone;

    private bool _invincible = true;

    void Awake()
    {
        StartCoroutine(SpawnStuff());
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<BulletBase>() != null && !_invincible)
        {
            int randomIndex = Random.Range(0, _weaponPrefabs.Length);

            GameObject gun = Instantiate(_weaponPrefabs[randomIndex], transform.position, Quaternion.identity);
            // Offsets the guns offset
            gun.transform.position = new Vector2(gun.transform.position.x - gun.transform.GetChild(0).localPosition.x, gun.transform.position.y);
            Instantiate(_psOpened, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }

    public IEnumerator SpawnStuff()
    {   
        _invincible = false;
        Instantiate(_psLanded, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(0.5f);

        Instantiate (_pickupPrefab, transform.position + new Vector3(2,-2), Quaternion.identity);
        Instantiate (_pickupPrefab, transform.position + new Vector3(-2,-2), Quaternion.identity);
        
        GameObject hp = Instantiate (_pickupPrefab, transform.position + new Vector3(0,-3), Quaternion.identity);
        hp.GetComponent<ConsumablePickup>().SetConsumableType(ConsumablePickup.ConsumableType.Health);

        Instantiate (_psLanded, transform.position + new Vector3(2,-2), Quaternion.identity);
        Instantiate (_psLanded, transform.position + new Vector3(-2,-2), Quaternion.identity);
        Instantiate (_psLanded, transform.position + new Vector3(0,-3), Quaternion.identity);

        Instantiate (_arrowHover, transform.position + new Vector3(0, 3), Quaternion.identity);

        Instantiate (_nextLevelZone, transform.position + new Vector3(0, 6f), Quaternion.identity);
    }
}
