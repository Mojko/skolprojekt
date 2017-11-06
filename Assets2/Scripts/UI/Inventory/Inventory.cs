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
public static class stringTools{
    public static string[] Items = new string[]{
        "Stick",
        "Fyring pan",
        "Wand"
    };
    public static UnityEngine.Object[] sprites = Resources.LoadAll("weapons");
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
        { 1000, "Pan" },
        { 1001, "Stick" },
    };
}
public class Inventory : MonoBehaviour
{

    private bool shouldUpdateInventory = true;
    public const int MAX_INVENTORY_SIZE = 12;
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
    private GameObject[] canvas = new GameObject[4];
    bool isShowing = false;
    int[] items;

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

    private Item ArrayToItem(int i)
    {
        if (i * Tools.ITEM_PROPERTY_SIZE >= items.Length) return new Item(-1);
        int[] props = new int[Tools.ITEM_PROPERTY_SIZE];
        for (int a = 0; a < Tools.ITEM_PROPERTY_SIZE; a++) {
            props[a] = items[a + i * Tools.ITEM_PROPERTY_SIZE];
        }
        return new Item(props);
    }
    void activate()
    {
        canvas[0].transform.parent.parent.gameObject.SetActive(true);
    }
    void deactivate()
    {
        canvas[0].transform.parent.parent.gameObject.SetActive(false);
        information.gameObject.SetActive(false);

    }
	public void init(Player player)
    {
		this.player = player;
        canvas[0] = Tools.getChild(player.getUI(), "Items_Inventory_Eqp");
        canvas[1] = Tools.getChild(player.getUI(), "Items_Inventory_Use");
        canvas[2] = Tools.getChild(player.getUI(), "Items_Inventory_Etc");
        canvas[3] = Tools.getChild(player.getUI(), "Items_Inventory_Quest");
        GameObject g = (GameObject)(Instantiate(informationPrefab.gameObject, Vector3.zero, Quaternion.identity));
        information = g.GetComponent<inventoryInformation>();
        information.gameObject.transform.SetParent(canvas[0].transform.parent.parent.parent);
        information.gameObject.transform.SetAsLastSibling();
        this.player.getNetwork().loadInventory();
    }

    public bool addItem(Item item)
    {
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
            if (!mouse.isEmpty() && Input.GetMouseButtonDown(0) && canvas[activeCanvas].GetComponent<MouseOverUI>().isMouseOver())
            {
                Debug.Log("mouse new pos clicked");
                Item temp;
                int newPos = Vec2ToSlots(canvas[activeCanvas].GetComponent<MouseOverUI>().getRelativePosition());
                int pos;
                if ((pos = isSlotEmpty(newPos)) != EMPTY)
                {
                    Debug.Log("isnt empty");
                    temp = itemsOwned[pos].getItem();
                    itemsOwned[pos].getItem().getStats()[8] = itemsOwned[mouse.holdingID].getItem().getStats()[8];
                    recalcPos(pos, itemsOwned[mouse.holdingID].getItem().getStats()[8]);
                }
                else
                {
                    temp = new Item(-1).getEmptyItem(newPos);
                }
                itemsOwned[mouse.holdingID].getItem().getStats()[8] = newPos;
                recalcPos(mouse.holdingID, newPos);
                // Debug.Log("slots: " + Mathf.Floor(canvas.GetComponent<MouseOverUI>().getRelativePosition().x / 50) + " : " + );
                // Debug.Log("item moved");
                player.getNetwork().moveItem(mouse.getItem().stats, temp.stats, PacketTypes.INVENTORY_MOVE_ITEM, this.player);
                mouse.empty();
                break;
            }
            else if (!mouse.isEmpty() && Input.GetMouseButtonDown(0) && !canvas[activeCanvas].GetComponent<MouseOverUI>().isMouseOver())
            {
                itemsOwned.RemoveAt(mouse.holdingID);
            }
        }
        shouldUpdateInventory = false;
    }
    void Update()
    {
        //if (shouldUpdateInventory)
        updateInventory();
    }
    int isSlotEmpty(int slot)
    {
        for (int i = 0; i < itemsOwned.Count; i++)
        {
            if (itemsOwned[i].getItem().getStats()[8] == slot)
                return i;
        }
        return -1;
    }
    void recalcPos(int index, int newPos)
    {
        itemsOwned[index].gameObject.GetComponent<RectTransform>().localPosition = new Vector3((newPos % 4) * 50, -25 - Mathf.Floor(newPos / 4) * 50, 0);
    }
    public void setInventory(int[] items)
    {

        Debug.Log("inventory loaded wewewewwe: " + items.Length);
        this.items = items;
        for (int i = 0; i < items.Length; i += Tools.ITEM_PROPERTY_SIZE)
        {
            Debug.Log("item loaded: " + i + " : " + items.Length);
            GameObject instansiatedSlot = (GameObject)Instantiate(InventorySlot);
            InventorySlot slot = new InventorySlot();
            slot = instansiatedSlot.GetComponent<InventorySlot>();
            slot.setID(i);
            slot.setItem(items[i + 8], new Item(items[i], items[i + 1], items[i + 2], items[i + 3], items[i + 4], items[i + 5], items[i + 6], items[i + 7], items[i + 8]));
            slot.setImage(slot.getItem());
            //itemIDString[i] = instansiatedSlot.GetComponentInChildren<Text> ();

            slot.gameObject.transform.SetParent(canvas[activeCanvas].transform);
            Debug.Log("slot: " + items[i + 8]);
            //invSlot[i].gameObject.GetComponent<RectTransform>().localPosition = new Vector3((invSlot[i].getItem().getStats()[8] % i) * 50, -25 - Mathf.Floor(invSlot[i].getItem().getStats()[8] / 4) * 50, 0);
            slot.gameObject.GetComponent<RectTransform>().localPosition = new Vector3((items[i + 8] % 4) * 50, -25 - Mathf.Floor(items[i + 8] / 4) * 50, 0);
            itemsOwned.Add(slot);
        }
    }

    private int Vec2ToSlots(Vector2 vec2)
    {
        return (int)(Mathf.Floor(vec2.x / 50) + Mathf.Floor(vec2.y / 50) * 4);
    }
}