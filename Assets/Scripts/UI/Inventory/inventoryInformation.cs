using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
enum game {

}
public class inventoryInformation : UI {
    public Item item;
    bool isShowing = false;
    MouseOverUI mouse;
	void Start () {
		
	}
    public void setItem(Item item) {
        this.item = item;
    }
    public void hide() {
        //this.gameObject.SetActive(false);
        isShowing = false;
    }
    public void show(MouseOverUI mouse) {
        this.mouse = mouse;
        this.transform.GetChild(0).gameObject.GetComponent<Text>().text = item.getName();
        //this.transform.GetChild(1).gameObject.GetComponent<Text>().text =
        //"Stats \n Watt: " + item.getDamage() + " \n Matt: " + item.getMagicAttack() + " \n Luk: " + item.getLuk() + "";
        this.transform.GetChild(1).gameObject.GetComponent<Text>().text = displayInfo().getString();
        //this.gameObject.SetActive(true);
        isShowing = true;
        this.transform.position = new Vector3(this.mouse.position().x, this.mouse.position().y, 0f);
    }
    private InventoryInformationString displayInfo() {
        int itemID = item.getID();
        ItemVariables info = ItemDataProvider.getInstance().getStats(item.getID());
        InventoryInformationString data = new InventoryInformationString();
        data.setDescription(info.getString("description"));
        if (itemID.isItemType(e_itemTypes.HATS) || itemID.isItemType(e_itemTypes.PANTS) || itemID.isItemType(e_itemTypes.BODY) || itemID.isItemType(e_itemTypes.BOOTS) || itemID.isItemType(e_itemTypes.WEAPON) || itemID.isItemType(e_itemTypes.GLOVE) || itemID.isItemType(e_itemTypes.FACE) || itemID.isItemType(e_itemTypes.ACCESSORY))
        {
            data.addInformation(info);
        }
        return data;
    }
	void Update () {
        if (isShowing) {
            this.transform.position = new Vector3(mouse.position().x, mouse.position().y, 0f);
        }
	}
}
public class InventoryInformationString{
    private string str = "Stats \n ";
    private string description = "";
    public string getDescription() {
        return description;
    }
    public void setDescription(string description) {
        this.description = description;
    }
    public void addInformation(ItemVariables data) {
        foreach (KeyValuePair<string, object> item in data.getDictionary())
        {
            str += item.Key + ": " +(string)item.Value + " \n ";
        }
    }
    public string getString() {
        return str;
    }
}
