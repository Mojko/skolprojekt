using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour {
	public Material blackMaterial;

	private void OnTriggerStay(Collider col){
		if(col.CompareTag("Player")){
			if(Input.GetKeyDown(KeyCode.Z)){
				GameObject w = GameObject.Find("World2");
				col.gameObject.transform.position = w.transform.Find("World2Location").transform.position;
				RenderSettings.skybox = blackMaterial;
			}
		}
	}
}
