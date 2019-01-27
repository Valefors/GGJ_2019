using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {

    public static string LUMB_TAG = "Lumb";
    public static string CENTRAL_FIRE_TAG = "CentralFire";
    public static string TREE_TAG = "Tree";
    public static string TORCHE_TAG = "Torche";

    public bool isPlaying = true;

    public int heatingModifier = 15;
    public int nightMinDuration = 10; // en minutes

    public int currentMinWait = 0;

    public int nbVillagersAlive = 0;
    public int maxFire = 100;
    public int totalVillagers;

    private static LevelManager _manager;
    public static LevelManager manager {
        get {
            return _manager;
        }
    }

    private void Awake()
    {
        if (_manager == null) _manager = this;

        else if (_manager != this) Destroy(gameObject);
    }

    private void Start()
    {
        EventManager.StartListening(EventManager.PLAY_EVENT, Play);
    }

    void Play()
    {
        StartCoroutine(TimeCoroutine());
    }

    private void Update()
    {
        if (!GameManager.manager.isPlaying) return;

        if (nbVillagersAlive <= (totalVillagers / 2))
        {
            LostFrozen();
        }
    }

    public void LostFire()
    {
        GameManager.manager.GameOver();
        Debug.Log("PERDU GROS NAZE, ton feu s'est éteint ahahah nul");
    }

    public void LostFrozen()
    {
        GameManager.manager.GameOver();
        Debug.Log("PERDU GROS NAZE, la moitié de ton village est congelé, t'es tellement mauvais putain tu me fais pité...");
    }

    public void WonFire()
    {
        GameManager.manager.Victory();
        Debug.Log("GAGNÉ BG, ton feu il est tro bo");
    }

    public void WonNight()
    {
        GameManager.manager.Victory();
        Debug.Log("GAGNÉ BG, ta passé la nuit, c'est moins cool mais t'as quand même gagné (deso pas deso)");
    }

    IEnumerator TimeCoroutine()
    {
        while (GameManager.manager.isPlaying)
        {
            yield return new WaitForSecondsRealtime(60);
            currentMinWait++;
            if (currentMinWait >= nightMinDuration) break;
        }
        // FIN DE LA NUIT
        WonNight();
    }

    private void OnDisable()
    {
        EventManager.StopListening(EventManager.PLAY_EVENT, Play);
    }
}
