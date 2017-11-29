using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class InventoryUIHandler : UIHandler
{
    //private List<InventoryTab> tabs = new List<InventoryTab>();
    private InventoryTab[] tabs;
    public GameObject tabsGameObjects;
    //public Inventory inventory;
    public void Start()
    {
        base.Start();
        tabs = new InventoryTab[tabsGameObjects.transform.childCount];
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i] = tabsGameObjects.transform.GetChild(i).gameObject.GetComponent<InventoryTab>();
        }
        Debug.Log("start size: " + tabs);
    }
    public void hasClickedTab(InventoryTab tab)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].GetComponent<Image>().color = Tools.hexColor(0x6CB95D);
        }
        tabs[tab.gameObject.transform.GetSiblingIndex()].GetComponent<Image>().color = Tools.hexColor(0x53A543);
        Inventory.activeCanvas = tab.gameObject.transform.GetSiblingIndex();
    }
}
