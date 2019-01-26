using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentralFire : MonoBehaviour {

    int _levelFire = 0;
    float _updateSize = 0.2f;

    //Sates of the fire
    [SerializeField] int[] _statesArray;

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

    public void UpdateFire(int pLumb, bool pIsUpgrade = true)
    {
        if (pIsUpgrade)
        {
            UpdateState(pLumb);
        }

        if (!pIsUpgrade) _levelFire--;
    }

    void UpdateState(int pLumb)
    {
        _levelFire += pLumb;

        
        float lUpdate = _updateSize * pLumb;

        transform.localScale += new Vector3(lUpdate, lUpdate, 0);
    }

    /*bool IsNextState()
    {
        for (int i = 0; i < _statesArray.Length; i++)
        {
            if (_levelFire)
        }
    }*/
}
