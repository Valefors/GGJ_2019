using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doggo : MonoBehaviour {

    private void OnMouseDown()
    {
        if (!GameManager.manager.isPlaying) return;
        LevelManager.manager.player.targetName = gameObject.name;
    }
}
