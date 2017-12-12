using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Networking;
using MySql.Data.MySqlClient;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;
using System.IO;
using System.Linq;
using System;
using System.Text;
public class MouseInventory {
	public int holdingID = -1;
	public Item whereItemWereTaken;

    public bool isEmpty() {
        return (whereItemWereTaken == null);
    }
    public Item getItem() {
        return whereItemWereTaken;
    }
	public int getItemID(){
		return whereItemWereTaken.getID();
	}
	public void empty(){
		holdingID = -1;
		whereItemWereTaken = null;
	}
	public int getItemPosition(){
		return holdingID;
	}
	public void setMouseItem(Item i,int pos) {
        whereItemWereTaken = i;
		holdingID = pos;
    }
}
public static class stringTools {
    public static UnityEngine.Object[][] spriteObjects = new UnityEngine.Object[][] {
        Resources.LoadAll("use"),
        Resources.LoadAll("weapons"),
        Resources.LoadAll("weapons"),
        Resources.LoadAll("weapons"),
        Resources.LoadAll("weapons"),
        Resources.LoadAll("weapons"),
        Resources.LoadAll("weapons"),
        Resources.LoadAll("weapons"),
        Resources.LoadAll("weapons"),
    };
}
public class Inventory : MonoBehaviour
{

    private bool shouldUpdateInventory = true;
    public const int MAX_INVENTORY_SIZE = 16;
    private readonly int EMPTY = -1;
    private int ItemAmounts = 0;
	MouseInventory mouse = new MouseInventory();
    public Player player;
    public inventoryInformation informationPrefab;
    private inventoryInformation information;
    List<InventorySlot> itemsOwned = new List<InventorySlot>();
    //List<int> inv = new List<int>();
    Text[] itemIDString = new Text[MAX_INVENTORY_SIZE];
    public GameObject InventorySlot;
    public GameObject canvasPrefab;
    private UIHandler handler;
    private GameObject[] canvas = new GameObject[4];
    bool isShowing = false;
    int[] items;

    private GameObject parentCanvas;
    public GameObject itemSettings;

    private RectTransform itemSettingTransform;
    private InventorySlot slotClicked;
    private ItemInfoHandler itemInfoHandler;
    private bool hasRightClicked = false;
    private bool isDoneLoading = false;
    public int itemRightClickSpeed;

