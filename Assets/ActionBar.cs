using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionBar : MonoBehaviour {

	public Skill skill;
	public GameObject skillIcon;

	void Update(){
		if(Input.GetKeyDown(KeyCode.F10)){
			Debug.Log("IS IT NULL THOUGH? " + skill);
		}
	}
}
