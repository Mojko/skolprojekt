using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FloatingEffect : NetworkBehaviour {

	float v = 0;
	public float rotSpeed = 40f;
	public float amp = 1f;
	public float period = 10;
	public float periodSpeed = 0.1f;

	void Update () {
		v += periodSpeed;
		this.transform.position += new Vector3(0,(Mathf.Sin(v/period) * amp * Time.deltaTime),0);
		this.transform.Rotate(new Vector3(0,rotSpeed * Time.deltaTime,0));
	}
}
