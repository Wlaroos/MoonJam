using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    [SerializeField] private GameObject _pausedCanvas;
    [SerializeField] private GameObject _gameOverCanvas;
    [SerializeField] private CutsceneCanvas _cutsceneCanvas;
    [SerializeField] private GameObject _moveUpIndicator;
    [SerializeField] private GameObject _hordeIndicator;
    [SerializeField] private int _level1WaveCount;
    [SerializeField] private int _level2WaveCount;
    [SerializeField] private int _level3WaveCount;
    [SerializeField] private int _level4WaveCount;
    [SerializeField] private GameObject _afterTutorialRoom;
    [SerializeField] private GameObject _room;
    [SerializeField] private GameObject _checkpointRoom;
    [SerializeField] private GameObject _finalRoom;
    [SerializeField] private GameObject _gunCrate;
    private GameObject _playerRef;
    private int _currentWaveCount;
    private int _currentLevel;
    public int CurrentLevel => _currentLevel;
    private float _cameraDistanceFromCenter = 0.1f;
    //private LineRenderer _waveBoundaryRenderer; // LineRenderer for wave boundary

    // Add a list to track rooms for the current level
    private List<GameObject> _currentLevelRooms = new List<GameObject>();
    private List<GameObject> _lastLevelRooms = new List<GameObject>();

    // State machine
    private enum GameState
    {
        TutorialStart,
        TutorialEnd,
        PreWave,
        DuringWave,
        PostWave,
        Transition,
        PreHorde,
        DuringHorde,
        PostHorde,
        MapCutscene,
        NextLevel,
        FinalArea,
        FinalCutscene,
    }

    private GameState _currentState;
    private PointAdmin _pointAdmin;
    private CameraFollow _cameraFollow;
    private bool _paused;
    void Awake()
    {
        _currentState = GameState.TutorialStart;
        _pointAdmin = GetComponent<PointAdmin>();
        _cameraFollow = GetComponent<CameraFollow>();
        _playerRef = GameObject.FindGameObjectWithTag("Player");

        //_waveBoundaryRenderer = GetComponent<LineRenderer>();

        if (_pausedCanvas != null)
        {
            _pausedCanvas.SetActive(false); // Ensure the canvas is disabled at the start
        }

        // if (_waveBoundaryRenderer != null)
        // {
        //     _waveBoundaryRenderer.enabled = false; // Disable the LineRenderer at the start
        // }
    }

    void Start()
    {
        CreateRooms();
    }

    void OnEnable()
    {
        _playerRef.GetComponent<PlayerHealth>().PlayerDeathEvent.AddListener(OnPlayerDeath);
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
            case GameState.PreHorde:
                HandlePreHordeState();
                break;
            case GameState.DuringHorde:
                HandleDuringHordeState();
                break;
            case GameState.PostHorde:
                HandlePostHordeState();
                break;
            case GameState.MapCutscene:
                HandleMapCutsceneState();
                break;
            case GameState.NextLevel:
                HandleNextLevelState();
                break;
            case GameState.FinalArea:
                HandleFinalAreaState();
                break;
            case GameState.FinalCutscene:
                HandleFinalCutsceneState();
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
        if (Mathf.Abs(transform.position.x - _cameraFollow.MaxX) <= _cameraDistanceFromCenter)
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
        _pointAdmin.SpawnEnemies();

        // // Enable and update the wave boundary
        // if (_waveBoundaryRenderer != null)
        // {
        //     _waveBoundaryRenderer.enabled = true;
        //     UpdateWaveBoundary();
        // }

        if (_pointAdmin.LiveEnemyList.Count == 0 && _pointAdmin.MaxPoints <= 0)
        {
            _currentState = GameState.PostWave;

            // // Disable the wave boundary when the wave ends
            // if (_waveBoundaryRenderer != null)
            // {
            //     _waveBoundaryRenderer.enabled = false;
            // }
        }
    }

    private void HandlePostWaveState()
    {
        Debug.Log("Game is in PostWave state.");

        _cameraFollow.AddMaxHeight(_cameraFollow.RoomHeight);
        GameObject moveUpIndicator = Instantiate(_moveUpIndicator, new Vector3(transform.position.x, transform.position.y + 4.5f, 0), Quaternion.identity);
        moveUpIndicator.transform.parent = transform;

        _currentState = GameState.Transition;

        _currentWaveCount++;
    }

    private void HandleTransitionState()
    {
        Debug.Log("Game is in Transition state.");

        if (Mathf.Abs(transform.position.y - _cameraFollow.MaxY) <= _cameraDistanceFromCenter)
        {
            transform.position = new Vector3(transform.position.x, _cameraFollow.MaxY, transform.position.z);
            _cameraFollow.AddMinHeight(_cameraFollow.RoomHeight);

            // Calculate points to add based on the current wave and level
            int pointsToAdd = 50 + (_currentWaveCount * 10) + (_currentLevel * 20);
            _pointAdmin.AddPoints(pointsToAdd);

            // Check if this is the second-to-last wave of the level
            if (_currentWaveCount == GetSecondToLastWaveCount())
            {
                _currentState = GameState.PreHorde;
                return;
            }

            _currentState = GameState.PreWave;
        }
    }

    private void HandlePreHordeState()
    {
        Debug.Log("Game is in PreHorde state.");

        // Spawn indicators
        GameObject hordeIndicator = Instantiate(_hordeIndicator, new Vector3(transform.position.x, transform.position.y - 5f, 0), Quaternion.identity);
        hordeIndicator.transform.parent = transform;

        GameObject moveUpIndicator = Instantiate(_moveUpIndicator, new Vector3(transform.position.x, transform.position.y + 4.5f, 0), Quaternion.identity);
        moveUpIndicator.transform.parent = transform;
        // Call SpawnHorde in the PointAdmin
        _pointAdmin.HordeSpawn(50);

        _cameraFollow.AddMaxHeight(_cameraFollow.RoomHeight);

        _currentState = GameState.DuringHorde;
    }

    private void HandleDuringHordeState()
    {
        Debug.Log("Game is in DuringHorde state.");
        
        if(_pointAdmin.LiveEnemyList.Count == 0 && _pointAdmin.HordeSpawningOver)
        {
            StartCoroutine(CrateSpawn());
            _currentState = GameState.PostHorde;
        }
    }

    private IEnumerator CrateSpawn()
    {
        yield return new WaitForSeconds(2f);
        Instantiate(_gunCrate, new Vector3(_cameraFollow.MaxX, _cameraFollow.MaxY, 0) , Quaternion.identity);
    }

    private void HandlePostHordeState()
    {
        Debug.Log("Game is in PostHorde state.");
    }
    
    public void NextLevelTrigger()
    {   
        // Create a copy of _currentLevelRooms instead of referencing it
        _lastLevelRooms = new List<GameObject>(_currentLevelRooms);

        if (_currentLevel >= 3)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("FinalCutscene");
        }
        else
        {
            _cutsceneCanvas.FadeIn();
            _currentState = GameState.MapCutscene;
            _currentLevel++;
            _currentWaveCount = 0;

            // Update the point pool based on the current level
            switch (_currentLevel)
            {
                case 1:
                    _pointAdmin.AllowSmallZombie = true;
                    _pointAdmin.UpdateEnemyList();
                    break;
                case 2:
                    _pointAdmin.AllowBigZombie = true;
                    _pointAdmin.UpdateEnemyList();
                    break;
                default:
                    break;
            }

            // Load rooms for the next level
            CreateRooms();
        }
    }

    private void HandleMapCutsceneState()
    {
        Debug.Log("Game is in MapCutscene state.");
    }

    private void HandleNextLevelState()
    {
        Debug.Log("Game is in NextLevel state.");
        
        _playerRef.transform.position = new Vector3(_cameraFollow.RoomWidth * _currentLevel, 0, 0);
        _cameraFollow.MinMaxReset();
        _cameraFollow.AddMaxWidth(_cameraFollow.RoomWidth * _currentLevel);
        _cameraFollow.AddMinWidth(_cameraFollow.RoomWidth * _currentLevel);

        // Clear the rooms from the previous level
        foreach (GameObject room in _lastLevelRooms)
        {
            Destroy(room);
        }

        _currentState = GameState.PreWave;
    }

    private void HandleFinalAreaState()
    {
        Debug.Log("Game is in FinalArea state.");
    }

    private void HandleFinalCutsceneState()
    {
        Debug.Log("Game is in FinalCutscene state.");
    }

    // Helper method to calculate the second-to-last wave count for the current level
    private int GetSecondToLastWaveCount()
    {
        int[] waveCounts = { _level1WaveCount, _level2WaveCount, _level3WaveCount, _level4WaveCount };
        return waveCounts[_currentLevel] - 2; // Second-to-last wave
    }
    public void TutorialEnded()
    {
        _currentState = GameState.TutorialEnd;
    }

    public void NextLevel()
    {
        _currentState = GameState.NextLevel;
    }

    // private void UpdateWaveBoundary()
    // {
    //     if (_waveBoundaryRenderer == null || Camera.main == null) return;

    //     // Get the camera's viewport bounds
    //     Camera cam = Camera.main;
    //     Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
    //     Vector3 bottomRight = cam.ViewportToWorldPoint(new Vector3(1, 0, 0));
    //     Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));
    //     Vector3 topLeft = cam.ViewportToWorldPoint(new Vector3(0, 1, 0));

    //     // Set the positions for the LineRenderer
    //     _waveBoundaryRenderer.positionCount = 5; // 4 corners + 1 to close the loop
    //     _waveBoundaryRenderer.SetPosition(0, new Vector3(bottomLeft.x, bottomLeft.y, 0));
    //     _waveBoundaryRenderer.SetPosition(1, new Vector3(bottomRight.x, bottomRight.y, 0));
    //     _waveBoundaryRenderer.SetPosition(2, new Vector3(topRight.x, topRight.y, 0));
    //     _waveBoundaryRenderer.SetPosition(3, new Vector3(topLeft.x, topLeft.y, 0));
    //     _waveBoundaryRenderer.SetPosition(4, new Vector3(bottomLeft.x, bottomLeft.y, 0)); // Close the loop

    //     _waveBoundaryRenderer.startColor = Color.red;
    //     _waveBoundaryRenderer.endColor = Color.red;

    //     _waveBoundaryRenderer.startWidth = .025f;
    //     _waveBoundaryRenderer.endWidth = .025f;
    // }

    private void CreateRooms()
    {
        // Clear the list of rooms for the current level
        _currentLevelRooms.Clear();

        int[] waveCounts = { _level1WaveCount, _level2WaveCount, _level3WaveCount, _level4WaveCount };
        int waveCountForCurrentLevel = waveCounts[_currentLevel];

        for (int wave = 0; wave < waveCountForCurrentLevel; wave++)
        {
            GameObject roomToInstantiate;

            // Use the after tutorial room for the first room in level 1
            if (_currentLevel == 0 && wave == 0)
            {
                roomToInstantiate = _afterTutorialRoom;
            }
            // Use the checkpoint room for the last room of the level
            else if (wave == waveCountForCurrentLevel - 1)
            {
                roomToInstantiate = _checkpointRoom;
            }
            else
            {
                roomToInstantiate = _room;
            }

            GameObject instantiatedRoom = Instantiate(
                roomToInstantiate,
                new Vector3(_currentLevel * _cameraFollow.RoomWidth, wave * _cameraFollow.RoomHeight, 0),
                Quaternion.identity
            );

            // Add the instantiated room to the list
            _currentLevelRooms.Add(instantiatedRoom);
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

        _cutsceneCanvas.gameObject.SetActive(false);

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

        _cutsceneCanvas.gameObject.SetActive(true);

        Time.timeScale = 1f; // Resume the game
    }

    private void OnPlayerDeath()
    {
        _gameOverCanvas.SetActive(true);
        Cursor.visible = true;
        _cutsceneCanvas.gameObject.SetActive(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
