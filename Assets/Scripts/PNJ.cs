using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PNJ : MonoBehaviour {

    #region Warmth
    [SerializeField] protected int _heat =30;
    [SerializeField] protected int _heatMax=100;
    [SerializeField] protected int _heatMin = -20;
    [SerializeField] protected float _timeFreeze = 10;

    [SerializeField] protected int _heatHelp = 60;
    [SerializeField] protected int _heatWarm = 20;
    #endregion

    static int frozen = 0;
    static int cold = 1;
    static int warm = 2;
    static int help = 3;

    protected int state = warm;

    protected float _timeSpent=0;

    // Use this for initialization
    void Start () {
        HeatCheck();
	}
	
	// Update is called once per frame
	void Update () {
        _timeSpent += Time.deltaTime;
        HeatCount();
	}

    void HeatCount()
    {
        if (_timeSpent >= _timeFreeze)
        {
            if(_heat>_heatMin) _heat--;
            _timeSpent = 0;
            HeatCheck();
        }
    }

    void HeatCheck()
    {
        if (_heat >= _heatHelp)
        {
            Help();
        }
        else if (_heat >= _heatWarm)
        {
            Warm();
        }
        else if (_heat <0)
        {
            Freeze();
        }
        else if (_heat<_heatWarm)
        {
            Cold();
        }
    }

    public void Cold()
    {
        state = cold;
        // GERER CHANGEMENTS VISUELS
    }

    public void Help()
    {
        state = help;
        Work();
    }

    public void Freeze()
    {
        state = frozen;
        // GERER CHANGEMEMENTS VISUELS
    }

    public void Warm()
    {
        state = warm;
        // GERER CHANGEMEMENTS VISUELS
    }

    public void AddHeat(int heatModifier)
    {
        _heat += heatModifier;
        if (_heat > _heatMax) _heat = _heatMax;
        _timeSpent = 0;
    }

    void Work()
    {

    }
}
