using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingEffect : MonoBehaviour {

	float v = 0;
	float rotSpeed = 1f;
	public float amp = 0.01f;
	public float period = 10;
	public float periodSpeed = 1f;

	void Update () {
		v += periodSpeed;
		this.transform.position += new Vector3(0,Mathf.Sin(v/period) * amp,0);
		this.transform.rotation = Quaternion.Euler(this.transform.rotation.x,this.transform.rotation.y+rotSpeed,this.transform.rotation.z);
	}
}
