using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CentralFire : MonoBehaviour
{

    public int _levelFire = 0;

    public int levelFire {
        get { return _levelFire; }
    }

    public int _currentState = 0;

    //Sates of the fire
    public int[] statesArray;
    [SerializeField] int _delayBetweenDecrease = 3;
    [SerializeField] int VALUE_PER_LUMB = 5;
    [SerializeField] int START_FIRE = 40;
    [SerializeField] int _decreasePerSecond = 2;
    [SerializeField] public int _valueFireTaken = 5;
    [SerializeField] public int _valueFireGiven = 10;
    [SerializeField] public Transform fire;

    [SerializeField] Animator _animator;
    [SerializeField] Slider _slider;
    [SerializeField] float _offset_y = 5;

    [SerializeField] SpriteRenderer _sr;
    [SerializeField] Sprite baseSprite;
    [SerializeField] Sprite highlightSprite;

    private void Start()
    {
        EventManager.StartListening(EventManager.PLAY_EVENT, Play);

        Vector3 lSliderPos = new Vector3(transform.position.x, transform.position.y + _offset_y, transform.position.z);
        Vector3 lPos = Camera.main.WorldToScreenPoint(lSliderPos);
        _slider.transform.position = lPos;
        SetFeu();
    }

    void SetFeu()
    {
        _levelFire = START_FIRE;
        CheckState();
        _slider.value = _levelFire;
        _slider.gameObject.SetActive(false);
        _animator.SetInteger("FireState", _currentState);
    }

    public void Reset()
    {
        SetFeu();
        Play();
    }

    void Play()
    {
        StopAllCoroutines();
        StartCoroutine(DecreaseCoroutine());
    }

    void OnMouseOver()
    {
        if (!GameManager.manager.isPlaying) return;
        _slider.gameObject.SetActive(true);

        Cursor.SetCursor(LevelManager.manager.hooverCursor, Vector2.zero, CursorMode.Auto);
        _sr.sprite = highlightSprite;
        LevelManager.manager.isTargetFire = true;
    }

    void OnMouseExit()
    {
        if (!GameManager.manager.isPlaying) return;
        _slider.gameObject.SetActive(false);

        Cursor.SetCursor(LevelManager.manager.normalCursor, Vector2.zero, CursorMode.Auto);
        _sr.sprite = baseSprite;
        LevelManager.manager.isTargetFire = false;
    }

    private void OnMouseDown()
    {
        if (!GameManager.manager.isPlaying) return;
        LevelManager.manager.player.targetName = gameObject.name;
    }

    public bool UpdateFire(int pLumb, bool pIsUpgrade = true)
    {
        AkSoundEngine.SetRTPCValue("FireValue", _levelFire);

        if (pIsUpgrade)
        {
            UpdateState(pLumb);
        }
        if (!pIsUpgrade) return TakeFire();
        return true;
    }

    bool TakeFire()
    {
        if(_levelFire-_valueFireTaken>0)
        {
            _levelFire -= _valueFireTaken;
            _slider.value = _levelFire;
            CheckState();
            return true;
        }
        return false;
    }

    void UpdateState(int pLumb)
    {
        
        _levelFire += pLumb * VALUE_PER_LUMB;

        CheckState();
        _slider.value = _levelFire;
    }

    void DecreaseFire()
    {
        _levelFire -= _decreasePerSecond+LevelManager.manager.currentBlizzardColdModifier;


        CheckState();
    }

    void CheckState()
    {
        for (int i = 0; i<statesArray.Length; i++)
        {
            if (_levelFire <= statesArray[i])
            {
                _currentState = i;
                break;
            }
        }
        _animator.SetInteger("FireState", _currentState);

        if (_levelFire <= 0) LevelManager.manager.LostFire();
        else if (_levelFire >= LevelManager.manager.maxFire) LevelManager.manager.WonFire();
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
            if (_currentState == 0)
            {
                AkSoundEngine.SetState("FireState", "High");
            }
            if (_currentState == 1)
            {
                AkSoundEngine.SetState("FireState", "Mid");
            }
            if (_currentState == 2)
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
    private void OnDisable()
    {
        EventManager.StopListening(EventManager.PLAY_EVENT, Play);
    }
}
