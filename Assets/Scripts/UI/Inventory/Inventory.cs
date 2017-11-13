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
public class Mouse {
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

public enum ItemIDs {
	SWORD = 9000,
	BOW = 9100,
	STAFF = 9200,
	WAND = 9300
}
public static class stringTools {
    public static string[] Items = new string[]{
        "Stick",
        "Fyring pan",
        "Wand"
    };
    public static UnityEngine.Object[][] spriteObjects = new UnityEngine.Object[][] {
        Resources.LoadAll("use"),
        new UnityEngine.Object[0],
        new UnityEngine.Object[0],
        new UnityEngine.Object[0],
        new UnityEngine.Object[0],
        Resources.LoadAll("weapons"),
        new UnityEngine.Object[0],
        new UnityEngine.Object[0],
        Resources.LoadAll("weapons"),
    };
}
[System.Serializable]
public class jsonRead{
    public string[] Items = new string[]{
        "Stick",
        "Fyring pan",
        "Wand"
    };
}
[System.Serializable]
public class ItemString{
    public static Dictionary<int, string> itemNames = new Dictionary<int, string> {
        { 0, "Mana" },
        { 1, "Hp" },
        { 2501, "Stick" },
        { 2500, "Pan" },
    };
}
public class Inventory : MonoBehaviour
{

    private bool shouldUpdateInventory = true;
    public const int MAX_INVENTORY_SIZE = 16;
    private readonly int EMPTY = -1;
    private int ItemAmounts = 0;
    Mouse mouse = new Mouse();
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
    public Item[] getItems() {
        Item[] item = new Item[itemsOwned.Count];
        for(int i= 0; i < itemsOwned.Count; i++) {
            item[i] = itemsOwned[i].getItem();
        }
        return item;
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
        this.player.getNetwork().loadInventory();
        //addItem(new Item().getEmptyItem(0));
    }
    public int getClosestSlot(int storeType) {
        bool isEmpty = true;
        for (int i = 0; i < MAX_INVENTORY_SIZE; i++) {
            isEmpty = true;
            for (int j = 0; j < itemsOwned.Count; j++) {
                if (itemsOwned[j].getItem().getType() == storeType && itemsOwned[j].getItem().getPosition() == i) {
                    isEmpty = false;
                    break;
                }
            }
            if (isEmpty) {
                return i;
            }
        }
        return -1;
    }
    public bool addItem(Item item)
    {
        InventorySlot slot = new InventorySlot();
        slot.setItem(item.getPosition(), item);
        Debug.Log("item added " + item);
        GameObject instansiatedSlot = (GameObject)Instantiate(InventorySlot);
        slot = instansiatedSlot.GetComponent<InventorySlot>();
        slot.setID(itemsOwned.Count);
        slot.setItem(item.getPosition(), item);
        slot.setImage(slot.getItem());
        slot.transform.SetParent(canvas[activeCanvas].transform);
        itemsOwned.Add(slot);
        recalcPos(itemsOwned.Count - 1, item.getPosition());
        player.getNetwork().moveItem(item,Item.getEmptyItem(-1), PacketTypes.INVENTORY_MOVE_ITEM, this.player);
        return true;
    }

    int mouseOver = 0;
    void updateInventory()
    {
        if (!isShowing)
            return;
        int index = 0;
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
                index = 0;
                if (mouse.isEmpty())
                {
                    mouse.setMouseItem(itemsOwned[i].getItem(), i);
                    itemsOwned[i].gameObject.GetComponent<Image>().color = new Color(0.9f, 0.9f, 0.9f, 1);
                    break;
                }
            }
            //Debug.Log(activeCanvas);
            if(Input.GetMouseButtonDown(0) && !mouse.isEmpty())
            {
                Debug.Log("is over?: " + canvas[activeCanvas]);
            }
            if (!mouse.isEmpty() && Input.GetMouseButtonDown(0) && canvas[activeCanvas].GetComponent<MouseOverUI>().isMouseOver())
            {
                Item temp;
                int newPos = Vec2ToSlots(canvas[activeCanvas].GetComponent<MouseOverUI>().getRelativePosition());
                int pos;
                Debug.Log("clicked");
                if ((pos = getSlot(newPos)) != EMPTY)
                {
                    Debug.Log("spot not empty");
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
                Debug.Log("right click! " + hasRightClicked);
                information.hide();
                slotClicked = itemsOwned[i];
                itemInfoHandler.setEquip((Equip)itemsOwned[i].getItem());
            }
            /*
            else if (!mouse.isEmpty() && Input.GetMouseButtonDown(0) && !canvas[activeCanvas].GetComponent<MouseOverUI>().isMouseOver())
            {
                itemsOwned.RemoveAt(mouse.holdingID);
            }
            */
        }
        if (hasRightClicked && !isDoneLoading) {
            Debug.Log("lerping!!");
            itemSettingTransform.sizeDelta = new Vector2(itemSettingTransform.sizeDelta.x, Mathf.Lerp(itemSettingTransform.sizeDelta.y,90f,Time.deltaTime * itemRightClickSpeed));
            if (itemSettingTransform.sizeDelta.y >= 89f) {
                itemSettingTransform.sizeDelta = new Vector2(itemSettingTransform.sizeDelta.x, 90F);
                isDoneLoading = true;
            }
        }
        shouldUpdateInventory = false;
    }
    void Update()
    {
        //if (shouldUpdateInventory)
        updateInventory();
    }
    bool isItemInSameTab(int item) {
        return false;
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
            InventorySlot slot = new InventorySlot();
            slot = instansiatedSlot.GetComponent<InventorySlot>();
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
}