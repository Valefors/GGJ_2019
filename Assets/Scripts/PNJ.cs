using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PNJ : MonoBehaviour {

    public int _heat = 30;
    [SerializeField] protected int INITIAL_HEAT = 30;
    #region Warmth
    [SerializeField] protected int _heatMax=100;
    [SerializeField] protected int _heatMin = -20;
    [SerializeField] protected float _timeFreeze = 10;

    [SerializeField] protected int _heatHelp = 60;
    [SerializeField] protected int _heatWarm = 20;
    #endregion

    [SerializeField] protected float _speed = 2;
    [SerializeField] float _minSpeed = 1;
    [SerializeField] float INITIAL_SPEED = 4;
    [SerializeField] float SLOW_SPEED = 1;
    [SerializeField] int lumbCapacity = 3;

    [SerializeField] Torche torch;
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

    protected int _numberLumbs = 0;
    NavMeshAgent agent;

    Animator animator;

    Vector3 _previousPosition;

    // Use this for initialization
    void Start () {
        _heat = INITIAL_HEAT;
        animator = GetComponent<Animator>();
        transform.position = tentPosition.position;
        agent = GetComponent<NavMeshAgent>();
        _speed = INITIAL_SPEED;

        if (state == frozen) LevelManager.manager.nbVillagersAlive--;
        HeatCheck();

        EventManager.StartListening(EventManager.PLAY_EVENT, Play);
    }

    void Play()
    {
        StartCoroutine(DecreaseCoroutine());
    }

    // Update is called once per frame
    void Update () {
        if (!GameManager.manager.isPlaying) return;

        if (_isMoving && _moveTarget != null)
        {
            MoveTo(_moveTarget);
        }

        if (_heat < _heatHelp && _heat > 0)
        {
            _moveTarget = tentPosition.gameObject;
            _isMoving = true;
        }

        if (state == help) Work();
	}

    private void OnTriggerEnter(Collider pCol)
    {
        if (!GameManager.manager.isPlaying) return;

        if (pCol.gameObject.tag == LevelManager.LUMB_TAG)
        {
            if (_numberLumbs < lumbCapacity)
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
            if (_numberLumbs <= 0) CutTree(pCol.GetComponent<Tree>());
        }

        StopMoving();
        if(state==help)Work();
    }

    private void OnTriggerStay(Collider pCol)
    {
        if (pCol.gameObject.tag == LevelManager.TREE_TAG)
        {

            lastTree = pCol.GetComponent<Tree>();
            lastTree.isBeingChopped = true;
        }
    }

    private void OnTriggerExit(Collider pCol)
    {
        if (lastTree != null)
        {
            lastTree.isBeingChopped = false;
            lastTree = null;
        }
    }

    void CutTree(Tree pTree)
    {
        AkSoundEngine.PostEvent("Play_PNJ_Wood", gameObject);
        pTree.Cut();
    }

    void UpdateFire()
    {
        AkSoundEngine.PostEvent("Play_PNJ_RefillFire", gameObject);
        if (_numberLumbs > 0) CentralFire.instance.UpdateFire(_numberLumbs);
        _numberLumbs = 0;
        _speed = INITIAL_SPEED;
    }

    void TakeLumb(GameObject pLumb)
    {
        AkSoundEngine.PostEvent("Play_PNJ_PickWood", gameObject);
        _numberLumbs++;
        _speed -= SLOW_SPEED;
        if (_speed < _minSpeed) _speed = _minSpeed;

        Destroy(pLumb);
        
        if(state==help)Work();
    }

    IEnumerator DecreaseCoroutine()
    {
        while (GameManager.manager.isPlaying)
        {
            yield return new WaitForSecondsRealtime(_timeFreeze);
            DecreaseHeat();
        }
    }

    public void Reset()
    {
        Start();
        Play();
    }

    void DecreaseHeat()
    {
        _heat--;
        HeatCheck();
    }

    void HeatCheck()
    {
        if (_heat >= _heatHelp)
        {
            if(state !=help)  Help();
        }
        else if (_heat >= _heatWarm)
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
        else if (_heat<_heatWarm)
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
        LevelManager.manager.nbVillagersAlive--;
        animator.SetInteger("PNJWalkState", -2);
        AkSoundEngine.PostEvent("Play_Ice", gameObject);
        // GERER CHANGEMEMENTS VISUELS
    }

    public void Warm()
    {
        state = warm;
        // GERER CHANGEMEMENTS VISUELS
    }

    public void AddHeat(int heatModifier)
    {
        AkSoundEngine.PostEvent("Play_RefillBrasier", gameObject);
        _heat += heatModifier;
        if (_heat > _heatMax) _heat = _heatMax;
        HeatCheck();
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
            if (lumbs != null && lumbs.Length >0)
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
            else if(_numberLumbs>0)
            {
                _moveTarget = CentralFire.instance.gameObject;
                _isMoving = true;
            }
            else
            {
                GameObject[] trees = GameObject.FindGameObjectsWithTag("Tree");
                if (trees != null && trees.Length > 0)
                {
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
                    _isMoving = true;
                }
            }
        }
        else
        {
            _moveTarget = CentralFire.instance.gameObject;
            _isMoving = true;
        }
    }

    void MoveTo(GameObject obj)
    {
        _previousPosition = transform.position;

        transform.position = Vector3.MoveTowards(transform.position, obj.transform.position, _speed*Time.deltaTime);
        UpdateSprite();

        if (transform.position == obj.transform.position)
        {
            StopMoving();
        }
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
        if (transform.position.y > _previousPosition.y && (Mathf.Abs(transform.position.y - _previousPosition.y) > Mathf.Abs(transform.position.x - _previousPosition.x))) animator.SetInteger("PNJWalkState", 1);
        if (transform.position.y < _previousPosition.y && (Mathf.Abs(transform.position.y - _previousPosition.y) > Mathf.Abs(transform.position.x - _previousPosition.x))) animator.SetInteger("PNJWalkState", 2);
        if (transform.position.x > _previousPosition.x && (Mathf.Abs(transform.position.y - _previousPosition.y) < Mathf.Abs(transform.position.x - _previousPosition.x))) animator.SetInteger("PNJWalkState", 4);
        if (transform.position.x < _previousPosition.x && (Mathf.Abs(transform.position.y - _previousPosition.y) < Mathf.Abs(transform.position.x - _previousPosition.x))) animator.SetInteger("PNJWalkState", 3);
    }

    private void OnDisable()
    {
        EventManager.StopListening(EventManager.PLAY_EVENT, Play);
    }
}
