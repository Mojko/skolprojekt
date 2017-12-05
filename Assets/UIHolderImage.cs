using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIHolderImage : MonoBehaviour {

	private Image img;
	private RectTransform rectTransform;
	private Vector2 pos;
	public bool isFollowing;

	private void Start(){
		this.img = GetComponent<Image>();
		this.rectTransform = GetComponent<RectTransform>();
	}

	public void followMouse(Sprite sprite){
		if(img.enabled == true) 
			return;
		isFollowing = true;
		img.enabled = true;
		img.sprite = sprite;
	}

	public void stopFollowMouse(){
		this.img.enabled = false;
		this.img.sprite = null;
		isFollowing = false;
	}

	void Update(){
		if(this.img.enabled == true){
			Vector3 pos = this.transform.position;
			Vector3 point = RectTransformUtility.WorldToScreenPoint(Camera.main, pos);
			this.rectTransform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		}
	}

}
