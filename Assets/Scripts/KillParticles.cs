using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillParticles : MonoBehaviour {

	ParticleSystem ps;	
	public bool hasTimer;
	public float time;

	void Start () {
		ps = this.GetComponent<ParticleSystem>();
	}
	
	void Update () {
		if(hasTimer){
			time -= 1 * Time.deltaTime;
			if(time <= 0){
				Destroy(this.gameObject);
			}
		} else {
			if(ps != null){
				if (!ps.IsAlive ()) {
					Destroy(this.gameObject);
				}
			}
		}
	}
}
