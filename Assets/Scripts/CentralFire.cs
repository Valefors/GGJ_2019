using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }

    public void UpdateFire(int pLumb, bool pIsUpgrade = true)
    {
        if (pIsUpgrade)
        {
            UpdateState(pLumb);
        }

        if (!pIsUpgrade) DecreaseFire();
    }

    void UpdateState(int pLumb)
    {
        _levelFire += pLumb * VALUE_PER_LUMB;

        if (_levelFire > 99) print("WIN");

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
        if (_levelFire <= 0) print("DEFEAT");

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

    IEnumerator DecreaseCoroutine()
    {
        while (GameManager.manager.isPlaying)
        {
            yield return new WaitForSecondsRealtime(_delayBetweenDecrease);
            DecreaseFire();
        }
    }
}
