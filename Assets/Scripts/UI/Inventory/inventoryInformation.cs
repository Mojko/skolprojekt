using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class inventoryInformation : UI {
    public Item item;
    bool isShowing = false;
    MouseOverUI mouse;
    Text statsText, description, name;
	void Start () {
        name = this.transform.GetChild(0).gameObject.GetComponent<Text>();
        description = this.transform.GetChild(1).gameObject.GetComponent<Text>();
        statsText = this.transform.GetChild(2).gameObject.GetComponent<Text>();
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
        //this.transform.GetChild(1).gameObject.GetComponent<Text>().text =
        //"Stats \n Watt: " + item.getDamage() + " \n Matt: " + item.getMagicAttack() + " \n Luk: " + item.getLuk() + "";
        InventoryInformationString data = displayInfo();
        name.text = data.getName();
        description.text = data.getDescription();
        statsText.text = data.getString();
        //this.gameObject.SetActive(true);
        isShowing = true;
        this.transform.position = new Vector3(this.mouse.position().x, this.mouse.position().y, 0f);
    }
    private InventoryInformationString displayInfo() {
        int itemID = item.getID();
        ItemVariables info = ItemDataProvider.getInstance().getStats(item.getID());
        InventoryInformationString data = new InventoryInformationString();
        data.setName(info.getString("name"));
        data.setDescription(info.getString("description"));
        if (itemID.isItemType(e_itemTypes.EQUIP))
        {
            data.setString("Stats \n Watt: " + item.getDamage() + " \n Matt: " + item.getMagicAttack() + " \n Luk: " + item.getLuk() + "");
        }
        else
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
    private string str = "Stats \n";
    private string description = "";
    private string name;
    public string getDescription() {
        return description;
    }
    public string getName() {
        return this.name;
    }
    public void setName(string name) {
        this.name = name;
    }
    public void setDescription(string description) {
        this.description = description;
    }
    public void setString(string str) {
        this.str = str;
    }
    public void addInformation(ItemVariables data) {
        str = "";
        foreach (KeyValuePair<string, string> item in data.getStrings())
        {
            if(data.shouldShow(item.Key))
                str += item.Key + ": " + item.Value + " \n";
        }
        foreach (KeyValuePair<string, float> item in data.getFloats())
        {
            if (data.shouldShow(item.Key))
                str += item.Key + ": " + item.Value + " \n";
        }
        foreach (KeyValuePair<string, int> item in data.getInts())
        {
            if (data.shouldShow(item.Key))
                str += item.Key + ": " + item.Value + " \n";
        }
    }
    public string getString() {
        return str;
    }
}
