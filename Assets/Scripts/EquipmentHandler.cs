using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
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
                Debug.Log(i + " just got clicked!");
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
    public void equipItem(Equip equip) {
        if (equip.getID().isItemType(e_itemTypes.HATS) || equip.getID().isItemType(e_itemTypes.WEAPON))
            setEquip(equip);
        else
            setEquipCloth(equip);
    }
    public void setEquip(Equip equip) {
        int index = (equip.getID() / Tools.ITEM_INTERVAL) - 2;
        Equip item;
        foreach (Transform child in player.getEquipSlot(equip).transform.getAllChildren())
        {
            Destroy(child.gameObject);
        }
        if ((item = equips[index]) != null) {
            item.setPosition(equip.getPosition());
            player.getInventory().addItem(item);
        }

        equips[index] = equip;
        GameObject itemEquip = Instantiate(Resources.Load<GameObject>(ItemDataProvider.getInstance().getStats(equip.getID()).getString("pathToModel")));
        itemEquip.transform.SetParent(player.getEquipSlot(equip).transform);
        itemEquip.transform.localScale = Vector3.one;
        itemEquip.transform.localPosition = Vector3.zero;
        itemEquip.transform.localRotation = Quaternion.identity;
        player.getNetwork().equipItem(equip);
        updateSlots();
    }
    public void setEquipCloth(Equip equip) {
        int index = (equip.getID() / Tools.ITEM_INTERVAL) - 2;
        Equip item;
        //equips[index];
        GameObject itemEquip = Instantiate(Resources.Load<GameObject>(ItemDataProvider.getInstance().getStats(equip.getID()).getString("pathToModel")));
        GameObject slot = player.getSkinSlot(equip);
        slot.GetComponent<SkinnedMeshRenderer>().sharedMesh = itemEquip.GetComponent<SkinnedMeshRenderer>().sharedMesh;
        slot.GetComponent<SkinnedMeshRenderer>().sharedMaterial = itemEquip.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
        player.updateSkinSlot(equip, slot);
        Destroy(itemEquip);
        if ((item = equips[index]) != null)
        {
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
        Inventory inventory = player.getInventory();
        int closestFree = inventory.getClosestSlot((int)inventoryTabs.EQUIP);
        equip.setPosition(closestFree);
        inventory.addItem(equip);
        clearSlot(pos, equip);
        this.equips[pos] = null;
        updateSlots();
        this.player.getNetwork().unEquipItem(equip);
    }
    private void clearEquipSlot(Item item)
    {

        GameObject equipSlot = player.getEquipSlot(item);
        foreach (Transform child in equipSlot.transform.getAllChildren())
        {
            Destroy(child.gameObject);
        }
    }
    private void clearClothesSlot(Item item) {
        GameObject itemEquip = Instantiate(Resources.Load<GameObject>(Tools.getBasicModelPath(item)));
        GameObject slot = player.getSkinSlot(item);
        slot.GetComponent<SkinnedMeshRenderer>().sharedMesh = itemEquip.GetComponent<SkinnedMeshRenderer>().sharedMesh;
        slot.GetComponent<SkinnedMeshRenderer>().sharedMaterial = itemEquip.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
        player.updateSkinSlot(item, slot);
        Destroy(itemEquip);
    }
    private void clearSlot(int slot, Item item) {
        Image image = slots[slot].transform.GetChild(0).GetComponent<Image>();
        image.sprite = null;
        image.color = new Color(0, 0, 0, 0);
        if (item.getID().isItemType(e_itemTypes.HATS) || item.getID().isItemType(e_itemTypes.WEAPON))
            clearEquipSlot(item);
        else
            clearClothesSlot(item);
    }
    public void updateSlots() {
        int countSize = 0;
        GameObject slot;
        Equip equip;
        for (int i = 0; i < equips.Count; i++) {
            if (equips[i] == null) continue;
            equip = equips[i];
            Image image = slots[(equip.getID() / Tools.ITEM_INTERVAL) - 2].transform.GetChild(0).GetComponent<Image>();
            image.sprite = (Sprite)equip.getID().getSprite();
            image.color = Color.white;
            image.preserveAspect = true;
        }
    }
    public void clear() {
        equips.Clear();
    }
}
