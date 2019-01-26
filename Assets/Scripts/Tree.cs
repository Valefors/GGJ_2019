using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour {

    [SerializeField] GameObject _lumb;

    public void Cut()
    {
        Instantiate(_lumb, transform.position, Quaternion.identity, transform.parent);
        Destroy(gameObject);
    }
}
