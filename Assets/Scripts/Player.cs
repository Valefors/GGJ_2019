using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    [SerializeField] Camera cam;
    [SerializeField] int _speed = 10;

    int _numberLumbs = 0;

    Vector3 _targetPosition;
    bool _isMoving;

    const int LEFT_MOUSE_BUTTON = 0;

    delegate void DelAction();
    DelAction playerAction;

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
            _numberLumbs++;
            Destroy(pCol.gameObject);
        }
    }
}
