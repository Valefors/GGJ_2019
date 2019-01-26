using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {

    public static string LUMB_TAG = "lumb";
    public static string WOOD_TAG = "wood";

    private static LevelManager _manager;
    public static LevelManager manager {
        get {
            return _manager;
        }
    }

    private void Awake()
    {
        if (_manager == null) _manager = this;

        else if (_manager != this) Destroy(gameObject);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
