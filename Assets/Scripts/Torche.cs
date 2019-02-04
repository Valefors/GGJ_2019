using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Torche : MonoBehaviour {

    [SerializeField] PNJ PNJ;
    [SerializeField] Animator _animator;

    int _animFireState = 0;

    [SerializeField] Slider _slider;
    [SerializeField] float _offset_y = 5;
    [SerializeField] Sprite baseSprite;
    [SerializeField] Sprite highlightSprite;
    [SerializeField] SpriteRenderer _sr;

    private void Start()
    {
        Vector3 lSliderPos = new Vector3(transform.position.x, transform.position.y + _offset_y, transform.position.z);
        Vector3 lPos = Camera.main.WorldToScreenPoint(lSliderPos);

        _slider.transform.position = lPos;
        _slider.gameObject.SetActive(false);
    }

    void OnMouseOver()
    {
        if (!GameManager.manager.isPlaying) return;
        _slider.gameObject.SetActive(true);
        Cursor.SetCursor(LevelManager.manager.hooverCursor, Vector2.zero, CursorMode.Auto);
        _sr.sprite = highlightSprite;
    }

    void OnMouseExit()
    {
        if (!GameManager.manager.isPlaying) return;
        _slider.gameObject.SetActive(false);
        Cursor.SetCursor(LevelManager.manager.normalCursor, Vector2.zero, CursorMode.Auto);
        _sr.sprite = baseSprite;
    }

    public void AddHeat(int value)
    {
        PNJ.AddHeat(value);
        _animFireState = 1;
        _animator.SetInteger("FireState", _animFireState);
    }

	private void Update()
    {
        if (!GameManager.manager.isPlaying) return;

        _slider.value = PNJ._heat;

        if (0 <= PNJ._heat && PNJ._heat <= LevelManager.manager._heatWarm)
        {
            UpgradeFire0();
        }
        else if (LevelManager.manager._heatWarm < PNJ._heat && PNJ._heat <= LevelManager.manager._heatHelp)
        {
            UpgradeFire1();
        }
        else
        {
            UpgradeFire2();
        }
    }

    void UpgradeFire0()
    {
        _animFireState = 0;
        _animator.SetInteger("FireState", _animFireState);
    }

    void UpgradeFire1()
    {
        _animFireState = 1;
        _animator.SetInteger("FireState", _animFireState);
    }

    void UpgradeFire2()
    {
        _animFireState = 2;
        _animator.SetInteger("FireState", _animFireState);
    }
}
