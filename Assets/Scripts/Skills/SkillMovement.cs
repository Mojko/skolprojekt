using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalTools;
using UnityEngine.Networking;

public class SkillMovement : NetworkBehaviour {

    public float speed = 4f;
	private GameObject target;
    ToolsGlobal tools = new ToolsGlobal();
    public GameObject activationEffect;
    Player player;
	PlayerServer playerServer;
	Server server;
	GameObject caster;

	public void cast(GameObject caster, Server server, PlayerServer playerServer){
		this.caster = caster;
		this.server = server;
		this.playerServer = playerServer;
	}

    private void Update () 
    {
        
		this.transform.position += transform.forward * speed * Time.deltaTime;

		Collider[] colliders = Physics.OverlapSphere(this.transform.position, 2);


		if(target == null){
			for(int i=0;i<colliders.Length;i++){
				if(colliders[i].CompareTag("Enemy")){

					target = colliders[i].gameObject;
					float yDeltaRotation = Mathf.Abs((tools.rotateTowards(this.transform, target.transform.position).eulerAngles.y - this.transform.rotation.eulerAngles.y));
				}
			}
		}
        if (target != null) {
            //this.transform.rotation = tools.rotateTowards(this.transform.position, target.transform.position);
			//this.transform.rotation = tools.rotateTowards(this.transform.transform, target.transform.position, 0.1f); 
            /*if(Vector3.Distance(this.transform.position, target.transform.position) <= 1.1f) {
                AI ai = target.GetComponent<AI>();
                if(ai != null) {
                    ai.damage(5, null);
                }
                Destroy(this.gameObject);
            }*/
		}
	}

    private void OnTriggerEnter(Collider other)
    {
		if(!isServer) return;
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
		if (other.transform.CompareTag("Enemy")) {
			AI ai = other.gameObject.GetComponent<AI>();
			ai.damage(5, this.caster, this.playerServer);
			Debug.Log("It hit! " + this.server + " | " + " collision");
			NetworkServer.Destroy(this.gameObject);
		}

    }
}
