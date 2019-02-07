using System.Collections;
using System.Collections.Generic;
using UB;
using UnityEngine;

public class LevelManager : MonoBehaviour {

    public static string LUMB_TAG = "Lumb";
    public static string CENTRAL_FIRE_TAG = "CentralFire";
    public static string TREE_TAG = "Tree";
    public static string TORCHE_TAG = "Torche";
    public static string DOGGO_TAG = "Doggo";

    [SerializeField] public int _heatHelp = 50;
    [SerializeField] public int _heatWarm = 20;
    [SerializeField] public float SLOW_SPEED = 1;
    [SerializeField] public int lumbCapacity = 3;
    [SerializeField] public float _minSpeed = 0.2f;
    [SerializeField] public int _heatMax = 100;
    [SerializeField] public int _heatMin = -20;
    [SerializeField] public float _timeFreeze = 10;

    [SerializeField] public CentralFire centralFire;
    [SerializeField] public Player player;
    List<PNJ> listPNJ;

    [SerializeField] GameObject[] levels;
    public int currentLevel = 1;
    

    public int heatingModifier = 15;
    public int nightMinDuration = 10; // en minutes

    public int currentMinNightWait = 0;
    public int currentTimeBlizzardWait = 0;
    public int currentBlizzardColdModifier = 0;
    [SerializeField] int BlizzardModifier = 3;
    [SerializeField] ParticleSystem snowParticles;
    [SerializeField] int[] valueParticleBlizzardStates = new int[4];
    [SerializeField] int[] blizzardStates = new int[4];
    [SerializeField] float[] blizzardFog = new float[4];
    [SerializeField] int[] blizzardFlag = new int[4];
    public int blizzardDuration = 1; // en secondes
    [SerializeField] GameObject flag;
    [SerializeField] GameObject flagRotation;
    Animator flagAnim;

    public int nbVillagersAlive = 0;
    public int maxFire = 100;
    public int totalVillagers;

    public bool isBlizzardOn = false;
    int currentBlizzardState = 0;

    int seconds=0;

    public Texture2D hooverCursor;
    public Texture2D normalCursor;

