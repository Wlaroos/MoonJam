using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    [SerializeField] private GameObject _pausedCanvas; // Reference to the paused canvas
    [SerializeField] private GameObject _gameOverCanvas; // Reference to the game over canvas
    [SerializeField] private GameObject _moveUpIndicator; // Reference to the move up indicator


    // State machine
    private enum GameState
    {
        Start,
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
        _currentState = GameState.Start;
        _spawnManager = GetComponent<PointAdmin>();
        _cameraFollow = GetComponent<CameraFollow>();

        if (_pausedCanvas != null)
        {
            _pausedCanvas.SetActive(false); // Ensure the canvas is disabled at the start
        }
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
            case GameState.Start:
                HandleStartState();
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

    private void HandleStartState()
    {
        // Logic for the Start state
        Debug.Log("Game is in Start state.");
        // Transition to PreWave
        _currentState = GameState.PreWave;
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

        // Example condition to transition to PostWave
        if (_spawnManager.LiveEnemyList.Count == 0 && _spawnManager.MaxPoints <= 0)
        {
            _currentState = GameState.PostWave;
        }
    }

    private void HandlePostWaveState()
    {
        Debug.Log("Game is in PostWave state.");

        _cameraFollow.AddMaxHeight(_cameraFollow.RoomHeight);
        Instantiate(_moveUpIndicator, new Vector3(transform.position.x, transform.position.y + 4.5f, 0), Quaternion.identity);

        _currentState = GameState.Transition;
    }

    private void HandleTransitionState()
    {
        Debug.Log("Game is in Transition state.");

        if (Mathf.Abs(transform.position.y - _cameraFollow.MaxY) <= 0.01f)
        {
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
}
