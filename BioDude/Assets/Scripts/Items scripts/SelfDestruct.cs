using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour {

    public float TimeUntilSelfDestruct;
	
	// Update is called once per frame
	void Update () {
        TimeUntilSelfDestruct -= Time.deltaTime;
        if (TimeUntilSelfDestruct <= 0)
            Destroy(gameObject);
	}
}
