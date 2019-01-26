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
    [SerializeField] int[] _statesArray;
    [SerializeField] int _delayBetweenDecrease = 5;

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
        print(GameManager.manager.isPlaying);
    }

    public void UpdateFire(int pLumb = 0, bool pIsUpgrade = true)
    {
        if (pIsUpgrade)
        {
            UpdateState(pLumb);
        }

        if (!pIsUpgrade) DecreaseFire();
    }

    void UpdateState(int pLumb)
    {
        _levelFire += pLumb;

        if (IsNextState())
        {
            print("NEW STATE");
            if (_currentState == _statesArray[_statesArray.Length - 1]) print("WIN");
        }

        else
        {
            float lUpdate = _updateSize * pLumb;
            transform.localScale += new Vector3(lUpdate, lUpdate, 0);
        }
    }

    void DecreaseFire()
    {
        _levelFire--;
        if (_levelFire < 0) print("DEFEAT");
    }

    bool IsNextState()
    {
        for (int i = 0; i < _statesArray.Length; i++)
        {
            if (_levelFire >= _statesArray[i] && _statesArray[i] > _currentState)
            {
                _currentState = _statesArray[i];
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
