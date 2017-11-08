using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalTools;

public class SkillMovement : MonoBehaviour {

    public float speed = 4f;
	private GameObject target;
    ToolsGlobal tools = new ToolsGlobal();
    public GameObject activationEffect;

    private void Update () 
    {
        
		this.transform.position += transform.forward * speed * Time.deltaTime;

		Collider[] colliders = Physics.OverlapSphere(this.transform.position, 2);


		if(target == null){
			for(int i=0;i<colliders.Length;i++){
				if(colliders[i].CompareTag("Enemy")){

					target = colliders[i].gameObject;
					float yDeltaRotation = Mathf.Abs((tools.rotateTowards(this.transform, target.transform.position).eulerAngles.y - this.transform.rotation.eulerAngles.y));

					Debug.Log("yDeltaRotation: " + yDeltaRotation);

					if(yDeltaRotation >= 90){
						target = null;
						continue;
					}
						
				}
			}
		}
        if (target != null) {
            //this.transform.rotation = tools.rotateTowards(this.transform.position, target.transform.position);
			this.transform.rotation = tools.rotateTowards(this.transform.transform, target.transform.position); 
            if(Vector3.Distance(this.transform.position, target.transform.position) <= 1.1f) {
                Destroy(this.gameObject);
            }
		}
	}
}
