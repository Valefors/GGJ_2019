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

    #region Blink
    float spriteBlinkingTimer = 0.0f;
    float spriteBlinkingMiniDuration = 0.1f;
    float spriteBlinkingTotalTimer = 0.0f;
    float spriteBlinkingTotalDuration = 1.0f;
    bool startBlinking = false;
    #endregion

    public void Cut()
    {
        print("cut");
        startBlinking = true;
    }

    IEnumerator CutTree()
    {
        yield return new WaitForSeconds(_delayRepop);
    }

    void SetModeLumb()
    {
        Vector3 lPos = new Vector3(transform.position.x + OFFSET_X, transform.position.y, transform.position.z);

        Instantiate(_lumb, lPos, Quaternion.identity, transform.parent);

        int lRandom = Random.Range(0, _strumpSprite.Length);
        _sr.sprite = _strumpSprite[lRandom];
        tag = "Untagged";

        StartCoroutine(RepopCoroutine());
    }

    private void SpriteBlinkingEffect()
    {
        spriteBlinkingTotalTimer += Time.deltaTime;
        if (spriteBlinkingTotalTimer >= spriteBlinkingTotalDuration)
        {
            startBlinking = false;
            spriteBlinkingTotalTimer = 0.0f;
            this.gameObject.GetComponent<SpriteRenderer>().enabled = true;

            SetModeLumb();

            return;
        }

        spriteBlinkingTimer += Time.deltaTime;
        if (spriteBlinkingTimer >= spriteBlinkingMiniDuration)
        {
            spriteBlinkingTimer = 0.0f;
            if (this.gameObject.GetComponent<SpriteRenderer>().enabled == true)
            {
                this.gameObject.GetComponent<SpriteRenderer>().enabled = false;
            }
            else
            {
                this.gameObject.GetComponent<SpriteRenderer>().enabled = true;
            }
        }
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

    void Update()
    {
        if (startBlinking == true)
        {
            SpriteBlinkingEffect();
        }
    }
}
