using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum e_cameraStates {
	NONE,
	DEFAULT, //moving etc
	TALKING_TO_NPC,
	PAN
}

public class MainCamera : MonoBehaviour {

	int state = (int)e_cameraStates.NONE;
	public GameObject player;
	private Vector3 pos;
	private Vector3 fromPos;
	private Quaternion angle;
	private float panSpeed;
	private float rotationSpeed;

	public float xoffset;
	public float yoffset;
	public float zoffset;
	
	public float xrot;
	public float yrot;
	public float zrot;

    private void Start()
    {
        this.player = GameObject.FindWithTag("Player");
    }

    void Update () {

		switch (state) {
		//Default, när man går etc.
		case (int)e_cameraStates.DEFAULT:
			pos = player.transform.position;
			transform.position = new Vector3(pos.x+(-4.96f),pos.y+(7),pos.z+(-9.11f));
			transform.rotation = Quaternion.Euler(19.23f,30.52f,0);
			break;
		//När man pratar med en NPC
		case (int)e_cameraStates.TALKING_TO_NPC:
			pos = player.GetComponent<Player>().npcTalkingTo.transform.position;
			transform.position = new Vector3(pos.x+xoffset,pos.y+yoffset,pos.z+zoffset);
			transform.rotation = Quaternion.Euler(xrot,yrot,zrot);
			break;
		case (int)e_cameraStates.PAN:
			transform.position = Vector3.Lerp (transform.position, pos, panSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, angle, rotationSpeed * Time.deltaTime);
			break;
		}
	}
	public void panTo(Vector3 position, Quaternion angle, float speed, float rotationSpeed){
		pos = position;
		panSpeed = speed;
		this.angle = angle;
		fromPos = this.transform.position;
		this.rotationSpeed = rotationSpeed;
		this.state = (int)e_cameraStates.PAN;
	}
	public void setState(int state){
		this.state = state;
	}
}
