using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum e_ItemTypes
{
    EQUIP,
    USE,
    ETC,
    NOT_DEFINED,
    MONEY
}

[System.Serializable]
public class Item {
	public int[] stats = new int[Tools.ITEM_PROPERTY_SIZE];
    private int position;
	private int id;
    private int keyID;
    private int inventoryType;
    private int quantity = 1;
    private e_ItemTypes type;
	public Item(int keyID, int position, int inventoryType, params int[] stats){
		this.id = stats[0];
		this.stats = stats;
        this.position = position;
        this.keyID = keyID;
        this.inventoryType = inventoryType;

    }
    public void setQuantity(int amount) {
        this.quantity = amount;
    }
    public int getInventoryType() {
        return this.inventoryType;
    }
    public bool compareTo(Item item) {
        if (this.keyID == item.keyID && this.stats.Equals(item.getStats()))
        {
            return true;
        }
        return false;
    }
    public int getQuantity() {
        return quantity;
    }
    public int getPosition()
    {
        return this.position;
    }
    public void setPosition(int position) {
        this.position = position;
    }
    public void setItem(int[] items) {
        this.stats = items;
    }
	public int[] getStats(){
		return stats;
	}
    public int getType() {
        return stats[Tools.ITEM_PROPERTY_SIZE - 2];
    }
	public int getID(){
		return stats[0];
	}
    public int getKeyID()
    {
        return this.keyID;
    }
	public Item getItem(){
		return this;
	}
    public static Item getEmptyItem(int position) {
        int[] a = new int[Tools.ITEM_PROPERTY_SIZE];
        for (int j = 0; j < a.Length; j++) {
            a[j] = -1;
        }
        return new Item(-1,position,-1,a);
    }
    public e_ItemTypes getItemType()
    {
        return this.type;
    }
    public int getDamage() {
        return stats[1];
    }
    public int getMagicAttack()
    {
        return stats[2];
    }
    public int getStr()
    {
        return stats[3];
    }
    public int getDex()
    {
        return stats[4];
    }
    public int getLuk()
    {
        return stats[5];
    }
    public string getName() {
        return ItemString.itemNames[stats[0]];
    }
    public Equip toEquip() {
        if (inventoryType == (int)inventoryTabs.EQUIP)
            return (Equip)this;
        return null;
    }
}
