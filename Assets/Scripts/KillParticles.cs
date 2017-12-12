using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillParticles : MonoBehaviour {

	ParticleSystem ps;	
	public bool hasTimer;
	public float time;
	public SkillCastManager skillCastManager;

	[HideInInspector]
	public bool fromServer;

	void Start () {
		ps = this.GetComponent<ParticleSystem>();
	}
	
	void Update () {
		if(hasTimer){
			time -= 1 * Time.deltaTime;
			if(time <= 0){
				Debug.Log("PRE-DESTROY");
				onDestroy();
				Debug.Log("DESTROYED");
			}
		} else {
			if(ps != null){
				if (!ps.IsAlive ()) {
					onDestroy();
				}
			}
		}
	}

	void onDestroy(){
		if(!fromServer){
			if(skillCastManager != null){
				skillCastManager.targetEntity.targeted = false;
			}
		}
		Destroy(this.gameObject);
	}
}
