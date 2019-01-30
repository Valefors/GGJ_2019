using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WwisePostEvent : MonoBehaviour {

    public AK.Wwise.Event footsteps;
	// Use this for initialization
	public void PlayFootstepSound() {
        footsteps.Post(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
