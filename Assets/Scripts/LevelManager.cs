﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {

    public static string LUMB_TAG = "Lumb";
    public static string CENTRAL_FIRE_TAG = "CentralFire";
    public static string TREE_TAG = "Tree";
    public static string TORCHE_TAG = "Torche";
    public static string DOGGO_TAG = "Doggo";

    [SerializeField] Player player;
    List<PNJ> listPNJ;

    [SerializeField] GameObject[] levels;
    public int currentLevel = 1;
    

    public int heatingModifier = 15;
    public int nightMinDuration = 10; // en minutes

    public int currentMinNightWait = 0;
    public float currentTimeBlizzardWait = 0;
    public float blizzardDuration = 1; // en secondes
    public int currentBlizzardColdModifier = 0;
    [SerializeField] int BlizzardModifier = 3;
    [SerializeField] ParticleSystem snowParticles;
    [SerializeField] int[] valueParticleBlizzardStates = new int[4];

    public int[] blizzardStates = new int[4];
    [SerializeField] GameObject flag;
    Animator flagAnim;

    public int nbVillagersAlive = 0;
    public int maxFire = 100;
    public int totalVillagers;

    bool isBlizzardOn = false;

    int seconds=0;

    public Texture2D hooverCursor;
    public Texture2D normalCursor;

    GameObject[] trees;

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

        flagAnim = flag.GetComponent<Animator>();

        listPNJ = new List<PNJ>();
        ResetLevel();
    }

    public void NextLevel()
    {
       /* Destroy(Player.instance);
        Destroy(CentralFire.instance);*/
        if(currentLevel<levels.Length) currentLevel++;
        trees = null;
        ResetLevel();
    }

    public void ResetVillagers()
    {
        listPNJ.Clear();
        GameObject[] temp = GameObject.FindGameObjectsWithTag("PNJ");
        for (int i = 0; i < temp.Length; i++)
        {
            listPNJ.Add(temp[i].GetComponent<PNJ>());
            listPNJ[i].Reset();
        }
        totalVillagers = listPNJ.Count;
    }

    public void ResetLevel()
    {
        currentTimeBlizzardWait = 0;
        currentMinNightWait = 0;
        isBlizzardOn = true;
        for (int i = 0; i < levels.Length; i++)
        {
            levels[i].SetActive(false);
        }
        levels[currentLevel - 1].SetActive(true);
        ResetVillagers();
        Start();
        CentralFire.instance.Reset();
        player.Reset();

        GameObject[] lumbs = GameObject.FindGameObjectsWithTag(LUMB_TAG);
        for (int i = 0; i < lumbs.Length; i++)
        {
            Destroy(lumbs[i]);
        }
        
        if(trees==null) trees = GameObject.FindGameObjectsWithTag(TREE_TAG);
        for (int i = 0; i < trees.Length; i++)
        {
            trees[i].GetComponent<Tree>().Repop();
        }
        Play();
    }

    private void Start()
    {
        currentTimeBlizzardWait = 0;
        currentMinNightWait = 0;
        nbVillagersAlive = totalVillagers;
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
        StopAllCoroutines();
        GameManager.manager.GameOver(1);
        //Debug.Log("PERDU GROS NAZE, ton feu s'est éteint ahahah nul");
    }

    public void LostFrozen()
    {
        StopAllCoroutines();
        GameManager.manager.GameOver(2);
        //Debug.Log("PERDU GROS NAZE, la moitié de ton village est congelé, t'es tellement mauvais putain tu me fais pité...");
    }

    public void WonFire()
    {
        StopAllCoroutines();
        GameManager.manager.Victory(1);
        //Debug.Log("GAGNÉ BG, ton feu il est tro bo");
    }

    public void WonNight()
    {
        StopAllCoroutines();
        GameManager.manager.Victory(2);
        //Debug.Log("GAGNÉ BG, ta passé la nuit, c'est moins cool mais t'as quand même gagné (deso pas deso)");
    }

    IEnumerator TimeCoroutine()
    {
        while (GameManager.manager.isPlaying)
        {
            yield return new WaitForSecondsRealtime(1);
            seconds++;
            currentTimeBlizzardWait++;
            if (seconds==60)
            {
                seconds = 0;
                currentMinNightWait++;
                if (currentMinNightWait >= nightMinDuration)
                {
                    break;
                }
            }
            if (!isBlizzardOn)
            {
                for(int i=blizzardStates.Length-1;i>=0;i--)
                {
                    if (currentTimeBlizzardWait <= blizzardStates[i]) SetBlizzardState(i);
                }
                if (currentTimeBlizzardWait >= blizzardDuration)
                {
                    Blizzard(true);
                }
            }
            else if (isBlizzardOn && currentTimeBlizzardWait >= blizzardStates[3])
            {
                Blizzard(false);
            }
        }
        // FIN DE LA NUIT
        if(GameManager.manager.isPlaying)WonNight();
    }

    void SetBlizzardState(int state)
    {
        ParticleSystem.EmissionModule newRate = snowParticles.emission;
        switch (state)
        {
            case 0:
                Debug.Log("Etat Blizzard 1");
                newRate.rateOverTime= valueParticleBlizzardStates[0];
                flagAnim.SetInteger("BlizzardState", 0);
                break;
            case 1:
                Debug.Log("Etat Blizzard 2");
                newRate.rateOverTime = valueParticleBlizzardStates[1];
                flagAnim.SetInteger("BlizzardState", 1);
                break;
            case 2:
                Debug.Log("Etat Blizzard 3");
                newRate.rateOverTime = valueParticleBlizzardStates[2];
                flagAnim.SetInteger("BlizzardState", 2);
                break;
            case 3:
                Debug.Log("Etat Blizzard 4");
                newRate.rateOverTime = valueParticleBlizzardStates[3];
                flagAnim.SetInteger("BlizzardState", 3);
                break;
        }
    }


    void Blizzard(bool b)
    {
        currentTimeBlizzardWait = 0;
        isBlizzardOn = b;
        if(isBlizzardOn)
        {
            currentBlizzardColdModifier = BlizzardModifier;
        }
        else
        {
            currentBlizzardColdModifier = 0;
        }

        // CODER LE BLIZZARD
    }

    private void OnDisable()
    {
        EventManager.StopListening(EventManager.PLAY_EVENT, Play);
    }
}
