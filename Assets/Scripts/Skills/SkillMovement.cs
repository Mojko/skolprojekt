using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalTools;
using UnityEngine.Networking;

public class SkillMovement : SkillCastManager {

    public float speed = 4f;

    /*private void Update () 
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
	}*/
}
