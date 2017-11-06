using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillParticles : MonoBehaviour {

	ParticleSystem ps;	
	public bool test;

	void Start () {
		ps = this.GetComponent<ParticleSystem>();
	}
	
	void Update () {
		test = ps.IsAlive();
		if (!ps.IsAlive ()) {
			Destroy(this.gameObject);
		}
	}
}