    public static int activeCanvas = 0;
    public void show()
    {
        isShowing = !isShowing;
        if (isShowing)
        {
            activate();
        }
        else
        {
            deactivate();
        }
    }
    /*
    public void JsonToDirectory(string json) {
        jsonRead read = JsonUtility.FromJson<jsonRead>(json);
        for (int i = 0; i < invSlot.Length; i++)
        {
            if (invSlot[i].getItem().stats[0] != EMPTY)
            {
                ItemString.itemNames.Add(invSlot[i].getItem().stats[0], read.Items[invSlot[i].getItem().stats[0] - 1000]);
            }
        }
    }
    */
    public List<InventorySlot> getItems() {
        return this.itemsOwned;
    }
    public int[] itemsToArray(Item[] items)
    {
        int[] itemStats = new int[items.Length * Tools.ITEM_PROPERTY_SIZE];
        for (int i = 0; i < items.Length; i += items[i].getStats().Length)
        {
            for (int j = 0; j < items[i].getStats().Length; j++)
            {
                itemStats[i + j] = items[i].getStats()[j];
            }
        }
        return itemStats;
    }
    void activate()
    {
        parentCanvas.SetActive(true);
    }
    void deactivate()
    {
        parentCanvas.SetActive(false);
        information.gameObject.SetActive(false);

    }
    public void removeItem(Item item) {
        itemsOwned.RemoveAt(findItem(item));
    }
    public void updateItem(Item oldItem, Item newItem) {
        Debug.Log("item: " + findItem(oldItem));
        itemsOwned[findItem(oldItem)].setItem(newItem.getPosition(), newItem);
    }
    public int findItem(Item item) {
        for (int i = 0; i < itemsOwned.Count; i++)
        {
            if (itemsOwned[i].getItem().compareTo(item))
            {
                return i;
            }
        }
        return -1;
    }
	public void init(Player player)
    {
		this.player = player;
        itemSettingTransform = Instantiate(itemSettings).GetComponent<RectTransform>();
        itemInfoHandler = itemSettingTransform.gameObject.GetComponent<ItemInfoHandler>();
        itemInfoHandler.setPlayer(this.player);
        parentCanvas = Tools.getChild(player.getUI(), "Inventory_UI");
        itemSettingTransform.SetParent(parentCanvas.transform);
        canvas[0] = Tools.getChild(player.getUI(), "Items_Inventory_Eqp");
        canvas[1] = Tools.getChild(player.getUI(), "Items_Inventory_Use");
        canvas[2] = Tools.getChild(player.getUI(), "Items_Inventory_Etc");
        canvas[3] = Tools.getChild(player.getUI(), "Items_Inventory_Quest");
        GameObject g = (GameObject)(Instantiate(informationPrefab.gameObject, Vector3.zero, Quaternion.identity));
        information = g.GetComponent<inventoryInformation>();
        information.gameObject.transform.SetParent(canvas[0].transform.parent.parent.parent);
        information.gameObject.transform.SetAsLastSibling();
        handler = parentCanvas.GetComponent<UIHandler>();
        //addItem(new Item().getEmptyItem(0));
    }
    public int getClosestSlot(int storeType) {
        bool isEmpty = true;
        for (int i = 0; i < MAX_INVENTORY_SIZE; i++) {
            isEmpty = true;
            for (int j = 0; j < itemsOwned.Count; j++) {
                if (itemsOwned[j].getItem().getInventoryType() == storeType && itemsOwned[j].getItem().getPosition() == i) {
                    isEmpty = false;
                    break;
                }
            }
            if (isEmpty) {
                Debug.Log("Emty::::::::: " + i);
                return i;
            }
        }
        return -1;
    }
    public bool addItem(Item item)
    {
        GameObject instansiatedSlot = (GameObject)Instantiate(InventorySlot);
        InventorySlot slot = instansiatedSlot.GetComponent<InventorySlot>();
        slot.setID(itemsOwned.Count);
        slot.setItem(item.getPosition(), item);
        slot.setImage(slot.getItem());
        slot.transform.SetParent(canvas[item.getInventoryType()].transform);
        itemsOwned.Add(slot);
        recalcPos(itemsOwned.Count - 1, item.getPosition());
        return true;
    }
    public inventoryInformation getItemInformationObject() {
        return this.information;
    }
    public void hideItemActionMenu() {
        hasRightClicked = false;
        isDoneLoading = true;
        itemSettingTransform.sizeDelta = new Vector2(itemSettingTransform.sizeDelta.x, 0F);
    }
    public int mouseOver = 0;
    void updateInventory()
    {
        if (!isShowing)
            return;

        for (int i = 0; i < itemsOwned.Count; i++)
        {
            if (itemsOwned[i].isMouseOver() && !information.gameObject.activeSelf)
            {
                information.setItem(itemsOwned[i].getItem());
                information.gameObject.SetActive(true);
                information.show(itemsOwned[i].getMouse());
                information.gameObject.transform.SetAsLastSibling();
                mouseOver = i;
            }
            else if (!itemsOwned[mouseOver].isMouseOver())
            {
                if (information.gameObject.activeSelf)
                {     
                    information.hide();
                    information.gameObject.SetActive(false);
                }
            }
            if (itemsOwned[i].isMouseOver() && mouse.isEmpty())
            {
                itemsOwned[i].gameObject.GetComponent<Image>().color = Color.gray;
            }
            else if (mouse.isEmpty())
            {
                itemsOwned[i].gameObject.GetComponent<Image>().color = new Color(0xF2, 0xF2, 0xF2, 0xFF);
            }
            if (itemsOwned[i].isMouseOver() && Input.GetMouseButtonDown(0))
            {
                if (mouse.isEmpty())
                {
                    mouse.setMouseItem(itemsOwned[i].getItem(), i);
                    itemsOwned[i].gameObject.GetComponent<Image>().color = new Color(0.9f, 0.9f, 0.9f, 1);
                    break;
                }
            }
            if (!mouse.isEmpty() && Input.GetMouseButtonDown(0) && canvas[activeCanvas].GetComponent<MouseOverUI>().isMouseOver())
            {
                Item temp;
                int newPos = Vec2ToSlots(canvas[activeCanvas].GetComponent<MouseOverUI>().getRelativePosition());
                int pos;
                if ((pos = getSlot(newPos)) != EMPTY)
                {
                    temp = itemsOwned[pos].getItem();
                    itemsOwned[pos].getItem().setPosition(itemsOwned[mouse.holdingID].getItem().getPosition());
                    recalcPos(pos, itemsOwned[mouse.holdingID].getItem().getPosition());
                }
                else
                {
                    temp = Item.getEmptyItem(newPos);
                }
                itemsOwned[mouse.holdingID].getItem().setPosition(newPos);
                recalcPos(mouse.holdingID, newPos);

                player.getNetwork().moveItem(mouse.getItem(), temp, PacketTypes.INVENTORY_MOVE_ITEM, this.player);
                mouse.empty();
            }

            if (Input.GetMouseButtonDown(1) && itemsOwned[i].isMouseOver())
            {
                if (slotClicked == itemsOwned[i])
                    hasRightClicked = !hasRightClicked;
                else 
                    hasRightClicked = true;

                isDoneLoading = false;
                itemSettingTransform.sizeDelta = new Vector2(itemSettingTransform.sizeDelta.x, 0f);
                itemSettingTransform.position = Input.mousePosition;
                itemSettingTransform.gameObject.SetActive(true);
                information.hide();
                slotClicked = itemsOwned[i];
                if (itemsOwned[i].getItem().getInventoryType() == (int)inventoryTabs.EQUIP)
                {
                    itemInfoHandler.setEquip(itemsOwned[i]);
                    itemInfoHandler.setButton(0,"Equip",e_itemMethods.EQUIP);
                }
                else {
                    itemInfoHandler.setItem(itemsOwned[i]);
                    itemInfoHandler.setButton(0, "Use", e_itemMethods.USE);
                }
            }

            if (!itemInfoHandler.isMouseOver() && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && hasRightClicked && isDoneLoading) {
                hasRightClicked = false;
                isDoneLoading = true;
                itemSettingTransform.sizeDelta = new Vector2(itemSettingTransform.sizeDelta.x, 0F);
            }
            /*
            else if (!mouse.isEmpty() && Input.GetMouseButtonDown(0) && !canvas[activeCanvas].GetComponent<MouseOverUI>().isMouseOver())
            {
                itemsOwned.RemoveAt(mouse.holdingID);
            }
            */
        }
        if (hasRightClicked && !isDoneLoading) {

            itemSettingTransform.sizeDelta = new Vector2(itemSettingTransform.sizeDelta.x, Mathf.Lerp(itemSettingTransform.sizeDelta.y,90f,Time.deltaTime * itemRightClickSpeed));
            if (itemSettingTransform.sizeDelta.y >= 89f) {
                itemSettingTransform.sizeDelta = new Vector2(itemSettingTransform.sizeDelta.x, 90F);
                isDoneLoading = true;
            }
        }
        if (itemSettings)
            shouldUpdateInventory = false;
    }
    void Update()
    {
        //if (shouldUpdateInventory)
        updateInventory();
    }
    int getSlot(int slot)
    {
        for (int i = 0; i < itemsOwned.Count; i++)
        {
            if (itemsOwned[i].getItem().getPosition() == slot && itemsOwned[i].getItem().getInventoryType() == activeCanvas)
                return i;
        }
        return -1;
    }
    void recalcPos(int index, int newPos)
    {
        itemsOwned[index].gameObject.GetComponent<RectTransform>().localPosition = new Vector3((newPos % 4) * 50, -25 - Mathf.Floor(newPos / 4) * 50, 0);
    }
    public void clearInventory() {
        foreach (InventorySlot slot in itemsOwned) {
            Destroy(slot.gameObject);
        }
        itemsOwned.Clear();
    }
    public void setInventory(List<Item> items)
    {
        Item item;
        for(int i = 0; i < items.Count; i++)
        {
            item = items[i];

            GameObject instansiatedSlot = (GameObject)Instantiate(InventorySlot);
            InventorySlot slot = instansiatedSlot.GetComponent<InventorySlot>();
            slot.setID(i);
            slot.setItem(item.getPosition(), item);
            slot.setImage(slot.getItem());
            //itemIDString[i] = instansiatedSlot.GetComponentInChildren<Text> ();

            slot.gameObject.transform.SetParent(canvas[item.getInventoryType()].transform);
            //invSlot[i].gameObject.GetComponent<RectTransform>().localPosition = new Vector3((invSlot[i].getItem().getStats()[8] % i) * 50, -25 - Mathf.Floor(invSlot[i].getItem().getStats()[8] / 4) * 50, 0);
            slot.gameObject.GetComponent<RectTransform>().localPosition = new Vector3((item.getPosition() % 4) * 50, -25 - Mathf.Floor(item.getPosition() / 4) * 50, 0);
            itemsOwned.Add(slot);
        }
        Debug.Log("item array size: " + itemsOwned.Count); 
    }

    private int Vec2ToSlots(Vector2 vec2)
    {
        return (int)(Mathf.Floor(vec2.x / 50) + Mathf.Floor(vec2.y / 50) * 4);
    }
    public RectTransform getItemInfoTransform() {
        return itemSettingTransform;
    }
}