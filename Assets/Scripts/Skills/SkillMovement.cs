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
    Player player;

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
        if (other.transform.CompareTag("Enemy")) {
            AI ai = other.GetComponent<AI>();
            ai.damage(5,null);
            Destroy(this.gameObject);
        }        
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Enemy")) {
            AI ai = collision.gameObject.GetComponent<AI>();
            ai.damage(5, null);
            Destroy(this.gameObject);
        }
    }
}
