using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum e_FadeType {
	IN,
	OUT
}
	

public class FadeUI : MonoBehaviour {

	public e_FadeType type;
	public int speed;
	CanvasGroup canvasGroup;
	public float timeUntilSwitchFade = 3f;

	void Start(){
		canvasGroup = this.GetComponent<CanvasGroup>();
		if(canvasGroup == null) Debug.LogError("Add canvas group to LevelUp_UI object");
	}

	void Update(){
		if(type == e_FadeType.IN)
			fadeIn();
		else 
			fadeOut();
	}

	void fadeIn(){
		this.canvasGroup.alpha += speed * Time.deltaTime;

		if(this.canvasGroup.alpha >= 1){
			timeUntilSwitchFade -= 1 * Time.deltaTime;
			if(timeUntilSwitchFade <= 0) {
				fadeOut();
				type = e_FadeType.OUT;
			}
		}
	}
	void fadeOut(){
		this.canvasGroup.alpha -= speed * Time.deltaTime;
		if(this.canvasGroup.alpha <= 0) Destroy(this.gameObject);
	}

	/*public e_FadeType type;
	public int speed;

	private Renderer renderer;
	private Image[] childImages;
	private Color[] rgba;


	void Start () {
		childImages = this.GetComponentsInChildren<Image>(true);

		for(int i=0;i<childImages.Length;i++){
			rgba[i] = childImages[i].material.color;
		}
	}

	void Update () {
	}

	void fadeIn(){
		foreach(Color c in rgba){
			if(c.a > 0){
				c.a -= 0.25f * Time.deltaTime;
			} else {
				Destroy(this.gameObject);
				foreach(Transform t in this.transform.getAllChildren){
					Destroy(t.gameObject);
				}
			}
		}
	}

	void fadeOut(){

	}*/
}
