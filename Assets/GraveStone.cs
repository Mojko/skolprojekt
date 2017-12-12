using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraveStone : MonoBehaviour {

	[HideInInspector] public Player player;

	public void respawn(){
		player.gameObject.SetActive(true);
		player.transform.position = new Vector3(-30, 0, 16);
		player.respawn();
	}
	public void onClick(){
		respawn();
	}
}
