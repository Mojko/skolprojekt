using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.Networking;




/*
	Funktion: Används för att gå runt, reagera till olika situationer.
	Datum: 2017-09-24
*/

public enum e_BossStates {
	SKILL_USE,
	TARGET
}


public class AI : MobManager {
	int range = 4;		
	
	//Oldpos behövs för att veta vart enemyn spawnade, så den kan patrullera runt där
	[HideInInspector]
	public Vector3 oldPos;
	public GameObject biteEffectPrefab;
    public GameObject body;
	public GameObject angerLight;

	private float speed;
	private float angularSpeed;
    private float attackRange = 1.5f;
	private bool coroutineStarted = false;
    private Timer timer;
	private bool isFreezed;
    private bool hasDamaged;
	private NavMeshAgent agent;
	private bool shouldRotate;
	private bool hasActivatedEffect = false;
	private Renderer renderer;
	private Renderer[] childRenderers;
    private Renderer bodyRenderer;
    private FaceManager faceManager;
	private NetworkIdentity identity;
	private e_BossStates bossState = e_BossStates.TARGET;
	private float bossTimer = 10f;

	[Header("Special Properties")]
	[Space(10)]
	public bool friendly;
	public bool charge;
	public bool isBoss;
	public GameObject magicBubble;
	bool startBossFight;

	void Start () {
		if(isClient){
			this.GetComponent<NavMeshAgent>().enabled = false;
		}

		this.body = this.transform.Find(body.name).gameObject;
		this.bodyRenderer = body.GetComponent<Renderer>();
		this.faceManager = GetComponent<FaceManager>();
		faceManager.initilize(this);
		animator = GetComponent<Animator>();

		if(!isServer) return;
        oldPos = this.transform.position;
        setNewDestination(chooseDestination());
        this.spawner = GameObject.FindWithTag("Spawner").GetComponent<Spawner>();
        //this.respawner = GameObject.FindWithTag("Respawner").GetComponent<Respawner>();
		this.agent = this.GetComponent<NavMeshAgent>();
		this.speed = agent.speed;
		this.angularSpeed = agent.angularSpeed;
		this.server = GameObject.FindWithTag("Server").GetComponent<Server>();
		this.renderer = GetComponent<Renderer>();
		this.childRenderers = GetComponentsInChildren<Renderer>(true);
		this.identity = GetComponent<NetworkIdentity>();

		if(this.getId() == 0){
			this.setId(10000);
		}
		this.monster = lookupMonster(this.getId());
		this.health = this.monster.health;

		Debug.Log("HEALTH MONSTER OMG : " + this.health + " | " + this.monster.health);
	}

	void Update () {
		//Klienten får inte acessa AI koden
		if (!isServer)
			return;
        if(timer != null){
            timer.update();
            //if(timer.isFinished()) timer = null;
        }
        
		if(!isBoss){
			switch (state) {
			case e_States.IDLE:
				idleState();
				break;
			case e_States.CHASE:
				chaseState();
				break;

			case e_States.ATTACK:
				attackState();
				break;
			case e_States.DIE:
				dieState();
				break;
			} 
		} else {

			Collider[] col = Physics.OverlapSphere(this.transform.position, 40);
			foreach(Collider c in col){
				if(c.CompareTag("Player")){
					startBossFight = true;
				}
			}

			if(startBossFight){
				bossTimer -= 1 * Time.deltaTime;
				if(bossTimer <= 0){
					bossState = e_BossStates.SKILL_USE;
				}
				switch(bossState){
				case e_BossStates.SKILL_USE:
					skillUseBossState();
					break;

				case e_BossStates.TARGET:
					chaseBossState();
					break;
				}
			}
		}

		if(newPos != null && state != e_States.DIE && !isBoss) {
            this.GetComponent<NavMeshAgent>().destination = newPos;
        }
	}

