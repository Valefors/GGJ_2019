using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    private static GameManager _manager;
    public static GameManager manager {
        get {
            return _manager;
        }
    }

    public bool isPlaying = false;

    private void Awake()
    {
        if (_manager == null) _manager = this;

        else if (_manager != this) Destroy(gameObject);
    }

    // Use this for initialization
    void Start () {
        isPlaying = true;
	}
	
	public void Play()
    {
        isPlaying = true;
        EventManager.TriggerEvent(EventManager.PLAY_EVENT);
    }

    public void Victory()
    {
        isPlaying = false;
        EventManager.TriggerEvent(EventManager.VICTORY_EVENT);
    }

    public void GameOver()
    {
        isPlaying = false;
        EventManager.TriggerEvent(EventManager.GAME_OVER_EVENT);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Menu()
    {
        isPlaying = false;
        EventManager.TriggerEvent(EventManager.MENU_EVENT);
    }

    public void Pause()
    {
        //A RECODER
        isPlaying = false;
        Time.timeScale = 0;
        EventManager.TriggerEvent(EventManager.PAUSE_EVENT);
    }

    public void Resume()
    {
        //A RECODER
        isPlaying = true;
        Time.timeScale = 1;
        EventManager.TriggerEvent(EventManager.RESUME_EVENT);
    }
}
