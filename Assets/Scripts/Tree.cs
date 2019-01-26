using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour {

    [SerializeField] GameObject _lumb;
    SpriteRenderer _sr;
    [SerializeField] Sprite _strumpSprite;

    private void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    public void Cut()
    {
        Instantiate(_lumb, transform.position, Quaternion.identity, transform.parent);
        _sr.sprite = _strumpSprite;
    }
}
