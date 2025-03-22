using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    [SerializeField] private GameObject _pausedCanvas;
    [SerializeField] private GameObject _gameOverCanvas;
    [SerializeField] private GameObject _moveUpIndicator;
    [SerializeField] private GameObject _hordeIndicator;
    [SerializeField] private int _level1WaveCount;
    [SerializeField] private int _level2WaveCount;
    [SerializeField] private int _level3WaveCount;
    [SerializeField] private int _level4WaveCount;
    [SerializeField] private GameObject _afterTutorialRoom;
    [SerializeField] private GameObject _room;
    [SerializeField] private GameObject _checkpointRoom;

    private int _currentWaveCount;
    private int _currentLevel;
    private LineRenderer _waveBoundaryRenderer; // LineRenderer for wave boundary

    // State machine
    private enum GameState
    {
        TutorialStart,
        TutorialEnd,
        PreWave,
        DuringWave,
        PostWave,
        Transition
    }

    private GameState _currentState;
    private PointAdmin _spawnManager;
    private CameraFollow _cameraFollow;
    private bool _paused;
    void Awake()
    {
        _currentState = GameState.TutorialStart;
        _spawnManager = GetComponent<PointAdmin>();
        _cameraFollow = GetComponent<CameraFollow>();
        _waveBoundaryRenderer = GetComponent<LineRenderer>();

        if (_pausedCanvas != null)
        {
            _pausedCanvas.SetActive(false); // Ensure the canvas is disabled at the start
        }

        if (_waveBoundaryRenderer != null)
        {
            _waveBoundaryRenderer.enabled = false; // Disable the LineRenderer at the start
        }
    }

    void Start()
    {
        CreateRooms();
    }

    void Update()
    {
        // Check for pause input
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (_paused)
            {
                UnpauseGame();
            }
            else
            {
                PauseGame();
            }
        }

        switch (_currentState)
        {
            case GameState.TutorialStart:
                HandleTutorialStartState();
                break;
            case GameState.TutorialEnd:
                HandleTutorialEndState();
                break;
            case GameState.PreWave:
                HandlePreWaveState();
                break;
            case GameState.DuringWave:
                HandleDuringWaveState();
                break;
            case GameState.PostWave:
                HandlePostWaveState();
                break;
            case GameState.Transition:
                HandleTransitionState();
                break;
        }
    }

    private void HandleTutorialStartState()
    {
        // Logic for the Start state
        Debug.Log("Game is in Start state.");
    }
    
    private void HandleTutorialEndState()
    {
        // Logic for the TutorialEnd state
        Debug.Log("Game is in TutorialEnd state.");
        // Transition to PreWave
        if (Mathf.Abs(transform.position.x - _cameraFollow.MaxX) <= 0.01f)
        {
            transform.position =  new Vector3(_cameraFollow.MaxX, transform.position.y, transform.position.z);
            _cameraFollow.AddMinWidth(_cameraFollow.RoomWidth);

            _currentState = GameState.PreWave;
        }
    }

    private void HandlePreWaveState()
    {
        // Logic for the PreWave state
        Debug.Log("Game is in PreWave state.");
        // Transition to DuringWave
         _currentState = GameState.DuringWave;
    }

    private void HandleDuringWaveState()
    {
        // Logic for the DuringWave state
        Debug.Log("Game is in DuringWave state.");
        _spawnManager.SpawnEnemies();

        // Enable and update the wave boundary
        if (_waveBoundaryRenderer != null)
        {
            _waveBoundaryRenderer.enabled = true;
            UpdateWaveBoundary();
        }

        // Example condition to transition to PostWave
        if (_spawnManager.LiveEnemyList.Count == 0 && _spawnManager.MaxPoints <= 0)
        {
            _currentState = GameState.PostWave;

            // Disable the wave boundary when the wave ends
            if (_waveBoundaryRenderer != null)
            {
                _waveBoundaryRenderer.enabled = false;
            }
        }
    }

    private void UpdateWaveBoundary()
    {
        if (_waveBoundaryRenderer == null || Camera.main == null) return;

        // Get the camera's viewport bounds
        Camera cam = Camera.main;
        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 bottomRight = cam.ViewportToWorldPoint(new Vector3(1, 0, 0));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));
        Vector3 topLeft = cam.ViewportToWorldPoint(new Vector3(0, 1, 0));

        // Set the positions for the LineRenderer
        _waveBoundaryRenderer.positionCount = 5; // 4 corners + 1 to close the loop
        _waveBoundaryRenderer.SetPosition(0, new Vector3(bottomLeft.x, bottomLeft.y, 0));
        _waveBoundaryRenderer.SetPosition(1, new Vector3(bottomRight.x, bottomRight.y, 0));
        _waveBoundaryRenderer.SetPosition(2, new Vector3(topRight.x, topRight.y, 0));
        _waveBoundaryRenderer.SetPosition(3, new Vector3(topLeft.x, topLeft.y, 0));
        _waveBoundaryRenderer.SetPosition(4, new Vector3(bottomLeft.x, bottomLeft.y, 0)); // Close the loop

        _waveBoundaryRenderer.startColor = Color.red;
        _waveBoundaryRenderer.endColor = Color.red;

        _waveBoundaryRenderer.startWidth = .025f;
        _waveBoundaryRenderer.endWidth = .025f;
    }

    private void HandlePostWaveState()
    {
        Debug.Log("Game is in PostWave state.");

        _cameraFollow.AddMaxHeight(_cameraFollow.RoomHeight);
        Instantiate(_moveUpIndicator, new Vector3(transform.position.x, transform.position.y + 4.5f, 0), Quaternion.identity);

        _currentState = GameState.Transition;

        // Disable the wave boundary when transitioning
        if (_waveBoundaryRenderer != null)
        {
            _waveBoundaryRenderer.enabled = false;
        }
    }

    private void HandleTransitionState()
    {
        Debug.Log("Game is in Transition state.");

        if (Mathf.Abs(transform.position.y - _cameraFollow.MaxY) <= 0.01f)
        {
            transform.position =  new Vector3(transform.position.x, _cameraFollow.MaxY, transform.position.z);
            _cameraFollow.AddMinHeight(_cameraFollow.RoomHeight);
            _spawnManager.AddPoints(50);

            _currentState = GameState.PreWave;
        }
    }

    private void PauseGame()
    {
        _paused = true;
        if (_pausedCanvas != null)
        {
            _pausedCanvas.SetActive(true); // Enable the paused canvas
        }
        Cursor.visible = true;
        Time.timeScale = 0f; // Freeze the game
    }

    public void UnpauseGame()
    {
        _paused = false;
        if (_pausedCanvas != null)
        {
            _pausedCanvas.SetActive(false); // Disable the paused canvas
        }
        Cursor.visible = false;
        Time.timeScale = 1f; // Resume the game
    }

    public void TutorialEnded()
    {
        _currentState = GameState.TutorialEnd;
    }

    private void CreateRooms()
    {
        int[] waveCounts = { _level1WaveCount, _level2WaveCount, _level3WaveCount, _level4WaveCount };

        for (int level = 0; level < waveCounts.Length; level++)
        {
                for (int wave = 0; wave < waveCounts[level]; wave++)
                {
                    GameObject roomToInstantiate;

                    // Use the after tutorial room for the first room in level 1
                    if (level == 0 && level == 0 && wave == 0)
                    {
                        roomToInstantiate = _afterTutorialRoom;
                    }
                    // Use the checkpoint room for the last room of each level
                    else if (wave == waveCounts[level] - 1)
                    {
                        roomToInstantiate = _checkpointRoom;
                    }
                    else
                    {
                        roomToInstantiate = _room;
                    }

                    Instantiate(roomToInstantiate, new Vector3(level * _cameraFollow.RoomWidth, wave * _cameraFollow.RoomHeight , 0), Quaternion.identity);
                }

        }
    }
}
