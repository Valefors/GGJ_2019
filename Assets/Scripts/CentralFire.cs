using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CentralFire : MonoBehaviour
{

    int _levelFire = 0;

    public int levelFire {
        get { return _levelFire; }
    }

    float _updateSize = 0.2f;
    int _currentState = 0;

    //Sates of the fire
    public int[] statesArray;
    [SerializeField] int _delayBetweenDecrease = 3;
    [SerializeField] int VALUE_PER_LUMB = 5;
    [SerializeField] int START_FIRE = 40;
    [SerializeField] int _decreasePerSecond = 2;

    int _animFireState = 0;

    [SerializeField] Animator _animator;
    [SerializeField] Slider _slider;
    [SerializeField] float _offset_y = 5;

    #region Singleton
    private static CentralFire _instance;
    public static CentralFire instance {
        get {
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null) _instance = this;

        else if (_instance != this) Destroy(gameObject);
    }
    #endregion

    private void Start()
    {
        StartCoroutine(DecreaseCoroutine());
        _currentState = START_FIRE;
        _levelFire = START_FIRE;

        _animFireState = 1;

        _animator.SetInteger("FireState", _animFireState);
        print(GameManager.manager.isPlaying);
        AkSoundEngine.PostEvent("Play_Fire", gameObject);
        AkSoundEngine.PostEvent("Play_Amb", gameObject);
        AkSoundEngine.PostEvent("Play_Music", gameObject);

        Vector3 lSliderPos = new Vector3(transform.position.x, transform.position.y + _offset_y, transform.position.z);
        Vector3 lPos = Camera.main.WorldToScreenPoint(lSliderPos);

        _slider.transform.position = lPos;
        _slider.gameObject.SetActive(false);
        _slider.value = _levelFire;
    }

    void OnMouseOver()
    {
        _slider.gameObject.SetActive(true);
    }

    void OnMouseExit()
    {
        _slider.gameObject.SetActive(false);
    }

    public void UpdateFire(int pLumb, bool pIsUpgrade = true)
    {
        AkSoundEngine.SetRTPCValue("FireValue", _levelFire);

        if (pIsUpgrade)
        {
            UpdateState(pLumb);
        }

        if (!pIsUpgrade) DecreaseFire();

        _slider.value = _levelFire;
    }

    void UpdateState(int pLumb)
    {
        _levelFire += pLumb * VALUE_PER_LUMB;

        if (_levelFire > LevelManager.manager.maxFire) LevelManager.manager.WonFire();

        if (IsNextState())
        {
            _animFireState++;
            _animator.SetInteger("FireState", _animFireState);
        }

        /*else
        {
            float lUpdate = _updateSize * pLumb;
            transform.localScale += new Vector3(lUpdate, lUpdate, 0);
        }*/
    }

    void DecreaseFire()
    {
        _levelFire -= _decreasePerSecond;
        if (_levelFire <= 0) LevelManager.manager.LostFire();

        if (IsBeforeState())
        {
            _animFireState--;
            _animator.SetInteger("FireState", _animFireState);
        }
    }

    bool IsNextState()
    {
        for (int i = 0; i < statesArray.Length; i++)
        {
            if (_levelFire > statesArray[i] && statesArray[i] > _currentState)
            {
                _currentState = statesArray[i];
                return true;
            }
        }

        return false;
    }

    bool IsBeforeState()
    {
        for (int i = 0; i < statesArray.Length; i++)
        {
            if (_levelFire <= statesArray[i] && statesArray[i] <= _currentState)
            {
                _currentState = statesArray[i];
                return true;
            }
        }

        return false;
    }

    public void Update()
    {
        if (GameManager.manager.isPlaying)
        {
            if (_animFireState == 0)
            {
                AkSoundEngine.SetState("FireState", "High");
            }
            if (_animFireState == 1)
            {
                AkSoundEngine.SetState("FireState", "Mid");
            }
            if (_animFireState == 2)
            {
                AkSoundEngine.SetState("FireState", "Low");
            }
        }
    }
    IEnumerator DecreaseCoroutine()
    {
        while (GameManager.manager.isPlaying)
        {
            yield return new WaitForSecondsRealtime(_delayBetweenDecrease);
            DecreaseFire();
        }
    }
}