	//Ta en random destination runt en range
	public Vector3 chooseDestination(){
		if(spawner != null){
			return this.spawner.randomVector3InRadius();
		}
		return Vector3.zero;
		//return new Vector3(oldPos.x + Random.Range(-range, range), oldPos.y, oldPos.z + Random.Range(-range, range));
	}

	bool hasReachedDestination () {
		if (Vector3.Distance (this.transform.position, newPos) < 0.5f) {
			return true;
		}
		return false;
	}

    public void startTimerIfNotStarted(int time)
    {
        if(this.timer == null) this.timer = new Timer(time, true);
    }

    void idleState()
    {
		startTimerIfNotStarted(Random.Range(4,8));

		if(!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")){
			toggleAngry(false);
            faceManager.setFace(this.bodyRenderer, e_Faces.DEFAULT);
			animator.Play("Idle", 0, 0);
			RpcSyncAnimation("Idle");
		}

		if(charge){
			Collider[] c = Physics.OverlapSphere(this.transform.position, 5);
			foreach(Collider col in c){
				if(col.CompareTag("Player")){
					target = col.gameObject;
					break;
				}
			}
		}

		if(this.target != null && !friendly){
			animator.Play("Idle");
			setState(e_States.CHASE);
		}

        if (this.timer.isFinished()) {
            setNewDestination(chooseDestination());
        }
    }

	void toggleAngry(bool toggle){
		if(angerLight != null){
			angerLight.SetActive(toggle);
		}
	}

	void skillUseBossState(){
		int rand = Random.Range(0,2);
		if(rand <= 1){
			for(int i=0;i<3;i++){
				Vector3 pos = Vector3.zero;;
				if(i == 0){
					pos = new Vector3(this.transform.position.x - (transform.lossyScale.x * 5), this.transform.position.y,this.transform.position.z);
				}
				if(i == 1){
					pos = new Vector3(this.transform.position.x + (transform.lossyScale.x * 5), this.transform.position.y,this.transform.position.z);
				}
				if(i == 2){
					pos = new Vector3(this.transform.position.x, this.transform.position.y,this.transform.position.z + (this.transform.lossyScale.z * 5));
				}
				Server.spawnMonster(10000, pos);
				Server.spawnObject(e_Objects.PARTICLE_DEATH, pos);
				this.bossTimer = 10;
				this.bossState = e_BossStates.TARGET;
			}
		}
	}

	void chaseBossState(){
		if(faceManager.getFace() != e_Faces.ANGRY){
			faceManager.setFace(this.bodyRenderer,e_Faces.ANGRY);
		}
	}

    void chaseState()
    {
    	followTarget(this.target);

		if(faceManager.getFace() != e_Faces.ANGRY){
			faceManager.setFace(this.bodyRenderer,e_Faces.ANGRY);
			toggleAngry(true);
		}

        if(isTargetOutOfRange(this.target, 25)) {
            setState(e_States.IDLE);
			return;
        }
        RaycastHit rayHit;
        if (canAttack(out rayHit, "Player")) {
            //Then attack lol
			setState(e_States.ATTACK);
			return;
        }
    }

	Quaternion rotateTowards(Transform transform, Transform target, float speed) {
		Quaternion r = Quaternion.LookRotation((target.position - transform.position));
		return Quaternion.Slerp(transform.rotation, r, speed * Time.deltaTime);
	}