    D2FogsPE fog;

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
        fog = Player.instance.cam.GetComponent<D2FogsPE>();
        SetLevel();
    }

    public void NextLevel()
    {
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

    public void SetLevel()
    {
        seconds = 0;
        currentTimeBlizzardWait = 0;
        currentMinNightWait = 0;
        for (int i = 0; i < levels.Length; i++)
        {
            levels[i].SetActive(false);
        }
        levels[currentLevel - 1].SetActive(true);
        ResetVillagers();
        currentTimeBlizzardWait = 0;
        currentMinNightWait = 0;
        nbVillagersAlive = totalVillagers;
        GameObject[] lumbs = GameObject.FindGameObjectsWithTag(LUMB_TAG);;
        for (int i = 0; i < lumbs.Length; i++)
        {
            Destroy(lumbs[i]);
        }
        if (trees == null) trees = GameObject.FindGameObjectsWithTag(TREE_TAG);
        for (int i = 0; i < trees.Length; i++)
        {
            trees[i].GetComponent<Tree>().Repop();
        }
        Blizzard(false);
    }

    public void ResetLevel()
    {
        SetLevel();
        centralFire.Reset();
        player.Reset();
        Play();
    }

    private void Start()
    {
        EventManager.StartListening(EventManager.PLAY_EVENT, Play);
    }

    void Play()
    {
        AkSoundEngine.PostEvent("Play_Amb", gameObject);
        AkSoundEngine.PostEvent("Play_Music", gameObject);
        AkSoundEngine.PostEvent("Play_Fire", gameObject);
        fog.Density = 0f;
        SetBlizzardState(0);
        StopAllCoroutines();
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
        AkSoundEngine.PostEvent("Stop_Music",gameObject);
        AkSoundEngine.PostEvent("Stop_SFX", gameObject);
        StopAllCoroutines();
        GameManager.manager.GameOver(2);
    }

    public void LostFrozen()
    {
        AkSoundEngine.PostEvent("Stop_Music", gameObject);
        AkSoundEngine.PostEvent("Stop_SFX", gameObject);
        StopAllCoroutines();
        GameManager.manager.GameOver(1);
    }

    public void WonFire()
    {
        AkSoundEngine.PostEvent("Stop_Music", gameObject);
        AkSoundEngine.PostEvent("Stop_SFX", gameObject);
        StopAllCoroutines();
        if (currentLevel >= levels.Length - 1) UIManager.manager.LastLevel();
        GameManager.manager.Victory(1);
        
    }

    public void WonNight()
    {
        AkSoundEngine.PostEvent("Stop_Music", gameObject);
        AkSoundEngine.PostEvent("Stop_SFX", gameObject);
        StopAllCoroutines();
        if (currentLevel >= levels.Length - 1) UIManager.manager.LastLevel();
        GameManager.manager.Victory(2);
        
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
                for(int i=0;i<blizzardStates.Length;i++)
                {
                    if (currentTimeBlizzardWait <= blizzardStates[i])
                    {
                        if(currentBlizzardState!=i) SetBlizzardState(i);
                        break;
                    }
                }
                if (currentTimeBlizzardWait >= blizzardStates[3])
                {
                    Blizzard(true);
                }
            }
            else if (isBlizzardOn && currentTimeBlizzardWait >= blizzardDuration)
            {
                Blizzard(false);
            }
        }
        if(GameManager.manager.isPlaying)WonNight();
    }

    IEnumerator FogCoroutine(float goal, int rotation)
    {
        float firstValue = fog.Density;
        while (GameManager.manager.isPlaying)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            flagRotation.transform.localEulerAngles = Vector3.Lerp(flagRotation.transform.localEulerAngles, new Vector3(flagRotation.transform.localEulerAngles.x, flagRotation.transform.localEulerAngles.y, rotation), 0.1f);
            if (firstValue<goal)
            {
                if (fog.Density < goal)
                {
                    fog.Density += 0.01f;
                }
                else
                {
                    fog.Density = goal;
                    break;
                }
            }
            else if (firstValue>goal)
            {
                if (fog.Density > goal)
                {
                    fog.Density -= 0.03f;
                }
                else
                {
                    fog.Density = goal;
                    break;
                }
            }
        }
    }

    void SetBlizzardState(int state)
    {
        ParticleSystem.EmissionModule newRate = snowParticles.emission;
        ParticleSystem.MainModule newMain = snowParticles.main;
        switch (state)
        {
            case 0:
                AkSoundEngine.SetState("BlizzardState", "Low");
                newRate.rateOverTime= valueParticleBlizzardStates[0];
                newMain.simulationSpeed = 1;
                flagAnim.speed = 1;
                StartCoroutine(FogCoroutine(blizzardFog[0], blizzardFlag[0]));
                currentBlizzardState = 0;
                break;
            case 1:
                AkSoundEngine.SetState("BlizzardState", "Medium");
                newMain.simulationSpeed = 2;
                newRate.rateOverTime = valueParticleBlizzardStates[1];
                flagAnim.speed = 1.5f;
                StartCoroutine(FogCoroutine(blizzardFog[1], blizzardFlag[1]));
                currentBlizzardState = 1;
                break;
            case 2:
                AkSoundEngine.SetState("BlizzardState", "High");
                newMain.simulationSpeed = 3;
                flagAnim.speed = 2;
                newRate.rateOverTime = valueParticleBlizzardStates[2];
                StartCoroutine(FogCoroutine(blizzardFog[2], blizzardFlag[2]));
                currentBlizzardState =2;
                break;
            case 3:
                AkSoundEngine.SetState("BlizzardState", "Blizzard");
                newMain.simulationSpeed = 4;
                flagAnim.speed = 3;
                newRate.rateOverTime = valueParticleBlizzardStates[3];
                StartCoroutine(FogCoroutine(blizzardFog[3], blizzardFlag[3]));
                currentBlizzardState = 3;
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
            SetBlizzardState(0);
        }
    }

    private void OnDisable()
    {
        EventManager.StopListening(EventManager.PLAY_EVENT, Play);
    }
}