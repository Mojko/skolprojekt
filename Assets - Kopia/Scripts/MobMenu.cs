using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
public class MobMenu : MonoBehaviour{

    /*
	int state = (int)e_states.IDLE;
	int range = 4;		
	private Vector3 newPos;
	//Oldpos behövs för att veta vart enemyn spawnade, så den kan patrullera runt där
	public Vector3 oldPos;

	Animator animator;

	//Target är objektet som AI'en ska jaga.
	GameObject target;

	float attackTimer = 0f;
	float hitTimer = 0f;
	float changeDirectionTimer = 0f;

	bool coroutineStarted = false;

	void Start () {
		animator = GetComponent<Animator>();
		oldPos = this.transform.position;
	}


	void Update () {
		//Klienten får inte acessa AI koden
		switch (state) {
		case (int)e_states.IDLE:
			if (!coroutineStarted) {
				StartCoroutine (idle ());
			}
			break;
		case (int)e_states.CHASE:
			chase ();
			break;

		case (int)e_states.ATTACK:
			if (!coroutineStarted) {
				StartCoroutine(attack());
			}
			break;
		}

		if (target != null) {
			state = (int)e_states.CHASE;
		}

		if (newPos != null || GetComponent<NavMeshAgent>().destination != newPos) {
			GetComponent<NavMeshAgent> ().SetDestination(newPos);
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

	IEnumerator attack () {

		coroutineStarted = true;
		yield return new WaitForSeconds(2);
		coroutineStarted = false;

		animator.SetBool ("attack", true);
		bool isName = animator.GetCurrentAnimatorStateInfo (1).IsName ("Attack");
		bool finished = animator.GetCurrentAnimatorStateInfo (1).normalizedTime > 1f;

		if (finished) {
			//Deal damage
			target.GetComponent<PlayerStats>().damage(5);
		}
	}

	IEnumerator idle(){
		coroutineStarted = true;
		setNewPos(chooseDestination());

		yield return new WaitForSeconds(this.range);
		coroutineStarted = false;
	}

	void chase () {
		if (target != null) {
			setNewPos(target.transform.position - this.transform.forward * 2f);
		}

		if (Vector3.Distance (this.transform.position, target.transform.position) > 5f) {
			state = (int)e_states.IDLE;
		}

		bool isDistanceCloseEnough = Vector3.Distance (transform.position, target.transform.position) < 1f;
		if (isDistanceCloseEnough) {
			state = (int)e_states.ATTACK;
		}
	}

	void setNewPos(Vector3 pos){
		this.newPos = pos;
	}
    */
}
