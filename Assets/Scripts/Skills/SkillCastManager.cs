using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


public class SkillCastManager : NetworkBehaviour {

	GameObject targetObject;
	string type;
	MethodInfo methodInfo;

	[HideInInspector]
	public MobManager targetEntity;

	public void cast(string type, GameObject targetObject){
		this.methodInfo = this.GetType().GetMethod(type);
	}

	public void target(){
		this.transform.position = targetObject.transform.position;
	}
	public void aoe(){
	}
	public void projectile(){

		float speed = 4f;
		this.transform.position += transform.forward * speed * Time.deltaTime;
	}

	/*public void cast(GameObject caster, Server server, PlayerServer playerServer, string type, GameObject target){
		this.caster = caster;
		this.server = server;
		this.playerServer = playerServer;
		this.targetObject = target;
		this.type = type;

		this.methodInfo = this.GetType().GetMethod(type);
		this.updateMethod = this.GetType().GetMethod("update_"+type);
        if(methodInfo != null) methodInfo.Invoke(this, null);
		if(updateMethod != null) updateMethod.Invoke(this, null);
	}*/
	/*
	public void target(){

		if(targetObject != null){
			ai = targetObject.GetComponent<AI>();
			if(ai != null){
				ai.damage(1, this.caster, this.playerServer);
                ai.setSkillCastManager(this);
			}
		}
		timer = new Timer(2, false);
	}*/

	/*void Update(){
		if(!isServer) return;
		if(updateMethod != null) this.updateMethod.Invoke(this, null);
		if(timer != null) {
			timer.update();
			if(timer.isFinished()) {
				timer = null;
				ai.targeted = false;
				NetworkServer.Destroy(this.gameObject);
			}
		}
	}*/

	/*private void OnTriggerEnter(Collider other)
	{
		if(!isServer) return;
		if(!type.Equals("projectile")) return;
		if (other.transform.CompareTag("Enemy")) {
			Debug.Log("It hit! " + this.server + " | collider");
			AI ai = other.GetComponent<AI>();
			ai.damage(5, this.caster, this.playerServer);
			NetworkServer.Destroy(this.gameObject);
		}   
	}
	private void OnCollisionEnter(Collision other)
	{
		if(!isServer) return;
		if(!type.Equals("projectile")) return;
		if (other.transform.CompareTag("Enemy")) {
			AI ai = other.gameObject.GetComponent<AI>();
			ai.damage(5, this.caster, this.playerServer);
			Debug.Log("It hit! " + this.server + " | " + " collision");
			NetworkServer.Destroy(this.gameObject);
		}
	}*/
}
