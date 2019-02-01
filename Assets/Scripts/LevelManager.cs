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

    [SerializeField] Player player;
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
        flagAnim.SetInteger("BlizzardState", 0);
        GameObject[] lumbs = GameObject.FindGameObjectsWithTag(LUMB_TAG);
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
        CentralFire.instance.Reset();
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
        GameManager.manager.GameOver(1);
        //Debug.Log("PERDU GROS NAZE, ton feu s'est éteint ahahah nul");
    }

    public void LostFrozen()
    {
        AkSoundEngine.PostEvent("Stop_Music", gameObject);
        AkSoundEngine.PostEvent("Stop_SFX", gameObject);
        StopAllCoroutines();
        GameManager.manager.GameOver(2);
        //Debug.Log("PERDU GROS NAZE, la moitié de ton village est congelé, t'es tellement mauvais putain tu me fais pité...");
    }

    public void WonFire()
    {
        AkSoundEngine.PostEvent("Stop_Music", gameObject);
        AkSoundEngine.PostEvent("Stop_SFX", gameObject);
        StopAllCoroutines();
        GameManager.manager.Victory(1);
        //Debug.Log("GAGNÉ BG, ton feu il est tro bo");
    }

    public void WonNight()
    {
        AkSoundEngine.PostEvent("Stop_Music", gameObject);
        AkSoundEngine.PostEvent("Stop_SFX", gameObject);
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
        // FIN DE LA NUIT
        if(GameManager.manager.isPlaying)WonNight();
    }

    IEnumerator FogCoroutine(float goal, int rotation)
    {
        float firstValue = fog.Density;
        while (GameManager.manager.isPlaying)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            //print("fog:" + fog.Density);
            //print("goal:" + goal);
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
                //flagAnim.SetInteger("BlizzardState", 0);
                flagAnim.speed = 1;
                StartCoroutine(FogCoroutine(blizzardFog[0], blizzardFlag[0]));
                currentBlizzardState = 0;
                break;
            case 1:
                AkSoundEngine.SetState("BlizzardState", "Medium");
                newMain.simulationSpeed = 2;
                newRate.rateOverTime = valueParticleBlizzardStates[1];
                //flagAnim.SetInteger("BlizzardState", 1);
                flagAnim.speed = 1.5f;
                StartCoroutine(FogCoroutine(blizzardFog[1], blizzardFlag[1]));
                currentBlizzardState = 1;
                break;
            case 2:
                AkSoundEngine.SetState("BlizzardState", "High");
                newMain.simulationSpeed = 3;
                flagAnim.speed = 2;
                newRate.rateOverTime = valueParticleBlizzardStates[2];
                //flagAnim.SetInteger("BlizzardState", 2);
                StartCoroutine(FogCoroutine(blizzardFog[2], blizzardFlag[2]));
                currentBlizzardState =2;
                break;
            case 3:
                AkSoundEngine.SetState("BlizzardState", "Blizzard");
                newMain.simulationSpeed = 4;
                flagAnim.speed = 3;
                newRate.rateOverTime = valueParticleBlizzardStates[3];
                //flagAnim.SetInteger("BlizzardState", 3);
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