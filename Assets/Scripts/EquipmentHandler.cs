using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class EquipmentHandler : UIHandler {
    List<int[]> equips = new List<int[]>();
    GameObject equip;
    GameObject[] slots;
    MouseOverUI[] slotsMouse;
    Player player;
    MouseOverUI mouse;
    public void Start() {
        base.Start();
        mouse = this.GetComponent<MouseOverUI>();
        for (int i = 0; i < slots.Length; i++) {
            slots[i].AddComponent<MouseOverUI>();
        }
    }
    public void Update() {
        base.Update();
        if (mouse.isMouseOver()) {

        }
    }
    public void setPlayer(Player player) {
        this.player = player;
    }
    public void setEquips(List<int[]> eqps) {
        equips = eqps;
    }
    public void setEquip(int type, int[] equip) {
        equips.Insert(type, equip);
    }
    public void setEquipmentUI(GameObject equipment) {
        equip = equipment;
        slots = equip.transform.GetChild(0).getAllChildren().transformsToObject();
    }
    public void onClick(int[] itemStats) {
        Inventory inventory = player.getInventory();
        int closestFree = inventory.getClosestSlot((int)inventoryTabs.EQUIP);
        itemStats[Tools.ITEM_PROPERTY_SIZE - 1] = closestFree;
        inventory.addItem(new Item(itemStats));
    }
    public void updateSlots() {
        Debug.Log("equipment: " + equips);
        int countSize = 0;
        for (int i = 0; i < equips.Count; i++) {
            if (equips[i] == null) continue;
            Debug.Log("position is not empty wewewewew: " + equips[i][Tools.ITEM_PROPERTY_SIZE - 1]);
            Image image = slots[Mathf.Abs(equips[i][Tools.ITEM_PROPERTY_SIZE - 1] + 1)].transform.GetChild(0).GetComponent<Image>();
            image.sprite = (Sprite)stringTools.sprites[equips[i][0] - 999];
            image.color = Color.white;
        }
    }
    public void clear() {
        equips.Clear();
    }
}
