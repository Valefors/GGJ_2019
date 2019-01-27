using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torche : MonoBehaviour {

    [SerializeField] PNJ PNJ;
    [SerializeField] Animator _animator;

    [SerializeField] int _decreasePerSecond = 2;
    [SerializeField] int _delayBetweenDecrease = 3;

    static int frozen = 0;
    static int cold = 1;
    static int warm = 2;
    static int help = 3;

    int _animFireState = 0;

    public void AddHeat(int value)
    {
        PNJ.AddHeat(value);
        _animFireState = 1;
        _animator.SetInteger("FireState", _animFireState);
    }

	private void Update()
    {
        /*float newSize = (PNJ._heat/50f);
        transform.localScale = new Vector3(newSize,newSize,0);*/
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
