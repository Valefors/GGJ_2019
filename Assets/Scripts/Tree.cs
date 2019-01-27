using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour {

    [SerializeField] GameObject _lumb;
    [SerializeField] SpriteRenderer _sr;
    [SerializeField] Sprite[] _strumpSprite;
    [SerializeField] Sprite[] _treeSprite;

    [SerializeField] int _delayRepop = 2;
    [SerializeField] Animator _animator;

    [SerializeField] float OFFSET_X = 3f;

    public void Cut()
    {
        Vector3 lPos = new Vector3(transform.position.x + OFFSET_X, transform.position.y, transform.position.z);

        Instantiate(_lumb, lPos, Quaternion.identity, transform.parent);

        int lRandom = Random.Range(0, _strumpSprite.Length);
        _sr.sprite = _strumpSprite[lRandom];
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
        int lRandom = Random.Range(0, _treeSprite.Length);
        _sr.sprite = _treeSprite[lRandom];
        _animator.SetTrigger("PopPineTree_Trigger");
        tag = LevelManager.TREE_TAG;
    }
}
