using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    [SerializeField] Camera cam;
    [SerializeField] int _speed = 10;

    [SerializeField] int INITIAL_SPEED = 10;
    [SerializeField] int SLOW_SPEED = 2;

    int _numberLumbs = 0;

    Vector3 _targetPosition;
    bool _isMoving;
    bool _hasFire = false;

    const int LEFT_MOUSE_BUTTON = 0;

    delegate void DelAction();
    DelAction playerAction;

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
    }
    #endregion

    // Use this for initialization
    void Start () {
        _targetPosition = transform.position;
        _isMoving = false;

        SetActionMove();
	}
	
	// Update is called once per frame
	void Update () {
        if (playerAction != null) playerAction();  
	}

    void SetActionMove()
    {
        playerAction = DoActionMove;
    }

    void DoActionMove()
    {
        if (Input.GetMouseButton(LEFT_MOUSE_BUTTON)) SetTargetPosition();
        if (_isMoving) MovePlayer();
    }

    void SetTargetPosition()
    {
        Plane lPlane = new Plane(Vector3.forward, transform.position);
        Ray lRay = cam.ScreenPointToRay(Input.mousePosition);
        float lPoint = 0f;

        if (lPlane.Raycast(lRay, out lPoint)) _targetPosition = lRay.GetPoint(lPoint);

        _isMoving = true;
    }

    void MovePlayer()
    {
        //transform.LookAt(_targetPosition);
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _speed * Time.deltaTime);

        if (transform.position == _targetPosition) _isMoving = false;
        Debug.DrawLine(transform.position, _targetPosition, Color.red);
    }

    private void OnTriggerEnter(Collider pCol)
    {
        if(pCol.gameObject.tag == LevelManager.LUMB_TAG)
        {
            if(_numberLumbs < 3 && !_hasFire) TakeLumb(pCol.gameObject);
        }

        if (pCol.gameObject.tag == LevelManager.TREE_TAG)
        {
            _isMoving = false;
            if (_numberLumbs <= 0 && !_hasFire) CutTree(pCol.GetComponent<Tree>());
        }

        if (pCol.gameObject.tag == LevelManager.CENTRAL_FIRE_TAG)
        {
            if (_numberLumbs > 0) UpdateFire();
            else TakeFire();
        }

        if (pCol.gameObject.tag == LevelManager.TORCHE_TAG)
        {
            if (_hasFire) UpdateTorche(pCol.gameObject.GetComponent<Torche>());
        }
    }

    void CutTree(Tree pTree)
    {
        pTree.Cut();
    }

    void TakeFire()
    {
        _hasFire = true;
        CentralFire.instance.UpdateFire(0, false);
    }

    void UpdateTorche(Torche pTorche)
    {
        pTorche.AddHeat(LevelManager.manager.heatingModifier);
        _hasFire = false;
    }

    void TakeLumb(GameObject pLumb)
    {
        _numberLumbs++;
        _speed -= SLOW_SPEED;

        Destroy(pLumb);
    }

    void UpdateFire()
    {
        if(_numberLumbs > 0) CentralFire.instance.UpdateFire(_numberLumbs);
        _numberLumbs = 0;
        _speed = INITIAL_SPEED;

    }
}
