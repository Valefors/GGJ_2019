using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {

    public static string LUMB_TAG = "Lumb";
    public static string CENTRAL_FIRE_TAG = "CentralFire";
    public static string TREE_TAG = "Tree";
    public static string TORCHE_TAG = "Torche";

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
}
