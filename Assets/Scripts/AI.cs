using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.Networking;

enum e_States {
	IDLE,
	CHASE,
	ATTACK
}


/*
	Funktion: Används för att gå runt, reagera till olika situationer.
	Datum: 2017-09-24
*/


public class AI : MobManager {
	e_States state = e_States.IDLE;
	int range = 4;		
	
	//Oldpos behövs för att veta vart enemyn spawnade, så den kan patrullera runt där
	[Header("AI")]
	[Header("Leave alone")]
	public Vector3 oldPos;

	[Space(10)]
	[Header("Fill in")]
	public GameObject biteEffectPrefab;
    public GameObject body;

	private float speed;
	private float angularSpeed;
	private Animator animator;
    private float attackRange = 1.5f;
	private bool coroutineStarted = false;
    private Timer timer;
	private bool isFreezed;
    private bool hasDamaged;
	private NavMeshAgent agent;
	private bool shouldRotate;
	private bool hasActivatedEffect = false;
    private Renderer bodyRenderer;
    private FaceManager faceManager;


	void Start () {
		if(!isServer) return;
		animator = GetComponent<Animator>();
        oldPos = this.transform.position;
        setNewDestination(chooseDestination());
        this.spawner = GameObject.FindWithTag("Spawner").GetComponent<Spawner>();
        this.respawner = GameObject.FindWithTag("Respawner").GetComponent<Respawner>();
		this.agent = this.GetComponent<NavMeshAgent>();
		this.speed = agent.speed;
		this.angularSpeed = agent.angularSpeed;
		this.server = GameObject.FindWithTag("Server").GetComponent<Server>();
        this.faceManager = GetComponent<FaceManager>();
        body = this.transform.Find(body.name).gameObject;
        this.bodyRenderer = body.GetComponent<Renderer>();
	}

	void Update () {
		//Klienten får inte acessa AI koden
		if (!isServer)
			return;
        if(timer != null){
            timer.update();
            //if(timer.isFinished()) timer = null;
        }
        
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
		}

        if(newPos != null) {
            this.GetComponent<NavMeshAgent>().destination = newPos;
        }
	}

	//Ta en random destination runt en range
	public Vector3 chooseDestination(){
		return new Vector3(oldPos.x + Random.Range(-range, range), oldPos.y, oldPos.z + Random.Range(-range, range));
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
        startTimerIfNotStarted(4);

		if(!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")){
            faceManager.setFace(this.bodyRenderer, e_Faces.DEFAULT);
			animator.Play("Idle", 0, 0);
		}

		if(this.target != null){
			setState(e_States.CHASE);
		}

        if (this.timer.isFinished()) {
            setNewDestination(chooseDestination());
        }
    }
    void chaseState()
    {
    	followTarget(this.target);

		if(!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")){
            faceManager.setFace(this.bodyRenderer,e_Faces.ANGRY);
			animator.Play("Idle", 0, 0);
		}

        if(isTargetOutOfRange(this.target, 25)) {
            setState(e_States.IDLE);
			return;
        }
        RaycastHit rayHit;
        if (canAttack(out rayHit)) {
            //Then attack lol
			setState(e_States.ATTACK);
			return;
        }
    }

	Quaternion rotateTowards(Transform transform, Transform target, float speed) {
		Quaternion r = Quaternion.LookRotation((target.position - transform.position));
		return Quaternion.Slerp(transform.rotation, r, speed * Time.deltaTime);
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
			shouldRotate = true;
		} else {
			freeze();
			if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.7f && !hasDamaged){
				hasDamaged = true;

                RaycastHit rayHit;
                if(canAttack(out rayHit)) {
				    if(rayHit.transform.CompareTag("Player")){
                       	this.target.GetComponent<Player>().damage(5, this.gameObject);
				    }
                }

			}

			if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.7f && !hasActivatedEffect){
				Instantiate(biteEffectPrefab).transform.position = new Vector3(this.transform.position.x, this.transform.position.y+1, this.transform.position.z) + (transform.forward*1.2f);
				hasActivatedEffect = true;
			}

			if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f){
				RaycastHit rayHit;
				hasActivatedEffect = false;
                if(canAttack(out rayHit)){
					animator.Play("Bite", 0, 0);
				} else {
					setState(e_States.IDLE);
					unfreeze();
				}
				hasDamaged = false;
			}
		}


		/*if(!animator.GetCurrentAnimatorStateInfo(0).IsName("Bite")){
			animator.Play("Bite");
			freeze();
		}

		if(animator.GetCurrentAnimatorStateInfo(0).IsName("Bite")){
			freeze();
		}

		if(!hasDamaged && animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.8f && animator.GetCurrentAnimatorStateInfo(0).IsName("Bite")){
			target.GetComponent<Player>().damage(5);
			hasDamaged = true;
		}

		if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1f && animator.GetCurrentAnimatorStateInfo(0).IsName("Bite")) {
			//Deal damage
			target.GetComponent<Player>().damage(5);
			animator.Play("Bite", 0, 0);

			if(!canAttack()){
				setState(e_States.IDLE);
				unfreeze();
			}

			Debug.Log("attacking!!!");
		}*/
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

    bool canAttack(out RaycastHit rayHit)
    {
        if(Physics.Raycast(this.transform.position, this.transform.forward, out rayHit, this.attackRange)) {
            return true;
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
}
