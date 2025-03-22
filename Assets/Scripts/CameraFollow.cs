using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _playerRef; // Reference to the player's transform
    [SerializeField] private float _smoothSpeed = 0.125f; // Smoothing speed for the camera movement
    private float _minY = 0f; // Minimum Y position for the camera
    public float MinY => _minY;
    private float _maxY = 0f; // Maximum Y position for the camera
    public float MaxY => _maxY;
    private float _minX = 0f; // Minimum Y position for the camera
    public float MinX => _minX;
    private float _maxX = 0f; // Maximum Y position for the camera
    public float MaxX => _maxX;
    private float _roomHeight = 14f; // Height of the room
    private float _roomWidth = 24.8125f; // Height of the room
    public float RoomHeight => _roomHeight;
    public float RoomWidth => _roomWidth;

    private void LateUpdate()
    {
        if (_playerRef != null)
        {
            // Get the current camera position
            Vector3 currentPosition = transform.position;

            // Update only the Y position to follow the player
            float targetX = Mathf.Clamp(_playerRef.position.x, _minX, _maxX); // Clamp the Y position within bounds
            float targetY = Mathf.Clamp(_playerRef.position.y, _minY, _maxY); // Clamp the Y position within bounds
            Vector3 targetPosition = new Vector3(targetX, targetY, currentPosition.z);

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

    public void AddMaxWidth(float width)
    {
        _maxX += width;
    }

    public void AddMinWidth(float width)
    {
        _minX += width;
    }

    public void MinMaxReset()
    {
        _minY = 0f;
        _maxY = 0f;
        _minX = 0f;
        _maxX = 0f;
    }
}