	void dieState(){
		freeze();
		if(!animator.GetCurrentAnimatorStateInfo(0).IsName("Die")){
			animator.Play("Die",0,0);
			RpcSyncAnimation("Die");
		} else {
			if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1){
				kill();
			}
		}
		foreach(Renderer renderer in childRenderers){
			float a = renderer.material.color.a;
			a -= 100f * Time.deltaTime;
			renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, a);
		}
	}

	[ClientRpc]
	public void RpcSetFace(e_Faces face){
		if(this.faceManager != null && this.bodyRenderer != null){
			this.faceManager.setFace(this.bodyRenderer, face);
		}
	}
	[ClientRpc]
	public void RpcInstansiateBite(){
		GameObject o = Instantiate(this.biteEffectPrefab);
		Instantiate(o);
		o.transform.position = new Vector3(this.transform.position.x, this.transform.position.y+1, this.transform.position.z) + (transform.forward*1.2f);
	}
	[ClientRpc]
	public void RpcSyncAnimation(string animationName){
		if(animator != null){
			animator.Play(animationName,0,0);
		}
	}

    void attackState()
    {
		followTarget(this.target);
		if(shouldRotate){
			this.transform.rotation = rotateTowards(this.transform, target.transform, 10);
			
            RaycastHit rayHit;
            if(Physics.Raycast(this.transform.position, this.transform.forward, out rayHit, 2)) {
				if(rayHit.transform.CompareTag("Player")){
					shouldRotate = false;
				}
            }
		}
		if(!animator.GetCurrentAnimatorStateInfo(0).IsName("Bite")){
			animator.Play("Bite", 0, 0);
			faceManager.setFace(this.bodyRenderer, e_Faces.ATTACK);
			shouldRotate = true;
		} else {
			freeze();
			if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.7f && !hasDamaged){
				hasDamaged = true;
				RpcInstansiateBite();
                RaycastHit rayHit;
                if(canAttack(out rayHit, "Player")) {
					this.dealDamage();
                }
			}

			if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.7f && !hasActivatedEffect){
				
				faceManager.setFace(this.bodyRenderer, e_Faces.ANGRY);
				hasActivatedEffect = true;
			}

			if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f){
				RaycastHit rayHit;
                if(canAttack(out rayHit, "Player")){
					animator.Play("Bite", 0, 0);
					RpcSyncAnimation("Bite");
				} else {
					setState(e_States.IDLE);
					unfreeze();
				}
				hasDamaged = false;
				hasActivatedEffect = false;
			}
		}
    }

	public void freeze(){
		agent.speed = 0;
		agent.angularSpeed = 0;

		this.isFreezed = true;
	}

	public void unfreeze(){
		agent.speed = this.speed;
		agent.angularSpeed = this.angularSpeed;
		this.isFreezed = false;
	}

    void setState(e_States newState)
    {
        this.state = newState;
    }

    e_States getState(){
        return this.state;
    }

	bool canAttack(out RaycastHit rayHit, string tag)
    {
        if(Physics.Raycast(this.transform.position, this.transform.forward, out rayHit, this.attackRange)) {
			if(rayHit.collider.gameObject.CompareTag(tag)){
            	return true;
			}
        }
		/*bool isDistanceCloseEnough = Vector3.Distance (transform.position, target.transform.position) < attackRange;
		if (isDistanceCloseEnough) {
			return true;
		}*/
        return false;
    }

    void followTarget(GameObject target)
    {
        if(target != null) {
			setNewDestination(target.transform.position - this.transform.forward * (attackRange/2));
        }
    }
    bool isTargetOutOfRange(GameObject target, float range)
    {
        if(target != null){
		    if (Vector3.Distance (this.transform.position, target.transform.position) > range) {
			    return true;
		    }
        }
        return false;
    }

	void setNewDestination(Vector3 pos){
		this.newPos = pos;
	}

	[Command]
	public void CmdSendDestinationToServer(Vector3 pos){
		RpcSendDestinationToClients(pos);
	}

	[ClientRpc]
	public void RpcSendDestinationToClients(Vector3 pos){
		this.GetComponent<NavMeshAgent>().destination = pos;
	}

	[Command]
	public void CmdSyncGameObject(NetworkInstanceId netId, Vector3 position, Quaternion rot)
	{
		/*GameObject o = NetworkServer.FindLocalObject(netId);
		o.transform.position = position;
		o.transform.rotation = rot;
		RpcSyncGameObject(netId, position, rot);*/

	}
	[ClientRpc]
	public void RpcSyncGameObject(Vector3 position, Quaternion rot)
	{
		this.transform.position = position;
		this.transform.rotation = rot;
	}
}
