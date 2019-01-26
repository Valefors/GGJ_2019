using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torche : MonoBehaviour {

    [SerializeField] GameObject PNJ;
    [SerializeField] int LEVEL_STATE = 1;
    int _levelTorche = 0;

	public void UpdateTorche()
    {
        _levelTorche++;
        if(_levelTorche >= LEVEL_STATE)
        {
            transform.localScale += new Vector3(0.2f, 0.2f, 0);
            //AXEL ACTIVE PNJ
        }
    }
}
