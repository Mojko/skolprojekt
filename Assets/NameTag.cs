using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameTag : MonoBehaviour {


	[HideInInspector] public Player player;

	void Update () {
		if(player != null){
			this.transform.position = new Vector3(player.transform.position.x + 1.8f, player.transform.position.y+0.5f, player.transform.position.z);
		}
	}
}
