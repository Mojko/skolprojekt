using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;
public class PlayerServer {
    public int[] items;
    public Dictionary<int, int[]> itemsDictionary = new Dictionary<int, int[]>();
    public List<int[]> equips = new List<int[]>();
    public int databaseID;
    public int connectionID;
	public int level = 0;
	public string playerName = "";
	public Quest[] quests;

    int[] stats = new int[5];

    int[] skills;
    public PlayerServer( int databaseID, int connectionID) {
        this.databaseID = databaseID;
        this.connectionID = connectionID;
        for (int i = 0; i < 9; i++) {
            equips.Add(null);
        }
    }

    public int[] getItem(int itemId)
    {
        int[] value;
        if(itemsDictionary.TryGetValue(itemId, out value)){
            return value;
        }
        return null;
    }

    public void addItems(int[] items) {
        this.items = items;
        for(int i=0;i<items.Length;i+=Tools.ITEM_PROPERTY_SIZE){
            int[] tempItem = new int[Tools.ITEM_PROPERTY_SIZE];
            for(int j=0;j<Tools.ITEM_PROPERTY_SIZE;j++){
               tempItem[j] = items[i+j];
            }
            itemsDictionary.Add(items[i], tempItem);
        }
    }
    public void addItem(int[] item)
    {
        int[] tempItem = new int[this.items.Length+item.Length];
        for(int i = 0; i < this.items.Length; i++) {
            tempItem[i] = this.items[i];
        }
        for(int i = this.items.Length; i < item.Length; i++) {
            tempItem[i] = item[i];
        }
    }
    public void addEquip(int type, int[] equip) {
        Debug.Log("added equip: " + type);
        Debug.Log("count size: " + equips.Count);
        equips[type] = equip;
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
        /*
        int[] equips = new int[this.equips.Length * Tools.ITEM_PROPERTY_SIZE];
        for (int i = 0; i < this.equips.Length; i++) {
            for (int j = 0; j < Tools.ITEM_PROPERTY_SIZE; j++)
            {
                equips[i + j] = this.equips[i][j];
            }
        }
        return equips;
        */
        return null;
    }
    public byte[] GetEquipBytes()
    {
        return Tools.objectToByteArray(equips);
    }
    public void SetEquipsFromBytes(byte[] bytes) {
        this.equips = (List<int[]>)Tools.byteArrayToObject(bytes);
    }
    public static PlayerServer GetDefaultCharacter(int connectionID)
    {
        return new PlayerServer(-1, connectionID);
        
    }
}
