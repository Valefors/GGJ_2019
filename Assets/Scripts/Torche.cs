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

    private void Start()
    {
        Vector3 lSliderPos = new Vector3(transform.position.x, transform.position.y + _offset_y, transform.position.z);
        Vector3 lPos = Camera.main.WorldToScreenPoint(lSliderPos);

        _slider.transform.position = lPos;
        _slider.gameObject.SetActive(false);
    }

    void OnMouseOver()
    {
        _slider.gameObject.SetActive(true);
    }

    void OnMouseExit()
    {
        _slider.gameObject.SetActive(false);
    }

    public void AddHeat(int value)
    {
        PNJ.AddHeat(value);
        _animFireState = 1;
        _animator.SetInteger("FireState", _animFireState);
    }

	private void Update()
    {
        _slider.value = PNJ._heat;

        if (0 <= PNJ._heat && PNJ._heat <= 30)
        {
            UpgradeFire0();
        }
        else if (30 < PNJ._heat && PNJ._heat <= 70)
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
