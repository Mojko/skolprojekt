using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum e_ScaleType {
	CYLINDER
}

public class ParticleScaler : MonoBehaviour {
	public e_ScaleType type;
	public float[] speed;
	public float maxScale;
	bool isScaled;
	public GameObject objectAttachedTo;
	public float timerUntilKill = 6f;
    private RectTransform rect;

	public GameObject levelUpUI;

	void Start(){
		levelUpUI = Instantiate(levelUpUI);
		levelUpUI.transform.SetParent(GameObject.Find("UI").transform);
        rect = levelUpUI.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 100);
        rect.transform.localPosition = new Vector2(0,0);
	}

	void Update(){
		if(objectAttachedTo != null) 
			this.transform.position = objectAttachedTo.transform.position;
		
		timerUntilKill -= 1 * Time.deltaTime;
		if(timerUntilKill <= 0){
			scale(-1);
		}
		if(isScaled) return;
		switch(type){
		case e_ScaleType.CYLINDER:
			scale(1);
			break;
		}
	}

	public void scale(int direction){
		if(this.transform.localScale.x >= maxScale && this.transform.localScale.z >= maxScale && this.transform.localScale.y < maxScale)
			this.transform.localScale += new Vector3(0,speed[1]*Time.deltaTime*direction,0);
		else 
			this.transform.localScale += new Vector3(speed[0]*Time.deltaTime*direction,0,speed[2]*Time.deltaTime*direction);

		if(this.transform.localScale.x < 0 || this.transform.localScale.z < 0 || this.transform.localScale.y < 0) Destroy(this.gameObject);
		if(this.transform.localScale.x >= maxScale && this.transform.localScale.z >= maxScale && this.transform.localScale.y >= maxScale) isScaled = true;
	}
}
