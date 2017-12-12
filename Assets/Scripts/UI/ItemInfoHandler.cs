using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum e_itemMethods
{
    EQUIP = 0, USE, DROP
}
public class ItemInfoHandler : MonoBehaviour {
    private Equip equip;
    private Item item;
    private Player player;
    private InventorySlot slot;
    private MouseOverUI mouse;
    private ItemInfoButton[] buttons;
    // Use this for initialization
    void Start() {
        mouse = this.GetComponent<MouseOverUI>();
        buttons = new ItemInfoButton[this.transform.childCount];
        for (int i = 0; i < buttons.Length; i++) {
            buttons[i] = this.transform.GetChild(i).GetComponent<ItemInfoButton>();
        }
    }
    public void setButton(int button, string message, e_itemMethods method){
        buttons[button].changeButtonClick(message,(int)method);
    }
    public bool isMouseOver() {
        return mouse.isMouseOver();
    }
    public void setEquip(InventorySlot equip) {
        slot = equip;
        this.equip = (Equip)slot.getItem();
    }
    public void setItem(InventorySlot slot) {
        Debug.Log("item here!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! " + slot.getItem());
        this.slot = slot;
        item = slot.getItem();
    }
    public void setPlayer(Player player) {
        this.player = player;
    }
    public Equip getItem() {
        return this.equip;
    }
    public void setUseClicked() {
        player.getNetwork().onItemUse(item);
        this.player.getInventory().hideItemActionMenu();
        //Debug.Log("item use clicked");
    }
    public void setEquipClicked() {
        destroyItemInInventory();
        player.getEquipHandler().equipItem(equip);
        this.player.getInventory().hideItemActionMenu();
    }
    private void destroyItemInInventory() {
        int index = player.getInventory().getItems().IndexOf(slot);
        Destroy(player.getInventory().getItems()[index].gameObject);
        player.getInventory().getItems().RemoveAt(index);
        player.getInventory().getItemInfoTransform().sizeDelta = new Vector2(player.getInventory().getItemInfoTransform().sizeDelta.x, 0f);
        player.getInventory().mouseOver = 0;
    }
    public void setDropClicked() {
    
    }
	// Update is called once per frame
	void Update () {
		
	}
}
