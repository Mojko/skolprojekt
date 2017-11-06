using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class InventoryUIHandler : UIHandler {
    private List<InventoryTab> tabs = new List<InventoryTab>();
    public Inventory inventory;
    public void addTab(InventoryTab tab) {
        tabs.Add(tab);
    }
    public void hasClickedTab(InventoryTab tab) {
        for (int i = 0; i < tabs.Count; i++) {
            tabs[i].GetComponent<Image>().color = Tools.hexColor(0x6CB95D);
            Inventory.activeCanvas = i;
        }
        tabs[tabs.IndexOf(tab)].GetComponent<Image>().color = Tools.hexColor(0x53A543);
    }
}
