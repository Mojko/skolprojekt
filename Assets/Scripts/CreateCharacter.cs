using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateCharacter : MonoBehaviour {

    public MainCamera camera;
    public Camera realCamera;
    public GameObject character;
    private GameObject parentObject;
    private CreateCharacterUI ui;
	void Start () {
		
	}
    public void setCamera(MainCamera camera) {
        this.camera = camera;
        this.realCamera = camera.gameObject.GetComponent<Camera>();
    }
    public void setUI(CreateCharacterUI ui) {
        this.ui = ui;
    }
    public void setCharacter(GameObject parent, GameObject playerModel) {
        parentObject = parent;
        character = (GameObject)Instantiate(playerModel);
        character.transform.SetParent(parentObject.transform);
        character.transform.localPosition = new Vector3(0,2.7f,0);
    }
    IEnumerator onPanDone(float time) {
        yield return new WaitForSeconds(time);
        this.ui.setCharacter(character);
        this.ui.slideIn();
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject hitObj = Tools.mouse3D(realCamera);
            if (hitObj == null) return;
            if (hitObj.Equals(this.gameObject.transform.GetChild(0).gameObject)) {
                Debug.Log("camera panning here");
                camera.panTo(new Vector3(16,14.5f,-1), Quaternion.Euler(-8,190,0),2f,3f);
                StartCoroutine(onPanDone(1.5f));
            }
        }
    }
}
