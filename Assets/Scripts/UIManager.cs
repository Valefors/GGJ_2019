using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

    private static UIManager _manager;
    public static UIManager manager {
        get {
            return _manager;
        }
    }

    [SerializeField] RectTransform _menuScreen;
    [SerializeField] RectTransform _pauseScreen;
    [SerializeField] RectTransform _victoryScreen;
    [SerializeField] RectTransform _gameOverScreen;
    [SerializeField] RectTransform _ingameScreen;
    [SerializeField] RectTransform _creditsScreen;

    RectTransform _currentScreen;

    private void Awake()
    {
        if (_manager == null) _manager = this;

        else if (_manager != this) Destroy(gameObject);
    }

    // Use this for initialization
    void Start () {
        EventManager.StartListening(EventManager.PLAY_EVENT, Play);
        EventManager.StartListening(EventManager.VICTORY_EVENT, Victory);
        EventManager.StartListening(EventManager.GAME_OVER_EVENT, GameOver);
        EventManager.StartListening(EventManager.MENU_EVENT, Menu);
        EventManager.StartListening(EventManager.PAUSE_EVENT, Pause);
        EventManager.StartListening(EventManager.RESUME_EVENT, Resume);

        _currentScreen = _menuScreen;
        //_currentScreen.gameObject.SetActive(true);
    }
	
    void Play()
    {
        _currentScreen.gameObject.SetActive(false);

        _currentScreen = _ingameScreen;

        _currentScreen.gameObject.SetActive(true);
    }

    void Victory()
    {
        _currentScreen.gameObject.SetActive(false);

        _currentScreen = _victoryScreen;

        _currentScreen.gameObject.SetActive(true);
    }

    void GameOver()
    {
        _currentScreen.gameObject.SetActive(false);

        _currentScreen = _gameOverScreen;

        _currentScreen.gameObject.SetActive(true);
    }

    void Menu()
    {
        _currentScreen.gameObject.SetActive(false);

        _currentScreen = _menuScreen;

        _currentScreen.gameObject.SetActive(true);
    }

    void Pause()
    {
        _currentScreen.gameObject.SetActive(false);

        _currentScreen = _pauseScreen;

        _currentScreen.gameObject.SetActive(true);
    }

    void Resume()
    {
        _currentScreen.gameObject.SetActive(false);

        _currentScreen = _ingameScreen;

        _currentScreen.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Destroy () {
        EventManager.StopListening(EventManager.PLAY_EVENT, Play);
        EventManager.StopListening(EventManager.VICTORY_EVENT, Victory);
        EventManager.StopListening(EventManager.GAME_OVER_EVENT, GameOver);
        EventManager.StopListening(EventManager.MENU_EVENT, Menu);
        EventManager.StopListening(EventManager.PAUSE_EVENT, Pause);
        EventManager.StopListening(EventManager.RESUME_EVENT, Resume);
    }
}
