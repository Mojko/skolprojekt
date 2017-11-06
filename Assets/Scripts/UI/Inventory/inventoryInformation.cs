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
        this.transform.GetChild(1).gameObject.GetComponent<Text>().text =
        "Stats \n Watt: " + item.getDamage() + " \n Matt: " + item.getMagicAttack() + " \n Luk: " + item.getLuk() + "";
        //this.gameObject.SetActive(true);
        isShowing = true;
        this.transform.position = new Vector3(this.mouse.position().x, this.mouse.position().y, 0f);
    }
	void Update () {
        if (isShowing) {
            this.transform.position = new Vector3(mouse.position().x, mouse.position().y, 0f);
        }
	}
}
