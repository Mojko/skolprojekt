using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInfoHandler : MonoBehaviour {
    private Equip equip;
    private Player player;
    private InventorySlot equipSlot;
    // Use this for initialization
    void Start() {

    }
    public void setEquip(InventorySlot equip) {
        equipSlot = equip;
        this.equip = (Equip)equipSlot.getItem();
    }
    public void setPlayer(Player player) {
        this.player = player;
    }
    public Equip getItem() {
        return this.equip;
    }
    public void setEquipClicked() {
        int index = player.getInventory().getItems().IndexOf(equipSlot);
        Destroy(player.getInventory().getItems()[index].gameObject);
        player.getInventory().getItems().RemoveAt(index);
        player.getInventory().getItemInfoTransform().sizeDelta = new Vector2(player.getInventory().getItemInfoTransform().sizeDelta.x, 0f);
        player.getInventory().mouseOver = 0;
        player.getEquipHandler().setEquip(equip.getID(), equip);
    }
    public void setDropClicked() {
    
    }
	// Update is called once per frame
	void Update () {
		
	}
}
