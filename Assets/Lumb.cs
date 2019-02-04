using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lumb : MonoBehaviour {

    [SerializeField] SpriteRenderer _sr;
    [SerializeField] Sprite[] _lumbSprite;
    [SerializeField] Sprite highlightSprite;

    int currentSprite;

    // Use this for initialization
    void Start () {
        currentSprite = Random.Range(0, _lumbSprite.Length);
        _sr.sprite = _lumbSprite[currentSprite];
    }

    void OnMouseOver()
    {
        if (!GameManager.manager.isPlaying) return;
        Cursor.SetCursor(LevelManager.manager.hooverCursor, Vector2.zero, CursorMode.Auto);
        _sr.sprite = highlightSprite;
    }

    void OnMouseExit()
    {
        if (!GameManager.manager.isPlaying) return;
        Cursor.SetCursor(LevelManager.manager.normalCursor, Vector2.zero, CursorMode.Auto);
        _sr.sprite = _lumbSprite[currentSprite];
    }
}
