using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;
using System.IO;
public class PlayerServer {
    public List<Item> items = new List<Item>();
    public List<Equip> equips = new List<Equip>();
    public int databaseID;
    public int connectionID;
    public int playerID;
	public int level = 0;
    public int health = 0;
    public int mana = 0;
    public int maxHealth = 100;
    public int maxMana = 100;
	public int money = 0;
	public string playerName = "";
	public int characterIdPlayingAs = -1;
	public List<Quest> questList = new List<Quest>();
    //public Quest[] quests;
    int[] stats = new int[5];

    int[] skills;
    public PlayerServer( int databaseID, int connectionID) {
        this.databaseID = databaseID;
        this.connectionID = connectionID;
        for (int i = 0; i < 9; i++) {
            equips.Add(null);
        }

    }
	public void setMoney(int money){
		this.money += money;
	}
    public void setPlayerID(int id) {
        this.playerID = id;
    }
    public int getPlayerID() {
        return playerID;
    }
    public List<Item> getItems() {
        return this.items;
    }
    public bool hasEquip(Equip equip) {
        for (int i = 0; i < this.equips.Count; i++)
        {
            if (this.equips[i] == null) continue;
            if (this.equips[i].compareTo(equip))
                return true;
        }
        return false;
    }
    public bool hasItem(Item item) {
        for (int i = 0; i < this.items.Count; i++) {
            if (this.items[i].compareTo(item))
                return true;
        }
        return false;
    }

    public ItemVariables useItem(Item item) {
        ItemVariables pot = ItemDataProvider.getInstance().getStats(item.getID());
        if (item.getID().isItemType(e_itemTypes.USE))
        {
            this.health = Mathf.Min(this.health + pot.getInt("health"), maxHealth);
            this.mana = Mathf.Min(this.mana + pot.getInt("mana"), maxMana);
            item.setQuantity(item.getQuantity() - 1);
        }
        return pot;
    }

    public void replaceItems(Item item1, Item item2) {
        int posItem1 = items.IndexOf(item1);
        int posItem2 = items.IndexOf(item2);
        items[posItem1] = item1;
        items[posItem2] = item1;
    }
    public void addItem(Item item) {
        items.Add(item);
    }
    public void removeItem(Item item) {
        items.Remove(item);
    }
    public void addEquip(Equip equip) {
        equips.Insert((equip.getID() / Tools.ITEM_INTERVAL) - 2, equip);
    }
    public void removeEquip(Equip equip) {
        equips[equips.IndexOf(equip)] = null;
    }
    public void setSkills(int[] skills) {
        this.skills = skills;
    }
    public void saveInventory(Server server) {
        MySqlConnection mysqlConn;
        server.mysqlNonQuerySelector(out mysqlConn, "");
        mysqlConn.Close();
    }
    public bool isEquipOccupied(int equipSlot) {
        if (equips[equipSlot] == null) return false;
        return true;
    }
    public int[] getEquips() {
        return null;
    }
    public byte[] GetEquipBytes()
    {
        return Tools.objectToByteArray(equips);
    }
    public void SetEquipsFromBytes(byte[] bytes) {
        this.equips = (List<Equip>)Tools.byteArrayToObject(bytes);
    }
    public static PlayerServer GetDefaultCharacter(int connectionID)
    {
        return new PlayerServer(-1, connectionID);
        
    }
}
