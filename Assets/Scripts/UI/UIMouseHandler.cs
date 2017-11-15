using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMouseHandler : MonoBehaviour {
    List<UIHandler> uiElemnts = new List<UIHandler>();
	// Use this for initialization
	void Start () {
		
	}
	
    public void removeElement(UIHandler handler) {
        uiElemnts.Remove(handler);
    }

	// Update is called once per frame
	void Update () {
		
	}
    public void addElement(UIHandler handler) {
        uiElemnts.Add(handler);
    }

    public bool isUnderUI(UIHandler handler) {
        for (int i = 0; i < uiElemnts.Count; i++) {
            if (uiElemnts[i] == handler || !uiElemnts[i].gameObject.activeSelf) continue;
            //Debug.Log("item: " + uiElemnts[i].gameObject.name + "index: " + uiElemnts[i].gameObject.transform.GetSiblingIndex() + " current index: " + handler.gameObject.transform.GetSiblingIndex());
            if (uiElemnts[i].isClickingInsideUI() && uiElemnts[i].gameObject.transform.GetSiblingIndex() > handler.gameObject.transform.GetSiblingIndex()) {
                return true;
            }
            
        }
        return false;
    }
    public bool canClick(UIHandler handler) {
        return !isUnderUI(handler);
    }
}
