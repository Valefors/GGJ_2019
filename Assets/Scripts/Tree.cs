using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour {

    [SerializeField] GameObject _lumb;
    [SerializeField] SpriteRenderer _sr;
    [SerializeField] Sprite _strumpSprite;
    [SerializeField] Sprite _treeSprite;

    [SerializeField] int _delayRepop = 2;

    public void Cut()
    {
        Vector3 lPos = new Vector3(transform.position.x + 3, transform.position.y, transform.position.z);

        Instantiate(_lumb, lPos, Quaternion.identity, transform.parent);
        _sr.sprite = _strumpSprite;
        tag = "Untagged";

        StartCoroutine(RepopCoroutine());
    }

    IEnumerator RepopCoroutine()
    {
        yield return new WaitForSeconds(_delayRepop);
        Repop();
    }

    void Repop()
    {
        _sr.sprite = _treeSprite;
        tag = LevelManager.TREE_TAG;
    }
}
