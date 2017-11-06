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
        
        /*Collider[] colliders = Physics.OverlapSphere(this.transform.position, 4);
        if(colliders.Length > 0 && target == null) {
            target = colliders[0].gameObject;
        }
        if (target != null) {
            this.transform.rotation = tools.rotateTowards(this.transform.position, target.transform.position);
            if(Vector3.Distance(this.transform.position, target.transform.position) < 1f) {
                Destroy(this.gameObject);
            }
        }*/
	}
    private void OnTriggerEnter(Collider collision)
    {
        if(collision.transform.CompareTag("Enemy")){
            Destroy(this.gameObject);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.CompareTag("Enemy")){
            Destroy(this.gameObject);
        }
    }
}
