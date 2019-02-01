using PolyNav;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour {

    [SerializeField] public Camera cam;
    [SerializeField] float _speed = 0;

    [SerializeField] float INITIAL_SPEED = 10f;
    [SerializeField] float SLOW_SPEED = 2f;
    [SerializeField] float MIN_SPEED = 0.5f;
    [SerializeField] int _lumbCapacity = 3;

    public int _numberLumbs = 0;

    Vector3 _targetPosition;
    public bool _isMoving;
    bool _hasFire = false;

    Vector3 _previousPosition;

    const int LEFT_MOUSE_BUTTON = 0;

    Tree lastTree;

    delegate void DelAction();
    DelAction playerAction;

    [SerializeField] Animator animator;

    PolyNavAgent agent;

    #region Singleton
    private static Player _instance;
    public static Player instance {
        get {
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null) _instance = this;

        else if (_instance != this) Destroy(gameObject);
        agent = GetComponent<PolyNavAgent>();
    }
    #endregion

    // Use this for initialization
    void Start () {
        SetPlayer();
	}
	
	// Update is called once per frame
	void Update () {
        if (!GameManager.manager.isPlaying) return;

        if (playerAction != null) playerAction();
    }

    void SetPlayer()
    {
        _hasFire = false;
        StopMoving();
        _targetPosition = transform.position;
        transform.position = transform.parent.position;
        _speed = INITIAL_SPEED;
        agent.maxSpeed = INITIAL_SPEED;
        animator.SetBool("isHoldingFire", false);
        SetActionMove();
    }

    public void Reset()
    {
        SetPlayer();
    }

    void SetActionMove()
    {
        playerAction = DoActionMove;
    }

    void DoActionMove()
    {
        if(GameManager.manager.isPlaying)
        {
            if (Input.GetMouseButtonDown(LEFT_MOUSE_BUTTON))
            {
                MovePlayer();
            }
            if (_isMoving)
            {
                if (agent.remainingDistance <= 0) StopMoving();
            }
        }
    }

    void SetTargetPosition()
    {
        _targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _isMoving = true;
    }

    void MovePlayer()
    {
        _previousPosition = transform.position;
        SetTargetPosition();
        agent.SetDestination(_targetPosition);
        UpdateSprite();

        Debug.DrawLine(transform.position, _targetPosition, Color.red);
    }

    void UpdateSprite()
    {
        if(_isMoving)
        {
            if (_targetPosition.y > _previousPosition.y && (Mathf.Abs(_targetPosition.y - _previousPosition.y) > Mathf.Abs(_targetPosition.x - _previousPosition.x))) animator.SetInteger("PNJWalkState", 1);
            if (_targetPosition.y < _previousPosition.y && (Mathf.Abs(_targetPosition.y - _previousPosition.y) > Mathf.Abs(_targetPosition.x - _previousPosition.x))) animator.SetInteger("PNJWalkState", 2);
            if (_targetPosition.x > _previousPosition.x && (Mathf.Abs(_targetPosition.y - _previousPosition.y) < Mathf.Abs(_targetPosition.x - _previousPosition.x))) animator.SetInteger("PNJWalkState", 4);
            if (_targetPosition.x < _previousPosition.x && (Mathf.Abs(_targetPosition.y - _previousPosition.y) < Mathf.Abs(_targetPosition.x - _previousPosition.x))) animator.SetInteger("PNJWalkState", 3);
        }
        else animator.SetInteger("PNJWalkState", 0);
        
        if (_numberLumbs > 0) animator.SetBool("isHoldingWood", true);
        else animator.SetBool("isHoldingWood", false);
    }

    private void OnTriggerEnter2D(Collider2D pCol)
    {
        if (!GameManager.manager.isPlaying) return;

        if (pCol.gameObject.tag == LevelManager.LUMB_TAG)
        {
            if(_numberLumbs < _lumbCapacity && !_hasFire) TakeLumb(pCol.gameObject);
        }

        if (pCol.gameObject.tag == LevelManager.TREE_TAG)
        {
            //_isMoving = false;
            if (_numberLumbs <= 0 && !_hasFire)
            {
                lastTree = pCol.GetComponent<Tree>();
                if (!lastTree.isBeingChopped)
                {
                    lastTree.isBeingChopped = true;
                    CutTree(lastTree);
                }
            }
        }

        if (pCol.gameObject.tag == LevelManager.CENTRAL_FIRE_TAG)
        {
            if (_numberLumbs > 0) UpdateFire();
            else
            {
                if(!_hasFire) TakeFire();
            }
        }

        if (pCol.gameObject.tag == LevelManager.TORCHE_TAG)
        {
            if (_hasFire) UpdateTorche(pCol.gameObject.GetComponent<Torche>());
        }

        if (pCol.gameObject.tag == LevelManager.DOGGO_TAG)
        {
            animator.SetBool("Patpat_Bool", true);
        }
    }

    private void OnTriggerStay2D(Collider2D pCol)
    {
        if (pCol.gameObject.tag == LevelManager.TREE_TAG)
        {
            if (_numberLumbs <= 0 && !_hasFire)
            {
                lastTree = pCol.GetComponent<Tree>();
                if (!lastTree.isBeingChopped)
                {
                    lastTree.isBeingChopped = true;
                    CutTree(lastTree);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D pCol)
    {
        animator.SetBool("Patpat_Bool",false);

        if (lastTree != null)
        {
            lastTree.isBeingChopped = false;
            lastTree = null;
        }
    }

    void StopMoving()
    {
        print("stop");
        _isMoving = false;
        animator.SetInteger("PNJWalkState", 0);
    }

    void CutTree(Tree pTree)
    {
        AkSoundEngine.PostEvent("Play_Wood", pTree.gameObject);
        pTree.Cut();
    }

    void TakeFire()
    {
        if(CentralFire.instance.UpdateFire(0, false))
        {
            AkSoundEngine.PostEvent("Play_PickTorch", gameObject);
            _hasFire = true;
            animator.SetBool("isHoldingFire", true);
        }
    }

    void UpdateTorche(Torche pTorche)
    {
        pTorche.AddHeat(CentralFire.instance._valueFireTaken);
        _hasFire = false;
        animator.SetBool("isHoldingFire", false);
    }

    void TakeLumb(GameObject pLumb)
    {
        AkSoundEngine.PostEvent("Play_PickWood", gameObject);
        _numberLumbs++;
        _speed -= SLOW_SPEED;
        agent.maxSpeed -= SLOW_SPEED;
        if (_speed <= MIN_SPEED) _speed = MIN_SPEED;

        Destroy(pLumb);
    }

    void UpdateFire()
    {
        AkSoundEngine.PostEvent("Play_RefillFire", gameObject);
        if (_numberLumbs > 0) CentralFire.instance.UpdateFire(_numberLumbs);
        _numberLumbs = 0;
        _speed = INITIAL_SPEED;
        agent.maxSpeed = INITIAL_SPEED;

    }
}
