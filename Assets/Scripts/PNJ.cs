using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PNJ : MonoBehaviour {

    public int _heat = 30;
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

    static int frozen = 0;
    static int cold = 1;
    static int warm = 2;
    static int help = 3;

    public int state = warm;

    protected float _timeSpent=0;
    protected bool _isMoving = false;
    protected bool _hasReachedTarget=false;
    protected GameObject _moveTarget;

    protected int _numberLumbs = 0;
    NavMeshAgent agent;

    Vector3 _previousPosition;

    // Use this for initialization
    void Start () {
        agent = GetComponent<NavMeshAgent>();
        _speed = INITIAL_SPEED;
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

        if (state == help) Work();
	}

    private void OnTriggerEnter(Collider pCol)
    {
        Debug.Log("entre collision");
        if (pCol.gameObject.tag == LevelManager.LUMB_TAG)
        {
            if (_numberLumbs <= lumbCapacity)
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
        
        Work();
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
            if (state == help)
            {
                _moveTarget = tentPosition.gameObject;
                _isMoving = true;
            }
            Warm();
        }
        else if (_heat <0)
        {
            AkSoundEngine.PostEvent("Play_BrasierOut", gameObject);
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
        //AkSoundEngine.PostEvent("Play_Ice", gameObject);
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
        //agent.Warp(transform.position);
        //agent.SetDestination(obj.transform.position);

        //agent.Warp(obj.transform.position);
        _previousPosition = transform.position;

        transform.position = Vector3.MoveTowards(transform.position, obj.transform.position, _speed*Time.deltaTime);
        UpdateSprite();

        if (transform.position == obj.transform.position)
        {
            _hasReachedTarget = true;
            _isMoving = false;
            _moveTarget = null;
        }
    }

    void UpdateSprite()
    {
        if (transform.position.y > _previousPosition.y && (Mathf.Abs(transform.position.y - _previousPosition.y) > Mathf.Abs(transform.position.x - _previousPosition.x))) print("dos");
        if (transform.position.y < _previousPosition.y && (Mathf.Abs(transform.position.y - _previousPosition.y) > Mathf.Abs(transform.position.x - _previousPosition.x))) print("face");
        if (transform.position.x > _previousPosition.x && (Mathf.Abs(transform.position.y - _previousPosition.y) < Mathf.Abs(transform.position.x - _previousPosition.x))) print("droite");
        if (transform.position.x < _previousPosition.x && (Mathf.Abs(transform.position.y - _previousPosition.y) < Mathf.Abs(transform.position.x - _previousPosition.x))) print("gauche");
    }
}
