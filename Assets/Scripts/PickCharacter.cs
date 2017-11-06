using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class PickCharacter : MonoBehaviour {
    public bool isEmpty;
    public Camera camera;
    private Vector3 originalPosition;
    private bool hadMouseOver = false;
    Login login;
    public string name;
	// Use this for initialization
	void Start () {
        originalPosition = this.transform.position;
	}
    public void setLogin(Login login) {
        this.login = login;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0)) {
            GameObject hitObj = mouse3D();
			if(hitObj != null && login != null)
	            if (hitObj.Equals(this.gameObject)) {
	                Debug.Log("clicked on: " + this.name);
	                login.characterName = this.name;
	                login.client.Disconnect();
	                login.manager.StartClient();
	            }
        }
        if (isMouseOver())
        {
            this.gameObject.transform.position = Vector3.Slerp(this.transform.position, new Vector3(this.transform.position.x, originalPosition.y + 0.2f, this.transform.position.z),6f*Time.deltaTime);
            hadMouseOver = true;
        }
        else if(hadMouseOver)
        {
            if (Vector3.Distance(this.transform.position, originalPosition) < 0.01f) {
                this.gameObject.transform.position = originalPosition;
                hadMouseOver = false;
            }
            this.gameObject.transform.position = Vector3.Slerp(this.transform.position, originalPosition, 6f * Time.deltaTime);
        }
	}
    private void onCharPicked(NetworkMessage msg) {
        login.client.Disconnect();
        login.manager.StartClient();
    }
    private bool isMouseOver() {
        GameObject obj = mouse3D();
        if (obj == null) return false;
        return (obj.Equals(this.gameObject));
    }
    private GameObject mouse3D() {
        RaycastHit hit;
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit)) {
            return hit.transform.gameObject;
        }
        return null;
    }
}
