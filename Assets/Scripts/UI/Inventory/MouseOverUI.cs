﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseOverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	private bool mouseOver;
	private bool inChild;
    private float lastClick = 0;
	public void OnPointerEnter(PointerEventData eventData){

		if(eventData.pointerCurrentRaycast.gameObject != null){
			mouseOver = true;
		}
	}
	void Update () {
		/*if(transform.parent.GetComponent<MouseOverUI>() != null){
			if(mouseOver){
				transform.parent.GetComponent<MouseOverUI>().inChild = true;
			} else {
				transform.parent.GetComponent<MouseOverUI>().inChild = false;
			}
		}*/

		//transform.Find ("InventoryInformation").gameObject.SetActive (true);
		/*
		if (mouseOver || Input.GetKey(KeyCode.C)) {
			transform.Find ("InventoryInformation").gameObject.SetActive (true);
		} else {
			transform.Find ("InventoryInformation").gameObject.SetActive (false);
		}*/
	}
    public bool hasDoubleClicked()
    {
        if (Time.time - lastClick < 0.2f)
        {
            return true;
        }
        lastClick = Time.time;
        Debug.Log("lastclick: " + (Time.time - lastClick));
        return false;
    }
    public void OnPointerExit(PointerEventData eventData){
		mouseOver = false;
	}
	public bool isMouseOver(){
		return mouseOver;
	}
    public Vector2 position() {
        return new Vector2(Input.mousePosition.x, Input.mousePosition.y);
    }
    public Vector2 getRelativePosition() {
        return new Vector2(Input.mousePosition.x - this.GetComponent<RectTransform>().position.x, Mathf.Abs(Input.mousePosition.y - this.GetComponent<RectTransform>().position.y + 25));
    }
}
