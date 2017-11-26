﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class InventorySlot : MonoBehaviour {

	public int ID;
    public int itemID;
	public Item item;
    public Text quantityText;
    MouseOverUI mouse;
    public void Start() {
        mouse = GetComponent<MouseOverUI>();
    }
    public bool isMouseOver(){
		return mouse.isMouseOver();
	}
    public MouseOverUI getMouse() {
        return mouse;
    }
	public int getID(){
		return this.ID;
	}
	/*public int getItemID(){
		return this.itemInSlot.getID();
	}*/
	public void setItem(int position, Item item){
        this.item = item;
        this.item.stats[8] = position;
        quantityText.text = item.getQuantity() + "";

    }

    public void setImage(Item item) {
        //om itemet är tomt så ska det vara en tom bild. annars ska en hämta en bild beroende på vilket item det är.
        if (this.item.stats[0] == -1)
        {
            this.gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = null;
        }
        else {
			this.gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = item.getID().getSprite();
        }
    }

	public void setID(int ID){
		this.ID = ID;
	}
    public int getItemID() {
		return this.item.getID();
    }
	public Item getItem(){
		return this.item;
	}
}
