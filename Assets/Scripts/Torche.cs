using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torche : MonoBehaviour {

    [SerializeField] PNJ PNJ;

    static int frozen = 0;
    static int cold = 1;
    static int warm = 2;
    static int help = 3;

    public void AddHeat(int value)
    {
        PNJ.AddHeat(value);
    }

	private void Update()
    {
        float newSize = (PNJ._heat/50f);
        transform.localScale = new Vector3(newSize,newSize,0);
    }
}
