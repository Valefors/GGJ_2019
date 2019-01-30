using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour {

    [SerializeField] Camera cam;
    [SerializeField] float _speed = 0;

    [SerializeField] float INITIAL_SPEED = 10f;
    [SerializeField] float SLOW_SPEED = 2f;
    [SerializeField] float MIN_SPEED = 0.5f;
    [SerializeField] int _lumbCapacity = 3;

    int _numberLumbs = 0;

    Vector3 _targetPosition;
    bool _isMoving;
    bool _hasFire = false;

    Vector3 _previousPosition;

    const int LEFT_MOUSE_BUTTON = 0;

    Tree lastTree;

    delegate void DelAction();
    DelAction playerAction;

    [SerializeField] Animator animator;

    NavMeshAgent agent;

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

        agent = GetComponent<NavMeshAgent>();
    }
    #endregion

    // Use this for initialization
    void Start () {
        _hasFire = false;
        _isMoving = false;
        _targetPosition = transform.position;
        transform.position = transform.parent.position;
        StopMoving();

        _speed = INITIAL_SPEED;

        animator.SetBool("isHoldingFire", false);
        SetActionMove();
	}
	
	// Update is called once per frame
	void Update () {
        if (!GameManager.manager.isPlaying) return;

        if (playerAction != null) playerAction();  
	}

    public void Reset()
    {
        Start();
    }

    void SetActionMove()
    {
        playerAction = DoActionMove;
    }

    void DoActionMove()
    {
        if(GameManager.manager.isPlaying)
        {
            if (Input.GetMouseButton(LEFT_MOUSE_BUTTON)) SetTargetPosition();
            if (_isMoving) MovePlayer();
        }
    }

    void SetTargetPosition()
    {
        Plane lPlane = new Plane(Vector3.forward, transform.position);
        Ray lRay = cam.ScreenPointToRay(Input.mousePosition);
        float lPoint = 0f;

        if (lPlane.Raycast(lRay, out lPoint))
        {
            _targetPosition = lRay.GetPoint(lPoint);
        }

        _isMoving = true;
    }

    void MovePlayer()
    {
        _previousPosition = transform.position;
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _speed * Time.deltaTime);

        UpdateSprite();

        if (transform.position == _targetPosition) StopMoving();
        Debug.DrawLine(transform.position, _targetPosition, Color.red);
    }

    void UpdateSprite()
    {
        if (_numberLumbs > 0) animator.SetBool("isHoldingWood", true);
        else animator.SetBool("isHoldingWood", false);
        if (transform.position.y > _previousPosition.y && (Mathf.Abs(transform.position.y - _previousPosition.y) > Mathf.Abs(transform.position.x - _previousPosition.x))) animator.SetInteger("PNJWalkState", 1);
        if (transform.position.y < _previousPosition.y && (Mathf.Abs(transform.position.y - _previousPosition.y) > Mathf.Abs(transform.position.x - _previousPosition.x))) animator.SetInteger("PNJWalkState", 2);
        if (transform.position.x > _previousPosition.x && (Mathf.Abs(transform.position.y - _previousPosition.y) < Mathf.Abs(transform.position.x - _previousPosition.x))) animator.SetInteger("PNJWalkState", 4);
        if (transform.position.x < _previousPosition.x && (Mathf.Abs(transform.position.y - _previousPosition.y) < Mathf.Abs(transform.position.x - _previousPosition.x))) animator.SetInteger("PNJWalkState", 3);
    }

    private void OnTriggerEnter2D(Collider2D pCol)
    {
        if (!GameManager.manager.isPlaying) return;

        print("collision");

        if (pCol.gameObject.tag == LevelManager.LUMB_TAG)
        {
            if(_numberLumbs < _lumbCapacity && !_hasFire) TakeLumb(pCol.gameObject);
        }

        if (pCol.gameObject.tag == LevelManager.TREE_TAG)
        {
            _isMoving = false;
            if (_numberLumbs <= 0 && !_hasFire)
            {
                lastTree = pCol.GetComponent<Tree>();
                lastTree.isBeingChopped = true;
                CutTree(lastTree);
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
            print("doggo");
            animator.SetBool("Patpat_Bool", true);
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
        _isMoving = false;
        if (!animator.GetBool("isHoldingFire")) animator.SetInteger("PNJWalkState", 0);
    }

    void CutTree(Tree pTree)
    {
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
        if (_speed <= MIN_SPEED) _speed = MIN_SPEED;

        Destroy(pLumb);
    }

    void UpdateFire()
    {
        AkSoundEngine.PostEvent("Play_RefillFire", gameObject);
        if (_numberLumbs > 0) CentralFire.instance.UpdateFire(_numberLumbs);
        _numberLumbs = 0;
        _speed = INITIAL_SPEED;

    }
}
