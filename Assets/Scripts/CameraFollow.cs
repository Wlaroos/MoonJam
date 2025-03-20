using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform _playerRef; // Reference to the player's transform
    public float _smoothSpeed = 0.125f; // Smoothing speed for the camera movement
    public float _minY = 0f; // Minimum Y position for the camera
    public float _maxY = 10f; // Maximum Y position for the camera

    private void LateUpdate()
    {
        if (_playerRef != null)
        {
            // Get the current camera position
            Vector3 currentPosition = transform.position;

            // Update only the Y position to follow the player
            float targetY = Mathf.Clamp(_playerRef.position.y, _minY, _maxY); // Clamp the Y position within bounds
            Vector3 targetPosition = new Vector3(currentPosition.x, targetY, currentPosition.z);

            // Smoothly interpolate between the current position and the target position
            transform.position = Vector3.Lerp(currentPosition, targetPosition, _smoothSpeed);
        }
    }

    public void AddMaxHeight(float height)
    {
        _maxY += height;
    }

    public void AddMinHeight(float height)
    {
        _minY += height;
    }
}
