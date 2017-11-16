using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class EquipmentHandler : UIHandler {
    List<Equip> equips = new List<Equip>();
    GameObject equip;
    GameObject[] slots;
    MouseOverUI[] slotsMouse;
    Player player;
    bool hasLoaded = false;
    new void Update() {
        base.Update();
        if (!hasLoaded) return;
        for (int i = 0; i < slotsMouse.Length; i++)
        {
            if (slotsMouse[i].isMouseOver() && Input.GetMouseButtonDown(0) && equips[i] != null)
            {
                Debug.Log("hovering equip slot: " + i);
                onClick(i, equips[i]);
            }
        }
    }
    public void setPlayer(Player player) {
        this.player = player;
    }
    public void setEquips(List<Equip> eqps) {
        this.equips = eqps;
    }
    private Equip isSlotEmpty(int slot) {
        for (int i = 0; i < equips.Count; i++) {
            if (((equips[i].getID() / Tools.ITEM_INTERVAL) - 2) == slot)
                return equips[i];
        }
        return null;
    }
    public void setEquip(int type, Equip equip) {
        Debug.Log("EQUIPS: " + equips.Count);
        int index = (equip.getID() / Tools.ITEM_INTERVAL) - 2;
        Equip item;
        if ((item = equips[index]) != null) {
            item.setPosition(equip.getPosition());
            player.getInventory().addItem(item);
        }
        equips[index] = equip;
        player.getNetwork().equipItem(equip);
        updateSlots();
    }
    public void setEquipmentUI(GameObject equipment) {
        equip = equipment;
        slots = equip.transform.GetChild(0).getAllChildren().transformsToObject();
        slotsMouse = new MouseOverUI[slots.Length];
        for (int i = 0; i < slots.Length; i++)
        {
            slotsMouse[i] = slots[i].GetComponent<MouseOverUI>();
        }
        hasLoaded = true;
    }
    public void onClick(int pos, Equip equip) {
        Debug.Log("EQUIP CLICKED");
        Inventory inventory = player.getInventory();
        int closestFree = inventory.getClosestSlot((int)inventoryTabs.EQUIP);
        equip.setPosition(closestFree);
        Debug.Log("slot: " + closestFree);
        inventory.addItem(equip);
        clearSlot(pos);
        this.equips[pos] = null;
        updateSlots();
        this.player.getNetwork().unEquipItem(equip);
    }
    private void clearSlot(int slot) {
        Image image = slots[slot].transform.GetChild(0).GetComponent<Image>();
        image.sprite = null;
        image.color = new Color(0, 0, 0, 0);
    }
    public void updateSlots() {
        int countSize = 0;
        GameObject slot;
        Equip equip;
        for (int i = 0; i < equips.Count; i++) {
            if (equips[i] == null) continue;
            equip = equips[i];
            Debug.Log("equip id position: " + ((equip.getID() / 500) - 2));
            Image image = slots[(equip.getID() / Tools.ITEM_INTERVAL) - 2].transform.GetChild(0).GetComponent<Image>();
            Debug.Log("sprite: " + equip.getID() / Tools.ITEM_INTERVAL + " : " + equip.getID() % Tools.ITEM_INTERVAL + 1);
            image.sprite = (Sprite)stringTools.spriteObjects[equip.getID() / Tools.ITEM_INTERVAL][equip.getID() % Tools.ITEM_INTERVAL + 1];
            image.color = Color.white;
        }
    }
    public void clear() {
        equips.Clear();
    }
}
