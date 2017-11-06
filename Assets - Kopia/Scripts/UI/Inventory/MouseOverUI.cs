using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseOverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	private bool mouseOver;
	private bool inChild;
    
	public void OnPointerEnter(PointerEventData eventData){

		if(eventData.pointerCurrentRaycast.gameObject != null){
			mouseOver = true;
		}
	}
	void Update () {

        if(transform.parent.GetComponent<MouseOverUI>() != null) {
            if (!mouseOver) {
                transform.parent.GetComponent<MouseOverUI>().inChild = false;
            } else {
                transform.parent.GetComponent<MouseOverUI>().inChild = true;
            }
        }


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

	public void OnPointerExit(PointerEventData eventData){
		mouseOver = false;
	}

	public bool isMouseOver(){
		return mouseOver && !inChild;
	}
    public Vector2 position() {
        return new Vector2(Input.mousePosition.x, Input.mousePosition.y);
    }
    public Vector2 getRelativePosition() {
        return new Vector2(Input.mousePosition.x - this.GetComponent<RectTransform>().position.x, Mathf.Abs(Input.mousePosition.y - this.GetComponent<RectTransform>().position.y + 25));
    }
}
