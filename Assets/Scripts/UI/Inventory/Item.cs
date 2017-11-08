using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum e_ItemTypes
{
    EQUIP,
    USE,
    ETC,
    NOT_DEFINED4,
    MONEY
}

public class Item {
	public int[] stats = new int[Tools.ITEM_PROPERTY_SIZE];
	private int id;
    private e_ItemTypes type;
	public Item(params int[] stats){
		this.id = stats[0];
		this.stats = stats;
	}
    public int getPosition()
    {
        return this.stats[Tools.ITEM_PROPERTY_SIZE-1];
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
	public Item getItem(){
		return this;
	}
    public static Item getEmptyItem(int position) {
        int[] a = new int[Tools.ITEM_PROPERTY_SIZE];
        for (int j = 0; j < a.Length; j++) {
            a[j] = -1;
        }
        a[Tools.ITEM_PROPERTY_SIZE - 1] = position;

        return new Item(a);
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
}
