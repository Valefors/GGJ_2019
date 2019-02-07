using PolyNav;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour {

    [SerializeField] public Camera cam;

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
    Torche lastTorch;

    delegate void DelAction();
    DelAction playerAction;

    [SerializeField] public Animator animator;

    PolyNavAgent agent;

    public string targetName;

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
        _numberLumbs = 0;
        _hasFire = false;
        StopMoving();
        _targetPosition = transform.position;
        transform.position = transform.parent.position;
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
                if (agent.remainingDistance <= 0)
                {
                    StopMoving();
                    if (targetName == "Doggo") targetName = null;
                }
            }
        }
    }

    void SetTargetPosition()
    {
        _targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (targetName=="Doggo") _targetPosition = GameObject.FindGameObjectWithTag(LevelManager.DOGGO_TAG).transform.position;
        _isMoving = true;
    }

    void MovePlayer()
    {
        animator.SetBool("isMoving", true);
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

    private void OnTriggerStay2D(Collider2D pCol)
    {
        if (!GameManager.manager.isPlaying) return;
        if (pCol.gameObject.tag == LevelManager.LUMB_TAG)
        {
            if (_numberLumbs < _lumbCapacity && !_hasFire) TakeLumb(pCol.gameObject);
        }
        if (targetName == null) return;
        string cName = pCol.gameObject.name;

        if (pCol.gameObject.tag == LevelManager.TREE_TAG)
        {
            if (_numberLumbs <= 0 && !_hasFire && targetName.Equals(cName))
            {
                lastTree = pCol.GetComponent<Tree>();
                if (!lastTree.isBeingChopped)
                {
                    CutTree();
                }
            }
        }

        if (pCol.gameObject.tag == LevelManager.CENTRAL_FIRE_TAG)
        {
            if (_numberLumbs > 0) UpdateFire();
            else
            {
                if (!_hasFire && targetName.Equals(cName)) TakeFire();
            }
        }

        if (pCol.gameObject.tag == LevelManager.TORCHE_TAG)
        {
            if (_hasFire && targetName.Equals(cName))
            {
                lastTorch =pCol.gameObject.GetComponent<Torche>();
                UpdateTorche();
            }
        }

        if (pCol.gameObject.tag == LevelManager.DOGGO_TAG && targetName.Equals("Doggo"))
        {
            PetDoggo();
        }
    }

    public void PetDoggo()
    {
        animator.SetBool("Patpat_Bool", true);
        targetName = null;
    }

    private void OnTriggerExit2D(Collider2D pCol)
    {
        if(pCol.tag==LevelManager.DOGGO_TAG)
        {
            animator.SetBool("Patpat_Bool", false);
        }
       else
        {      
            lastTorch = null;
            if (lastTree != null)
            {
                lastTree.isBeingChopped = false;
                lastTree = null;
                animator.SetBool("isChopping", false);
                StopMoving();
            }
        }
    }

    public void StopMoving()
    {
        agent.Stop();
        _isMoving = false;
        animator.SetInteger("PNJWalkState", 0);
    }

    public void CutTree()
    {
        if (lastTree == null) return;
        agent.Stop();
        _isMoving = false;
        animator.SetBool("isMoving", false);
        lastTree.isBeingChopped = true;
        AkSoundEngine.PostEvent("Play_Wood", lastTree.gameObject);
        lastTree.Cut();
        animator.SetBool("isChopping", true);
        targetName = null;
    }

    public void TakeFire()
    {
        if(LevelManager.manager.centralFire.UpdateFire(0, false))
        {
            StopMoving();
            AkSoundEngine.PostEvent("Play_PickTorch", gameObject);
            _hasFire = true;
            animator.SetBool("isHoldingFire", true);
            targetName = null;
        }
    }

    public void UpdateTorche()
    {
        if (lastTorch == null) return;
        StopMoving();
        lastTorch.AddHeat(LevelManager.manager.centralFire._valueFireTaken);
        _hasFire = false;
        animator.SetBool("isHoldingFire", false);
        targetName = null;
    }

    void TakeLumb(GameObject pLumb)
    {
        AkSoundEngine.PostEvent("Play_PickWood", gameObject);
        _numberLumbs++;
        agent.maxSpeed -= SLOW_SPEED;
        if (agent.maxSpeed <= MIN_SPEED) agent.maxSpeed = MIN_SPEED;
        UpdateSprite();
        Destroy(pLumb);
    }

    void UpdateFire()
    {
        AkSoundEngine.PostEvent("Play_RefillFire", gameObject);
        if (_numberLumbs > 0) LevelManager.manager.centralFire.UpdateFire(_numberLumbs);
        _numberLumbs = 0;
        agent.maxSpeed = INITIAL_SPEED;
        UpdateSprite();
        targetName = null;
        StopMoving();
    }
}
