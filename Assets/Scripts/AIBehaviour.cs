using System.Collections.Generic;
using UnityEngine;

public class AIBehaviour : StateMachineBehaviour {

	public AI ai;

	public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		ai = animator.GetComponent<AI>();
		base.OnStateEnter (animator, stateInfo, layerIndex);
	}
	public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit (animator, stateInfo, layerIndex);
	}
	public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate (animator, stateInfo, layerIndex);
	}
}
