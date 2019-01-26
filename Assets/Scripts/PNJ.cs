﻿using System.Collections;
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

    [SerializeField] protected int _speed = 2;
    [SerializeField] int INITIAL_SPEED = 10;
    [SerializeField] int SLOW_SPEED = 2;
    [SerializeField] int lumbCapacity = 3;

    static int frozen = 0;
    static int cold = 1;
    static int warm = 2;
    static int help = 3;

    public int state = warm;

    protected float _timeSpent=0;
    protected bool _isMoving = false;
    protected bool _hasReachedTarget=false;
    protected GameObject _moveTarget;

    int _numberLumbs = 0;


    // Use this for initialization
    void Start () {
        HeatCheck();
	}
	
	// Update is called once per frame
	void Update () {
        _timeSpent += Time.deltaTime;
        HeatCount();
        if (_isMoving && _moveTarget != null)
        {
            MoveTo(_moveTarget);
        }
	}

    private void OnTriggerEnter(Collider pCol)
    {
        if (pCol.gameObject.tag == LevelManager.LUMB_TAG)
        {
            if (_numberLumbs <= lumbCapacity)
            {
                TakeLumb(pCol.gameObject);
                Work();
            }

        }

        if (pCol.gameObject.tag == LevelManager.CENTRAL_FIRE_TAG)
        {
            if (_numberLumbs > 0) UpdateFire();
        }
    }

    void UpdateFire()
    {
        if (_numberLumbs > 0) CentralFire.instance.UpdateFire(_numberLumbs);
        _numberLumbs = 0;
        _speed = INITIAL_SPEED;
    }

    void TakeLumb(GameObject pLumb)
    {
        _numberLumbs++;
        _speed -= SLOW_SPEED;

        Destroy(pLumb);
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
        // GERER CHANGEMENTS VISUELS
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

    float GetDistance(GameObject obj)
    {
        return Vector3.Distance(transform.position, obj.transform.position);
    }

    void Work()
    {
        if(_numberLumbs<lumbCapacity)
        {
            GameObject[] lumbs = GameObject.FindGameObjectsWithTag("Lumb");
            Debug.Log("lumbs ?:" + lumbs.Length);
            if (lumbs != null)
            {
                float distanceMin = GetDistance(lumbs[0]);
                int id = 0;
                for (int i = 0; i < lumbs.Length; i++)
                {
                    if (GetDistance(lumbs[i]) < distanceMin)
                    {
                        distanceMin = GetDistance(lumbs[i]);
                        id = i;
                    }
                }
                _moveTarget = lumbs[id];
                _isMoving = true;
            }
        }
        else
        {
            _moveTarget = 
        }
    }

    void MoveTo(GameObject obj)
    {
        transform.position = Vector3.MoveTowards(transform.position, obj.transform.position, _speed*Time.deltaTime);
        if (transform.position == obj.transform.position)
        {
            _hasReachedTarget = true;
            _isMoving = false;
            _moveTarget = null;
        }
    }
}
