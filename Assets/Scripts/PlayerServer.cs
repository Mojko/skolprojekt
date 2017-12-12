using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;
using System.IO;
using UnityEngine.Networking;
public class PlayerServer {
    public List<Item> items = new List<Item>();
    public List<Equip> equips = new List<Equip>();
    public NetworkInstanceId netID;
    public int databaseID;
    public int connectionID;
    public int playerID;
	public int level = 1;
	public NetworkInstanceId netId;

    PlayerStats info;

    public string playerName = "";
	public int characterIdPlayingAs = -1;
    public int money = 0;
	public List<Quest> questList = new List<Quest>();
    //public Quest[] quests;
    int[] stats = new int[5];
    Skill[] skills;
    public PlayerServer( int databaseID, int connectionID, NetworkInstanceId netID) {
        info = new PlayerStats();
        this.databaseID = databaseID;
        this.connectionID = connectionID;
        this.netID = netID;
        for (int i = 0; i < 9; i++) {
            equips.Add(null);
        }
    }
    public int getClosestSlot(int storeType)
    {
        bool isEmpty = true;
        for (int i = 0; i < Inventory.MAX_INVENTORY_SIZE; i++)
        {
            isEmpty = true;
            for (int j = 0; j < items.Count; j++)
            {
                if (items[j].getItem().getInventoryType() == storeType && items[j].getPosition() == i)
                {
                    isEmpty = false;
                    break;
                }
            }
            if (isEmpty)
            {
                Debug.Log("Emty::::::::: " + i);
                return i;
            }
        }
        return -1;
    }
    public PlayerStats getPlayerStats() {
        return info;
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
    public Item getItem(Item item) {
        for (int i = 0; i < this.items.Count; i++)
        {
            if (this.items[i].compareTo(item))
                return items[i];
        }
        return null;
    }
    public Item findItemWithKey(int key) {
        for (int i = 0; i < this.items.Count; i++)
        {
            if (this.items[i].getKeyID() == key)
                return items[i];
        }
        return null;
    }
	//This can only run ONCE
	public void levelUp(){
		this.level += 1;
		this.info.exp = 0;
		this.getPlayerStats().expRequiredForNextLevel *= 2;
		Server.sendLevelUp(this.getPlayerStats().expRequiredForNextLevel, this.connectionID);
		Debug.Log("LEVELUP!!");
	}

	public void giveExp(int exp){
		this.info.exp += exp;
		if(this.info.exp >= this.getPlayerStats().expRequiredForNextLevel){
			levelUp();
		}
		Debug.Log("EXP given, EXP now: " + this.info.exp + " | " + this.getPlayerStats().expRequiredForNextLevel);
	}

    public ItemVariables useItem(Item item) {

        //hämtar items variablar.
        ItemVariables pot = ItemDataProvider.getInstance().getStats(item.getID());
        //kollar om det är en use så den läser in rätt variablar
        if (item.getID().isItemType(e_itemTypes.USE))
        {
            //ökar hpet på spelare och manan.
            this.getPlayerStats().health = Mathf.Min(this.getPlayerStats().health + pot.getInt("health"), getPlayerStats().maxHealth);
            this.getPlayerStats().mana = Mathf.Min(this.getPlayerStats().mana + pot.getInt("mana"), getPlayerStats().maxMana);
            //gör så att mängden minskar med 1.
            item.setQuantity(item.getQuantity() - 1);
        }
        return pot;
    }

    public void setMoney(int money)
    {
        this.info.money = money;
    }
    public int getMoney()
    {
        return this.money;
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
	public void setSkills(Skill[] skills) {
        this.skills = skills;
    }
	public Skill[] getSkills(){
		return this.skills;
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
	public List<Equip> getEquips() {
		return equips;
	}
    public byte[] GetEquipBytes()
    {
        return Tools.objectToByteArray(equips);
    }
    public void SetEquipsFromBytes(byte[] bytes) {
        this.equips = (List<Equip>)Tools.byteArrayToObject(bytes);
    }
    public static PlayerServer GetDefaultCharacter(int connectionID, NetworkInstanceId netID)
    {
        return new PlayerServer(-1, connectionID,  netID);
        
    }
}
