using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInfoHandler : MonoBehaviour {
    private Equip equip;
    private Player player;
    // Use this for initialization
    void Start() {

    }
    public void setEquip(Equip equip) {
        this.equip = equip;
    }
    public void setPlayer(Player player) {
        this.player = player;
    }
    public Equip getItem() {
        return this.equip;
    }
    public void setEquipClicked() {
        player.getEquipHandler().setEquip(equip.getID(), equip);
    }
    public void setDropClicked() {
    
    }
	// Update is called once per frame
	void Update () {
		
	}
}
