using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class Drop : MonoBehaviour {
	public Item item;
	public int[] debug;
	Renderer renderer;

	public GameObject[] dropAbleObjects;
	GameObject thisObject;

	void Start(){
		this.transform.rotation = Quaternion.Euler (dropAbleObjects [0].transform.rotation.x, 0, dropAbleObjects [0].transform.rotation.z);
		this.transform.localScale = dropAbleObjects[0].transform.lossyScale;
		this.renderer = this.GetComponent<Renderer> ();
	}

	void OnTriggerStay(Collider col) {
		this.renderer.material.color = Color.white;
		if (col.gameObject.CompareTag("Player") && Input.GetKey(KeyCode.B)) {
			Player player = col.gameObject.GetComponent<Player>();

		}
	}
	void OnTriggerExit(Collider col){
		this.renderer.material.color = Color.gray;	
	}
}
