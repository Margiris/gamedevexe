using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravHole : MonoBehaviour {
    private float duration;
    public ParticleSystem emitter;
    private PointEffector2D gravityEffector;
    private WindZone windZone;
    private float timeSpent = 0;
	// Use this for initialization
	void Start () {
        gravityEffector = transform.GetComponentInChildren<PointEffector2D>();
        duration = transform.GetComponentInParent<ParticleSystem>().main.duration;
        windZone = transform.GetComponentInChildren<WindZone>();
    }

    // Update is called once per frame
    void FixedUpdate () {
        timeSpent += Time.fixedDeltaTime;
        if(timeSpent > duration - 2 && timeSpent < duration)
        {
            gravityEffector.forceMagnitude += 150 * Time.fixedDeltaTime;
        }
        if(timeSpent >= duration -1 && timeSpent <= duration && windZone.radius > 5)
        {
            windZone.radius = 0.5f;
            emitter.transform.parent = null;
        }
	}
}
