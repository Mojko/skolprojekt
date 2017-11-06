using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AI;

public class MobManager : NetworkBehaviour {
	
	int health = 3;
 	int mana;
	public Vector3 newPos;
	public Respawner respawner;
    public Spawner spawner;
	
	public GameObject drop;
	public GameObject part;
    public GameObject target;

    float flashTimer = 1;

    public Vector3 rootPos;

    public void stun(int damage, int duration) {

	}

	public void heal(int amount) {

	}

	public void damage (int damage, GameObject damager) {
        StartCoroutine(flash());
		health -= 1;
        target = damager;
		if (health <= 0) {
			kill();
		}
	}
	void kill(){
		if(!isServer) return;
        if(this.respawner == null) this.respawner = GameObject.FindWithTag("Respawner").GetComponent<Respawner>();
        if(this.spawner == null) this.spawner = GameObject.FindWithTag("Spawner").GetComponent<Spawner>();

        Vector3 pos = new Vector3(this.transform.position.x, this.transform.position.y + 0.5f, this.transform.position.z) + this.transform.forward;
        Server.spawnObject(e_Objects.PARTICLE_DEATH, pos);
		//Server.spawnObject(drop, this.transform.position);
		//Server.spawnObject(part, new Vector3(this.transform.position.x, this.transform.position.y + 0.5f, this.transform.position.z) + this.transform.forward);
        spawner.totalEnemiesInArea--;
		NetworkServer.Destroy(this.gameObject);
	}
	
	public int getHealth (){
		return health;
	}
	public int getMana (){
		return mana;
	}
	public void setHealth (int health){
		this.health = health;
	}
	public void setMana (int mana){
		this.mana = mana;
	}

	void toggleFlash(int state){
		this.transform.GetChild (1).GetChild (0).GetComponent<Renderer> ().material.SetFloat ("_Flash", state);
		this.transform.GetChild (1).GetComponent<Renderer> ().material.SetFloat ("_Flash", state);
	}

    [ClientRpc]
    void RpcToggleFlash(int state)
    {
        GameObject[] children = Tools.getChildrenByTag(this.gameObject, "Body");
        foreach(GameObject o in children) {
            o.GetComponent<Renderer>().material.SetFloat("_Flash", state);
        }
    }

    IEnumerator flash()
    {
        RpcToggleFlash(0);
        yield return new WaitForSeconds(0.1f);
        RpcToggleFlash(1);
    }
}
