using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInfoButton : MonoBehaviour {
    private ItemInfoHandler handler;
	// Use this for initialization
	void Start () {
        handler = this.transform.parent.gameObject.GetComponent<ItemInfoHandler>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public void onEquip() {
        handler.setEquipClicked();
    }
    public void onDrop() {

    }
}
