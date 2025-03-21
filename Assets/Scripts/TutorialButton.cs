using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialButton : MonoBehaviour
{
    [SerializeField] private GameObject _garageDoor;
    [SerializeField] private GameObject _ps;

    private CameraFollow _cameraFollow;

    void Awake()
    {
        _cameraFollow = FindObjectOfType<CameraFollow>();
    }

    void Start()
    {
        // Tutorial room is to the left of the camera
        _cameraFollow.AddMaxWidth(-_cameraFollow.RoomWidth);
        _cameraFollow.AddMinWidth(-_cameraFollow.RoomWidth);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.GetComponent<BulletBase>() != null)
        {
            Instantiate(_ps, _garageDoor.transform.position, Quaternion.identity);
            Destroy(_garageDoor);
            
            _cameraFollow.AddMaxWidth(_cameraFollow.RoomWidth);

            FindObjectOfType<GameStateManager>().TutorialEnded();

            Destroy(gameObject);
        }
    }
}
