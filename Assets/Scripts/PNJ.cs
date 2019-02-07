﻿using PolyNav;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PNJ : MonoBehaviour {

    public int _heat = 30;
    [SerializeField] protected int INITIAL_HEAT = 30;
    #region Warmth

    #endregion

    [SerializeField] float INITIAL_SPEED = 4;

    [SerializeField] Transform tentPosition;

    Tree lastTree;

    static int frozen = 0;
    static int cold = 1;
    static int warm = 2;
    static int help = 3;

    public int state = warm;

    protected bool _isMoving = false;
    protected bool _hasReachedTarget=false;
    public GameObject _moveTarget;

    public int _numberLumbs = 0;
    bool hasObjective = false;
    PolyNavAgent agent;

    Animator animator;

    Vector3 _previousPosition;

    private void Awake()
    {
        agent = GetComponent<PolyNavAgent>();
        animator = GetComponent<Animator>();
        tentPosition.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }

    void Start () {
        SetPNJ();
        EventManager.StartListening(EventManager.PLAY_EVENT, Play);
    }

    void SetPNJ()
    {
        transform.position = new Vector3(tentPosition.position.x, tentPosition.position.y, tentPosition.position.z);
        _heat = INITIAL_HEAT;
        agent.maxSpeed = INITIAL_SPEED;
        HeatCheck();
        if (state == frozen) LevelManager.manager.nbVillagersAlive--;
        hasObjective = false;
    }

    public void Reset()
    {
        SetPNJ();
        Play();
    }


    void Play()
    {
        StopAllCoroutines();
        StartCoroutine(DecreaseCoroutine());
    }

    // Update is called once per frame
    void Update () {
        if (!GameManager.manager.isPlaying) return;

        if (_heat < LevelManager.manager._heatHelp && _heat > 0)
        {
            _moveTarget = tentPosition.gameObject;
            Move();
        }

        if (agent.remainingDistance <= 0) StopMoving();
        if (state == help) Work();
    }

    private void OnTriggerEnter2D(Collider2D pCol)
    {
        if (!GameManager.manager.isPlaying) return;

        if (pCol.gameObject.tag == LevelManager.LUMB_TAG)
        {
            if (_numberLumbs < LevelManager.manager.lumbCapacity)
            {
                TakeLumb(pCol.gameObject);
            }

        }

        if (pCol.gameObject.tag == LevelManager.CENTRAL_FIRE_TAG)
        {
            if (_numberLumbs > 0) UpdateFire();
        }

        if (pCol.gameObject.tag == LevelManager.TREE_TAG)
        {
            if (_numberLumbs <= 0)
            {
                lastTree = pCol.GetComponent<Tree>();
                if(!lastTree.isBeingChopped)
                {
                    lastTree.isBeingChopped = true;
                    CutTree(lastTree);
                    animator.SetBool("isChopping", true);
                }
            }
        }

        StopMoving();
        if(state==help)Work();
    }

    private void OnTriggerStay2D(Collider2D pCol)
    {
        if (pCol.gameObject.tag == LevelManager.TREE_TAG)
        {
            if (_numberLumbs <= 0)
            {
                lastTree = pCol.GetComponent<Tree>();
                if (!lastTree.isBeingChopped)
                {
                    lastTree.isBeingChopped = true;
                    animator.SetBool("isChopping", true);
                    CutTree(lastTree);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D pCol)
    {
        if (lastTree != null)
        {
            lastTree.isBeingChopped = false;
            lastTree = null;
            animator.SetBool("isChopping", false);
            hasObjective = false;
        }
    }

    void CutTree(Tree pTree)
    {
        AkSoundEngine.PostEvent("Play_PNJ_Wood", pTree.gameObject);
        pTree.Cut();
        animator.SetBool("isMoving", false);
    }

    void UpdateFire()
    {
        AkSoundEngine.PostEvent("Play_PNJ_RefillFire", gameObject);
        if (_numberLumbs > 0) LevelManager.manager.centralFire.UpdateFire(_numberLumbs);
        _numberLumbs = 0;
        agent.maxSpeed = INITIAL_SPEED;
        hasObjective = false;
    }

    void TakeLumb(GameObject pLumb)
    {
        AkSoundEngine.PostEvent("Play_PNJ_PickWood", gameObject);
        _numberLumbs++;
        agent.maxSpeed -= LevelManager.manager.SLOW_SPEED;
        if (agent.maxSpeed < LevelManager.manager._minSpeed)
        {
            agent.maxSpeed = LevelManager.manager._minSpeed;
        }

        Destroy(pLumb);
        
        if(state==help)Work();
    }

    IEnumerator DecreaseCoroutine()
    {
        while (GameManager.manager.isPlaying)
        {
            yield return new WaitForSecondsRealtime(LevelManager.manager._timeFreeze);
            DecreaseHeat();
        }
    }

    void DecreaseHeat()
    {
        _heat-=1+ LevelManager.manager.currentBlizzardColdModifier;
        HeatCheck();
    }

    void HeatCheck()
    {
        if (_heat >= LevelManager.manager._heatHelp)
        {
            if(state !=help)  Help();
        }
        else if (_heat >= LevelManager.manager._heatWarm)
        {
            if (state!=warm)
            {
                Warm();
            }
        }
        else if (_heat <0)
        {
            if(state!=frozen)
            {
                Freeze();
            }
        }
        else if (_heat< LevelManager.manager._heatWarm)
        {
            if (state!=cold)
            {
                animator.SetInteger("PNJWalkState", -1);
                if(state==frozen)LevelManager.manager.nbVillagersAlive++;
                Cold();
            }
        }
    }

    public void Cold()
    {
        state = cold;
        UpdateSprite();
    }

    public void Help()
    {
        state = help;
        UpdateSprite();
        Work();
    }

    public void Freeze()
    {
        state = frozen;
        LevelManager.manager.nbVillagersAlive--;
        animator.SetInteger("PNJWalkState", -2);
        AkSoundEngine.PostEvent("Play_Ice", gameObject);
    }

    public void Warm()
    {
        state = warm;
        UpdateSprite();
    }

    public void AddHeat(int heatModifier)
    {
        AkSoundEngine.PostEvent("Play_RefillBrasier", gameObject);
        _heat += heatModifier;
        if (_heat > LevelManager.manager._heatMax) _heat = LevelManager.manager._heatMax;
        HeatCheck();
    }

    float GetDistance(GameObject obj)
    {
        return Vector3.Distance(transform.position, obj.transform.position);
    }

    void Work()
    {
        if(GameManager.manager.isPlaying)
        {
            if (_numberLumbs < LevelManager.manager.lumbCapacity)
            {
                GameObject[] lumbs = GameObject.FindGameObjectsWithTag("Lumb");
                if (lumbs != null && lumbs.Length > 0 && !hasObjective)
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
                    Move();
                }
                else if (_numberLumbs > 0 && !hasObjective)
                {
                    _moveTarget = LevelManager.manager.centralFire.gameObject;
                    Move();
                }
                else
                {
                    GameObject[] trees = GameObject.FindGameObjectsWithTag("Tree");
                    if (trees != null && trees.Length > 0)
                    {
                        hasObjective = true;
                        float distanceMin = GetDistance(trees[0]);
                        int id = 0;
                        for (int i = 0; i < trees.Length; i++)
                        {
                            if (GetDistance(trees[i]) < distanceMin)
                            {
                                distanceMin = GetDistance(trees[i]);
                                id = i;
                            }
                        }
                        _moveTarget = trees[id];
                        Move();
                    }
                }
            }
            else
            {
                _moveTarget = LevelManager.manager.centralFire.gameObject;
                Move();
            }
        }
    }

    void Move()
    {
        _previousPosition = transform.position;

        if (_moveTarget != null)
        {
            agent.SetDestination(_moveTarget.transform.position);
            _isMoving = true;
            animator.SetBool("isMoving", true);
        }
            UpdateSprite();
    }

    void StopMoving()
    {
        _hasReachedTarget = true;
        _isMoving = false;
        _moveTarget = null;
        if(state>=warm)animator.SetInteger("PNJWalkState", 0);
    }

    void UpdateSprite()
    {
        if(_isMoving)
        {
            if (_moveTarget.transform.position.y > _previousPosition.y && (Mathf.Abs(_moveTarget.transform.position.y - _previousPosition.y) > Mathf.Abs(_moveTarget.transform.position.x - _previousPosition.x))) animator.SetInteger("PNJWalkState", 1);
            if (_moveTarget.transform.position.y < _previousPosition.y && (Mathf.Abs(_moveTarget.transform.position.y - _previousPosition.y) > Mathf.Abs(_moveTarget.transform.position.x - _previousPosition.x))) animator.SetInteger("PNJWalkState", 2);
            if (_moveTarget.transform.position.x > _previousPosition.x && (Mathf.Abs(_moveTarget.transform.position.y - _previousPosition.y) < Mathf.Abs(_moveTarget.transform.position.x - _previousPosition.x))) animator.SetInteger("PNJWalkState", 4);
            if (_moveTarget.transform.position.x < _previousPosition.x && (Mathf.Abs(_moveTarget.transform.position.y - _previousPosition.y) < Mathf.Abs(_moveTarget.transform.position.x - _previousPosition.x))) animator.SetInteger("PNJWalkState", 3);
        }
       else animator.SetInteger("PNJWalkState", 0);
        if (_numberLumbs>0) animator.SetBool("isHoldingWood", true);
        else animator.SetBool("isHoldingWood", false);
        
    }

    private void OnDisable()
    {
        EventManager.StopListening(EventManager.PLAY_EVENT, Play);
    }
}
