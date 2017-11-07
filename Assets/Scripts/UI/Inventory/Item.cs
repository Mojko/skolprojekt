using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Item {
	public int[] stats = new int[Tools.ITEM_PROPERTY_SIZE];
	int id;
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
	public int getID(){
		return stats[0];
	}
	public Item getItem(){
		return this;
	}
    public Item getEmptyItem(int i) {
        int[] a = new int[Tools.ITEM_PROPERTY_SIZE];
        for (int j = 0; j < Tools.ITEM_PROPERTY_SIZE; j++) {
            a[j] = -1;
        }
        a[Tools.ITEM_PROPERTY_SIZE - 1] = i;
        return new Item(a);
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
