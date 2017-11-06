using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;
public class PlayerServer {
    public int[] items;
    public int[][] equips = new int[9][];
    public int databaseID;
    public int connectionID;

    int[] stats = new int[5];

    int[] skills;
    public PlayerServer( int databaseID, int connectionID) {
        this.databaseID = databaseID;
        this.connectionID = connectionID;
    }
    public void addItems(int[] items) {
        this.items = items;
    }
    public void addEquip(int type, int[] equip) {
        Debug.Log("added equip: " + type);
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
    public int[] getEquip() {
        int[] equips = new int[this.equips.Length * Tools.ITEM_PROPERTY_SIZE];
        for (int i = 0; i < this.equips.Length; i++) {
            for (int j = 0; j < Tools.ITEM_PROPERTY_SIZE; j++)
            {
                equips[i + j] = this.equips[i][j];
            }
        }
        return equips;
    }
    public static PlayerServer getDefaultCharacter(int connectionID)
    {
        return new PlayerServer(-1, connectionID);
        
    }
}
