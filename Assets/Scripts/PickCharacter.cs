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

	void Start () {
        originalPosition = this.transform.position;
	}
    public void setLogin(Login login) {
        this.login = login;
    }

	void Update () {
        if (Input.GetMouseButtonDown(0)) {
            GameObject hitObj = Tools.mouse3D(camera);
			if(hitObj != null && login != null)
	            if (hitObj.Equals(this.gameObject)) {
	                login.characterName = this.name;
	                login.client.Disconnect();      
                    login.loadWorld();
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
        GameObject obj = Tools.mouse3D(camera);
        if (obj == null) return false;
        return (obj.Equals(this.gameObject));
    }
}
