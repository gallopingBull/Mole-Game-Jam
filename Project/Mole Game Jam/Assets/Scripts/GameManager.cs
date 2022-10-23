using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    [SerializeField ]private GameData _gameData;
    [SerializeField ]private GameObject _musicSource;

    private int _babyCount = 3; 
    private int _foodSaved = 0;
    private int _minFoodRequired = 10;
    private int _introCounter;

    private float _moleBabiesHungerValue = 10;
    private float _introLength = 38f; // get length from timeline

    private bool _gameStarted = false;
    private bool _introPlaying = false;

    [SerializeField] GameObject _directorTImeline; 
    public static GameManager Instance { get => _instance; set => _instance = value; }
    public float MoleBabiesHungerValue { get => _moleBabiesHungerValue; set => _moleBabiesHungerValue = value; }
    public int BabyCount { get => _babyCount; set => _babyCount = value; }
    public bool GameStarted { get => _gameStarted; set => _gameStarted = value; }
    public bool IntroPlaying { get => _introPlaying; set => _introPlaying = value; }
    public GameData GameData { get => _gameData; }

    void Awake() 
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        InitGame();
    }

    private void InitGame()
    {
        GameEvents.OnTimerFinished += GameComplete;
        GameEvents.OnFoodSaved += SaveFood;
        GameEvents.OnFoodRemoved += RemoveFood;
        GameEvents.OnGameBegin += StartGame;
        _directorTImeline = GameObject.Find("DirectorTimeline");
        BeginIntroCutscene();
    }

    private void BeginIntroCutscene() => StartCoroutine("IntroCutscene");

    private IEnumerator IntroCutscene()
    {
        _introPlaying = true;
        _introCounter = (int)_introLength;
        while (_introCounter > 0)
        {
            // enable ai movement for intro cutscene 
            if (_introCounter == 8)
                PlayerController.Instance.EnableMainLight();
            if (_introCounter == _introLength - 28)
                PlayerController.Instance.NavMeshAgent.enabled = true;

            yield return new WaitForSeconds(1);
            _introCounter--;
        }
        GameEvents.OnGameBegin?.Invoke();
    }
    
    public void StartGame()
    {
        _directorTImeline.SetActive(false);
        UIManager.Instance.HideIntoCutsceneDialog();
        SoundManager.PlayMusicTrack();
        StopCoroutine("IntroCutscene");
        PlayerController.Instance.EnableMovement = true;
        PlayerController.Instance.EnableMainLight();
        _gameStarted = true;
        PlayerController.Instance.NavMeshAgent.enabled = false;
        PlayerController.Instance.EnableInput = true;
        PlayerController.Instance.EnableMovement = true;
        CameraManager.Instance.EnableMainCamera();
        _introPlaying = false;
    }

    public void GameOver(FailStates failState) 
    {
        _gameStarted = false;
        UIManager.Instance.HideHUD();
        PlayerController.Instance.EnableInput = false;
        PlayerController.Instance.EnableMovement = false;
        UIManager.Instance.DisplayEndMenu(failState);
    }

    private void GameComplete()
    {
        _gameStarted = false;
        UIManager.Instance.HideHUD();
        PlayerController.Instance.EnableInput = false;
        PlayerController.Instance.EnableMovement = false;
        GetEndResults();
    }

    private void GetEndResults()
    {
        // random perecentage of child(ren) dying based on how
        // much food was saved and total children alive
        string resultSummary = "";
        if (_foodSaved >= _minFoodRequired)
            resultSummary = $"You've accumulated enough food to feed your babies - {_babyCount + 1} mole(s) survied";
        else
        {
            float d = Random.Range(0f, 100f);
            if ((d -= 30) < 0)
            {
                _babyCount -= 2;
                resultSummary = $"You didn't collect enough food for the Winter - only {_babyCount + 1} mole(s) survied \n" +
                    $"The rest died of hunger...";
            };
            if ((d -= 12) < 0)
            {
                resultSummary = $"You didn't collect enough food for the Winter \n" +
                    $"All your babies died...";
            };
            if ((d -= 45) < 0)
            {
                _babyCount--;
                resultSummary = $"You didn't collect enough food for the Winter - only {_babyCount + 1} mole(s) survied \n" +
                  $"One died of hunger...";
            };
        }
        
        UIManager.Instance.DisplayEndMenu(resultSummary);
    }

    public void OnDisable()
    {
        GameEvents.OnTimerFinished -= GameComplete;
        GameEvents.OnFoodSaved -= SaveFood;
        GameEvents.OnFoodRemoved -= RemoveFood;
        GameEvents.OnGameBegin -= StartGame;
    }

    private void SaveFood(int value)
    {
        _foodSaved += value;
        Debug.Log($"food saved:{_foodSaved}");
    }

    private void RemoveFood(int value)
    {
        _foodSaved += value;
        Debug.Log($"food removed:{_foodSaved}");
    }

    public void EatBaby()
    {
        _babyCount--;
        if (_babyCount == 0)
            GameOver(FailStates.babiesDied);
    }
}

public enum FailStates
{
    playerDied, babiesDied
}
