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
	public Vector3 oldPos;
	private Animator animator;
    private float attackRange = 2f;
	private bool coroutineStarted = false;
    private Timer timer;


	void Start () {
		if(!isServer) return;
		animator = GetComponent<Animator>();
        oldPos = this.transform.position;
        setNewDestination(chooseDestination());
        this.spawner = GameObject.FindWithTag("Spawner").GetComponent<Spawner>();
        this.respawner = GameObject.FindWithTag("Respawner").GetComponent<Respawner>();
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
            idle();
			break;
		case e_States.CHASE:
			chase ();
			break;

		case e_States.ATTACK:
            attack();
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

	/*IEnumerator attack () {
		coroutineStarted = true;
		yield return new WaitForSeconds(2);
		coroutineStarted = false;


		animator.SetBool ("attack", true);
		bool isName = animator.GetCurrentAnimatorStateInfo (1).IsName ("Attack");
		bool finished = animator.GetCurrentAnimatorStateInfo (1).normalizedTime > 1f;

		if (finished) {
			//Deal damage
			target.GetComponent<Player>().damage(5);
		}

        Debug.Log("attacking!!!");
	}

	IEnumerator idle(){
		coroutineStarted = true;
		setNewPos(chooseDestination());
		
		yield return new WaitForSeconds(this.range);
		coroutineStarted = false;
	}
	
	void chase () {
        float attackRange = 2f;
		if (target != null) {
			setNewPos(target.transform.position - this.transform.forward * attackRange);
		}

		if (Vector3.Distance (this.transform.position, target.transform.position) > 5f) {
			state = (int)e_states.IDLE;
		}

		bool isDistanceCloseEnough = Vector3.Distance (transform.position, target.transform.position) < attackRange;
		if (isDistanceCloseEnough) {
			state = (int)e_states.ATTACK;
		}
	}*/
    public void startTimerIfNotStarted(int time)
    {
        if(this.timer == null) this.timer = new Timer(time, true);
    }

    void idle()
    {
        startTimerIfNotStarted(4);
        if (this.timer.isFinished()) {
            setNewDestination(chooseDestination());
        }
    }
    void chase()
    {
        followTarget(this.target);
        if(isTargetOutOfRange(this.target, 5)) {
            setState(e_States.IDLE);
        }
        if (canAttack()) {
            //Then attack lol
            state = e_States.ATTACK;
        }
    }
    void attack()
    {
        startTimerIfNotStarted(2);

		animator.SetBool ("attack", true);
		bool isName = animator.GetCurrentAnimatorStateInfo (1).IsName ("Attack");
		bool finished = animator.GetCurrentAnimatorStateInfo (1).normalizedTime > 1f;

		if (finished) {
			//Deal damage
			target.GetComponent<Player>().damage(5);
		}

        Debug.Log("attacking!!!");
    }

    void setState(e_States newState)
    {
        this.state = newState;
    }

    e_States getState(){
        return this.state;
    }

    bool canAttack()
    {
		bool isDistanceCloseEnough = Vector3.Distance (transform.position, target.transform.position) < attackRange;
		if (isDistanceCloseEnough) {
			return true;
		}
        return false;
    }

    void followTarget(GameObject target)
    {
        if(target != null) {
            setNewDestination(target.transform.position - this.transform.forward * attackRange);
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
