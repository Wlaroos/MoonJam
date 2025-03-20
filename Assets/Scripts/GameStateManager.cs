using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    // State machine
    private enum GameState
    {
        Start,
        PreWave,
        DuringWave,
        PostWave,
        End,
        Paused
    }

    private GameState _currentState;
    private PointAdmin _spawnManager;

    [SerializeField] private GameObject pausedCanvas; // Reference to the paused canvas

    void Awake()
    {
        // Initialize the state
        _currentState = GameState.Start;
        _spawnManager = GetComponent<PointAdmin>();
        if (pausedCanvas != null)
        {
            pausedCanvas.SetActive(false); // Ensure the canvas is disabled at the start
        }
    }

    void Update()
    {
        // Check for pause input
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (_currentState == GameState.Paused)
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
            case GameState.End:
                HandleEndState();
                break;
            case GameState.Paused:
                HandlePausedState();
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
        // Logic for the PostWave state
        Debug.Log("Game is in PostWave state.");
        // Transition to End or PreWave based on game logic
        _currentState = GameState.End;
    }

    private void HandleEndState()
    {
        // Logic for the End state
        Debug.Log("Game is in End state.");
        // Game over logic here
    }

    private void HandlePausedState()
    {
        // Logic for the Paused state
        Debug.Log("Game is Paused.");
        // Wait for unpause
    }

    private void PauseGame()
    {
        _currentState = GameState.Paused;
        if (pausedCanvas != null)
        {
            pausedCanvas.SetActive(true); // Enable the paused canvas
        }
        Cursor.visible = true;
        Time.timeScale = 0f; // Freeze the game
    }

    public void UnpauseGame()
    {
        _currentState = GameState.DuringWave; // Resume the game state (adjust as needed)
        if (pausedCanvas != null)
        {
            pausedCanvas.SetActive(false); // Disable the paused canvas
        }
        Cursor.visible = false;
        Time.timeScale = 1f; // Resume the game
    }
}
