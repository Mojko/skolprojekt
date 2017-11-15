using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInfoHandler : MonoBehaviour {
    private Equip equip;
    private Item item;
    private Player player;
    private InventorySlot slot;
    // Use this for initialization
    void Start() {

    }
    public void setEquip(InventorySlot equip) {
        slot = equip;
        this.equip = (Equip)slot.getItem();
    }
    public void setItem(InventorySlot slot) {
        this.slot = slot;
        this.equip = (Equip)slot.getItem();
    }
    public void setPlayer(Player player) {
        this.player = player;
    }
    public Equip getItem() {
        return this.equip;
    }
    public void setUseClicked() {
        player.getNetwork().onItemUse(item);
    }
    public void setEquipClicked() {
        destroyItemInInventory();
        player.getEquipHandler().setEquip(equip.getID(), equip);
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
