using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseOverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	private bool mouseOver;
    private bool isParent;
    
	public void OnPointerEnter(PointerEventData eventData){
		if(eventData.pointerCurrentRaycast.gameObject != null){
			mouseOver = true;
            if(Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)){
                if(transform.childCount > 0) {
                    isParent = true;
                }
            }
		}
	}
	void Update () {
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
        isParent = false;
	}

	public bool isMouseOver(){
		return mouseOver && !isParent;
	}
    public Vector2 position() {
        return new Vector2(Input.mousePosition.x, Input.mousePosition.y);
    }
    public Vector2 getRelativePosition() {
        return new Vector2(Input.mousePosition.x - this.GetComponent<RectTransform>().position.x, Mathf.Abs(Input.mousePosition.y - this.GetComponent<RectTransform>().position.y + 25));
    }
}
