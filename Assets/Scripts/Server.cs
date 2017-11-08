using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using MySql.Data.MySqlClient;
using System.Reflection;
using System.IO;
using System.Linq;
using System;
using System.Xml;
using System.IO;
using System.Text;
using NPCManager;
using UnityEngine.AI;
using NPCObject = System.Func<NPCManager.NPCConversationManager, int, bool>;

public class Server : NetworkManager
{
    delegate bool Mark();
    ResourceStructure resourceStructure;
    private Dictionary<string, int> playerID = new Dictionary<string, int>();
    private Dictionary<string, int> charactersOnline = new Dictionary<string, int>();
    private Dictionary<int, string> characterConnections = new Dictionary<int, string>();
    private Dictionary<int, string> connections = new Dictionary<int, string>();
    private Dictionary<string, int> conns = new Dictionary<string, int>();
    private Dictionary<string, NPCObject> npcActive = new Dictionary<string, NPCObject>();
    private Dictionary<int, PlayerServer> playerObjects = new Dictionary<int, PlayerServer>();
    public int port;
    public NPCCompiler npcCompiler = new NPCCompiler();
    public string database, username, password, host;
    string connectionString;
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        addPlayer(conn, playerControllerId);
    }
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
    {
        addPlayer(conn, playerControllerId);
    }
    public void addPlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject player = (GameObject)Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        player.name = "Player";
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (connections.ContainsKey(conn.connectionId))
        {
            Debug.Log("disconnect server func");
            string name = connections[conn.connectionId];
            charactersOnline.Remove(characterConnections[conn.connectionId]);
            playerID.Remove(connections[conn.connectionId]);
            connections.Remove(conn.connectionId);
            characterConnections.Remove(conn.connectionId);
            playerObjects.Remove(conn.connectionId);
            conns.Remove(name);
        }
        base.OnServerDisconnect(conn);
    }

    //#SETUP
    public override void OnStartServer()
    {
        Application.targetFrameRate = 60;
        connectionString = "Server=" + host + ";Database=" + database + ";Uid=" + username + ";Pwd=" + password + ";";
        base.OnStartServer();
        NetworkServer.RegisterHandler(PacketTypes.LOAD_PLAYER, onLoadCharacter);
        NetworkServer.RegisterHandler(PacketTypes.LOGIN, onLogin);
        NetworkServer.RegisterHandler(PacketTypes.DISCONNECT, onDisconnect);
        NetworkServer.RegisterHandler(PacketTypes.SAVE_INVENTORY, onSaveInventory);
        NetworkServer.RegisterHandler(PacketTypes.INVENTORY_MOVE_ITEM, onChangeItem);
        NetworkServer.RegisterHandler(PacketTypes.INVENTORY_DROP_ITEM, onDropItem);
        NetworkServer.RegisterHandler(PacketTypes.LOAD_INVENTORY, onLoadInventory);
        NetworkServer.RegisterHandler(PacketTypes.INVENTORY_PICKUP_ITEM, onPickupItem);
        NetworkServer.RegisterHandler(PacketTypes.SEND_MESSAGE, onReciveMessage);
        NetworkServer.RegisterHandler(PacketTypes.NPC_INTERACT, onNPCInteract);
        NetworkServer.RegisterHandler(PacketTypes.PICK_CHAR, onCharPicked);
        NetworkServer.RegisterHandler(PacketTypes.VERIFY_SKILL, onVerifySkills);
		NetworkServer.RegisterHandler(PacketTypes.PLAYER_BUFF, onPlayerBuff);
        NetworkServer.RegisterHandler(PacketTypes.PROJECTILE_CREATE, onProjectTileCreate);
        NetworkServer.RegisterHandler(PacketTypes.MONSTER_SPAWN, onMonsterSpawn);
        NetworkServer.RegisterHandler(PacketTypes.SPAWN_ITEM, onSpawnItem);
        resourceStructure = new ResourceStructure();
        
        //Create system objects
        Instantiate(ResourceStructure.getGameObjectFromObject(e_Objects.SYSTEM_RESPAWNER));

    }

    private void onSpawnItem(NetworkMessage netMsg)
    {
        ItemInfo itemInfo = netMsg.ReadMessage<ItemInfo>();
        playerObjects[netMsg.conn.connectionId].addItem(itemInfo.item);
    }

    private void onMonsterSpawn(NetworkMessage netMsg)
    {
        MobInfo mobInfo = netMsg.ReadMessage<MobInfo>();
        for(int i=0;i<mobInfo.amount;i++){
            spawnMonster(mobInfo.mobId);
         }
    }

    public ResourceStructure getResourceStructure()
    {
        return this.resourceStructure;
    }

    public void onProjectTileCreate(NetworkMessage netMsg)
    {
        ProjectTileInfo pInfo = netMsg.ReadMessage<ProjectTileInfo>();

        GameObject skillPrefab = (GameObject)Resources.Load(pInfo.pathToObject);
        GameObject skillEffectPrefab = (GameObject)Resources.Load(pInfo.pathToEffect);

        GameObject skillObject = Instantiate(skillPrefab);
        GameObject skillEffect = Instantiate(skillEffectPrefab);

        skillEffect.transform.SetParent(skillObject.transform);
        skillObject.transform.position = new Vector3(pInfo.spawnPosition.x, pInfo.spawnPosition.y+1f, pInfo.spawnPosition.z);
        skillObject.transform.rotation = Quaternion.Euler(pInfo.rotationInEuler);
        NetworkServer.Spawn(skillObject);
    }

	public void onPlayerBuff(NetworkMessage netMsg){
		StatInfo statInfo = netMsg.ReadMessage<StatInfo>();
		NetworkServer.SendToAll(PacketTypes.PLAYER_BUFF, statInfo);
	}

    //#DEPRICATED
    public static GameObject spawnObject(GameObject prefab, Vector3 position)
    {
        GameObject obj = Instantiate(prefab);
        obj.transform.position = position;
        NetworkServer.Spawn(obj);
        return obj;
    }

    public static GameObject spawnObject(string name, Vector3 position)
    {
        GameObject prefab = (GameObject)Resources.Load("Prefabs/" + name);
        GameObject obj = Instantiate(prefab);
        obj.transform.position = position;
        NetworkServer.Spawn(obj);
        return obj;
    }
    public static GameObject spawnObject(string name)
    {
        GameObject prefab = (GameObject)Resources.Load("Prefabs/" + name);
        GameObject obj = Instantiate(prefab);
        NetworkServer.Spawn(obj);
        return obj;
    }
    //#END OF DEPRICATED


    //#NEW SPAWN METHODS
    public static void spawnObject(e_Objects obj)
    {
        NetworkServer.Spawn(Instantiate(Tools.loadObjectFromResources(obj)));
    }
    public static void spawnObject(e_Objects obj, Vector3 position)
    {
        GameObject o = Instantiate(Tools.loadObjectFromResources(obj));
        o.transform.position = position;
        NetworkServer.Spawn(o);
    }
    public static void spawnObject(GameObject prefab)
    {
        GameObject o = Instantiate(prefab);
        NetworkServer.Spawn(o);
    }

    public static Monster getMonsterFromJson(int monsterId)
    {
        Monster monsters = JsonUtility.FromJson<Monster>(File.ReadAllText("Assets/XML/Monster.json"));
        foreach(Monster monster in monsters.Monsters) {
            if(monster.id == monsterId) {
                return monster;
            }
        }
        return null;
    }

    public static void spawnMonster(int id)
    {
        Monster monsterToFind = getMonsterFromJson(id);
        if(monsterToFind != null){
            GameObject enemy = Instantiate(ResourceStructure.getGameObjectFromPath(monsterToFind.pathToModel));
            NetworkServer.Spawn(enemy);
        }
    }
    public static void spawnMonster(int id, Vector3 pos)
    {
        Monster monsterToFind = getMonsterFromJson(id);
        if(monsterToFind != null){
            GameObject enemy = Instantiate(ResourceStructure.getGameObjectFromPath(monsterToFind.pathToModel));
            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            
            if(agent){
                agent.Warp(pos);
            }

            NetworkServer.Spawn(enemy);
        }
    }
    /*public static void spawnParticle(e_Objects obj, Vector3 position){
        GameObject part = Tools.loadObjectFromResources(obj);
        part.transform.position = position;
        NetworkServer.Spawn(Instantiate(part));
    }*/

    //# SKILLS
    void onSaveSkills(NetworkMessage netMsg)
    {
        //Initilize
        SkillInfo skillInfo = netMsg.ReadMessage<SkillInfo>();
        MySqlConnection conn;
        MySqlDataReader reader;
        int playerID = getPlayerIDFromDir(skillInfo.playerName);

        //Read
        mysqlReader(out conn, out reader, "SELECT * FROM skills");
        int idFound = -1;
        while (reader.Read() && idFound == -1)
        {
            if (reader.GetInt32("playerID").Equals(playerID) && reader.GetInt32("skillId").Equals(skillInfo.id))
            {
                idFound = skillInfo.id;
                break;
            }
        }
        if (idFound != -1)
        {
            mysqlNonQuerySelector(out conn, "UPDATE skills SET skillId='" + idFound + "', currentPoints='" + skillInfo.currentPoints + "', maxPoints='" + skillInfo.maxPoints + "' WHERE playerID='" + playerID + "' AND skillId='" + idFound + "'");
        }
        else
        {
            mysqlNonQuerySelector(out conn, "INSERT INTO skills(playerID, skillId, currentPoints, maxPoints) VALUES('" + playerID + "', '" + skillInfo.id + "', '" + skillInfo.currentPoints + "', '" + skillInfo.maxPoints + "')");
        }
        //Finish
        conn.Close();
        NetworkServer.SendToClient(netMsg.conn.connectionId, PacketTypes.LOAD_SKILLS, skillInfo);
    }

    void onVerifySkills(NetworkMessage netMsg)
    {
        SkillInfo skillInfo = netMsg.ReadMessage<SkillInfo>();
        MySqlConnection conn;
        MySqlDataReader reader;
        int playerID = getPlayerIDFromDir(skillInfo.playerName);
        mysqlReader(out conn, out reader, "SELECT * FROM skills WHERE skillId='" + skillInfo.id + "'");

        int skillIdFromDatabase = 0;
        int playerIdFromDatabase = 0;
        int maxPointsFromDatabase = 0;
        int results = 0;

        while (reader.Read())
        {
            results++;
            skillIdFromDatabase = reader.GetInt32("skillId");
            playerIdFromDatabase = reader.GetInt32("playerID");
            maxPointsFromDatabase = reader.GetInt32("maxPoints");
        }

        if (results > 0)
        {
            if (maxPointsFromDatabase > skillInfo.currentPoints)
            {
                mysqlNonQuerySelector(out conn, "UPDATE skills SET skillId='" + skillInfo.id + "', currentPoints='" + (skillInfo.currentPoints+1) + "', maxPoints='" + skillInfo.maxPoints + "' WHERE playerID='" + playerID + "' AND skillId='" + skillInfo.id + "'");
                NetworkServer.SendToClient(netMsg.conn.connectionId, PacketTypes.VERIFY_SKILL, skillInfo);
            }
            else
            {
                NetworkServer.SendToClient(netMsg.conn.connectionId, PacketTypes.ERROR_SKILL, skillInfo);
            }
        }
        else
        {
            mysqlNonQuerySelector(out conn, "INSERT INTO skills(playerID, skillId, currentPoints, maxPoints) VALUES('" + playerID + "', '" + skillInfo.id + "', '" + (skillInfo.currentPoints+1) + "', '" + skillInfo.maxPoints + "')");
            NetworkServer.SendToClient(netMsg.conn.connectionId, PacketTypes.VERIFY_SKILL, skillInfo);
        }
        conn.Close();
    }

    string skillsSqlValues(SkillInfo skillInfo)
    {
        return "playerName='" + skillInfo.playerName + "', skillId='" + skillInfo.id + "', currentPoints='" + skillInfo.currentPoints + "', maxPoints='" + skillInfo.maxPoints + "'";
    }


    //# CHARACTER
    void onCharPicked(NetworkMessage msg)
    {
        OnPickCharacterPacket packet = msg.ReadMessage<OnPickCharacterPacket>();
    }
    void onNPCInteract(NetworkMessage msg)
    {
        Debug.Log("debugged npc");
        NPCInteractPacket senderObj = msg.ReadMessage<NPCInteractPacket>();
        if (!npcActive.ContainsKey(senderObj.sender))
            npcActive.Add(senderObj.sender, npcCompiler.compileNPC(senderObj.npcID, msg.conn));

        NPCObject npc = npcActive[senderObj.sender];
        if (npc.Invoke(npcCompiler.objManager, senderObj.state))
        {
            npcActive.Remove(senderObj.sender);
        }
    }
    void onLoadCharacter(NetworkMessage msg)
    {
        PlayerInfo packet = msg.ReadMessage<PlayerInfo>();
        int id = getCharacterID(packet.characterName);
        //PlayerServer player = getPlayerObject(msg.conn.connectionId);
        PlayerServer player = PlayerServer.GetDefaultCharacter(msg.conn.connectionId);

        connections.Add(msg.conn.connectionId, packet.name);
        conns.Add(packet.name, msg.conn.connectionId);
        charactersOnline.Add(packet.characterName, getCharacterID(packet.characterName));
        characterConnections.Add(msg.conn.connectionId, packet.characterName);
        playerObjects.Add(msg.conn.connectionId, new PlayerServer(msg.conn.connectionId, id));

        MySqlConnection conn;
        MySqlDataReader reader;
        mysqlReader(out conn, out reader, "SELECT * FROM skills WHERE playerID = '" + id + "'");
        List<int> skillProperties = new List<int>();
        Debug.Log("SERVER: wew");
        while (reader.Read()) {

            skillProperties.Add(reader.GetInt16("skillId"));
            skillProperties.Add(reader.GetInt16("currentPoints"));
            skillProperties.Add(reader.GetInt16("maxPoints"));
        }
        player.setSkills(skillProperties.ToArray());
        packet.skillProperties = skillProperties.ToArray();

        NetworkServer.SendToClient(msg.conn.connectionId, PacketTypes.LOAD_PLAYER, packet);
        conn.Close();
    }
    void onReciveMessage(NetworkMessage msg)
    {
        PacketMessage message = msg.ReadMessage<PacketMessage>();
        if (message.type == MessageTypes.EVERYONE)
        {
            NetworkServer.SendToAll(PacketTypes.SEND_MESSAGE, message);
        }
        else if (message.type == MessageTypes.PRIVATE_MESSAGE)
        {
            int connId = conns[message.reciver];
            NetworkServer.SendToClient(connId, PacketTypes.SEND_MESSAGE, message);
        }
        else if (message.type == MessageTypes.CHAT)
        {
            NetworkServer.SendToAll(PacketTypes.SEND_MESSAGE, message);
        } else if(message.type == MessageTypes.SERVER) {
            message.message = "SERVER: " + message.message;
            NetworkServer.SendToAll(PacketTypes.SEND_MESSAGE, message);
        }
    }
    //# INVENTORY

    void onLoadInventory(NetworkMessage msg)
    {
        Debug.Log("loaded inventory outside");
        InventoryInfo inf = msg.ReadMessage<InventoryInfo>();
        inf.items = getInventoryFromDatabase(inf.name, msg.conn.connectionId);
        Debug.Log("inf.quipment.COunt: " + inf.equipment.Length);
        inf.equipment = playerObjects[msg.conn.connectionId].GetEquipBytes();
        Debug.Log(inf.items.Length);
        NetworkServer.SendToClient(msg.conn.connectionId, PacketTypes.LOAD_INVENTORY, inf);
    }

    private int[] getInventoryFromDatabase(string character) {
        return getInventoryFromDatabase(character, -1);
    }

    private int[] getInventoryFromDatabase(string character, int connectionID)
    {
        Debug.Log("loaded inventory");
        int characterID = getCharacterIDFromDir(character);
        //string connectionString = "Server=" + host + ";Database=" + database + ";Uid=Gerry;Pwd=pass;";
        MySqlConnection conn;
        MySqlCommand cmd;
        MySqlDataReader reader;
        PlayerServer player = getPlayerObject(connectionID);
        mysqlReader(out conn, out reader, "SELECT * FROM inventory LEFT JOIN inventoryEquipment ON inventory.id = inventoryEquipment.inventoryID WHERE characterID = '" + characterID + "'");
        List<int> data = new List<int>();
        List<int> equips = new List<int>();
        while (reader.Read())
        {
            int invType = reader.GetInt32("inventoryType");
            if (invType == 0)
            {
                int position = reader.GetInt32("position");
                Debug.Log("item psotions: " + position);
                if (position >= 0)
                {
                    data.Add(reader.GetInt32("itemID"));
                    data.Add(reader.GetInt32("Watt"));
                    data.Add(reader.GetInt32("Matt"));
                    data.Add(reader.GetInt32("Luk"));
                    data.Add(-1);
                    data.Add(-1);
                    data.Add(-1);
                    data.Add(-1);
                    data.Add(-1);
                    data.Add(-1);
                    data.Add(-1);
                    data.Add(-1);
                    data.Add(reader.GetInt32("id"));
                    data.Add(invType);
                    data.Add(reader.GetInt32("position"));
                }
                else {

                    int pos = reader.GetInt32("position");
                    int[] item = new int[] {
                        reader.GetInt32("itemID"),
                        reader.GetInt32("Watt"),
                        reader.GetInt32("Matt"),
                        reader.GetInt32("Luk"),
                        -1,
                        -1,
                        -1,
                        -1,
                        -1,
                        -1,
                        -1,
                        -1,
                        reader.GetInt32("id"),
                        invType,
                        position
                    };
                    Debug.Log("position in an item from a database: " + Math.Abs(position + 1));
                    player.addEquip(Math.Abs(position + 1), item);
                }
            }
            else
            {
                data.Add(reader.GetInt32("itemID"));
                data.Add(-1);
                data.Add(-1);
                data.Add(-1);
                data.Add(-1);
                data.Add(-1);
                data.Add(-1);
                data.Add(-1);
                data.Add(-1);
                data.Add(-1);
                data.Add(-1);
                data.Add(-1);
                data.Add(reader.GetInt32("id"));
                data.Add(invType);
                data.Add(reader.GetInt32("position"));
            }
        }
        int[] dataArray = data.ToArray();
        player.addItems(dataArray);
        conn.Close();
        Debug.Log("inventory array: " + dataArray);
        return dataArray;
    }

    void onSaveInventory(NetworkMessage netMsg)
    {
        playerObjects[netMsg.conn.connectionId].saveInventory(this);
    }
    public void onDropItem(NetworkMessage netMsg)
    {
        moveItem item = netMsg.ReadMessage<moveItem>();
        MySqlConnection conn;
        MySqlDataReader reader;
        int characterID = getCharacterIDFromDir(item.player);
        //Debug.Log("wew from drop: " + item.name + " : " + sqlItemStatement(item.item1));
        mysqlReader(out conn, out reader, "SELECT * FROM inventory WHERE characterID = " + characterID + " AND " + sqlItemStatement(item.item1) + "");
        mysqlNonQuerySelector(out conn, "DELETE FROM inventory WHERE characterID = " + characterID + " AND " + sqlItemStatement(item.item1) + "");
        conn.Close();
        GameObject itemObject = spawnObject("Drop", new Vector3(item.position[0], item.position[1], item.position[2]));
    }

    public void onPickupItem(NetworkMessage netMsg)
    {
        moveItem item = netMsg.ReadMessage<moveItem>();
        MySqlConnection conn;
        int playerID = getCharacterIDFromDir(item.player);
        Debug.Log("wew from pickup: " + item.player + " : " + playerID);
        mysqlNonQuerySelector(out conn, "INSERT INTO inventory (playerID, itemID, WAtt, MAtt, Luk, position) VALUES ('" + playerID + "','" + item.item1[0] + "','" + item.item1[1] + "','" + item.item1[2] + "','" + item.item1[3] + "','" + item.item1[4] + "')");
        conn.Close();
    }

    public void onChangeItem(NetworkMessage netMsg)
    {
        moveItem item = netMsg.ReadMessage<moveItem>();
        MySqlConnection conn;
        //ServerDebug("hello from server");
        int playerID = getCharacterIDFromDir(item.player);
        Debug.Log("item1: " + item.item2[Tools.ITEM_PROPERTY_SIZE - 1] + " item2: " + item.item1[Tools.ITEM_PROPERTY_SIZE - 2] + " item2: " + item.item1[0]);
        mysqlNonQuerySelector(out conn, "UPDATE inventory SET position = '" + item.item1[Tools.ITEM_PROPERTY_SIZE - 1] + "' WHERE id = '" + item.item1[Tools.ITEM_PROPERTY_SIZE - 3] + "'");
        if (item.item2[0] != -1)
            mysqlNonQuerySelector(out conn, "UPDATE inventory SET position = '" + item.item2[Tools.ITEM_PROPERTY_SIZE - 1] + "' WHERE id = '" + item.item2[Tools.ITEM_PROPERTY_SIZE - 3] + "'");
        conn.Close();
    }


    private string sqlItemStatement(int[] item)
    {
        return "itemID = '" + item[0] + "' AND inventoryType = '" + item[Tools.ITEM_PROPERTY_SIZE - 2] + "'";
    }

    //#PLAYER LOGIN

    void onLogin(NetworkMessage msg)
    {

        //string connectionString = "Server=" + host + ";Database=" + database + ";Uid=Gerry;Pwd=pass;";
        MySqlConnection mysqlConn;

        LoginPacket packet = msg.ReadMessage<LoginPacket>();
        //MySqlCommand getLogin = new MySqlCommand("SELECT * FROM accounts WHERE name='" + packet.name + "' AND password='" + packet.password + "' ",mysqlConn);
        MySqlDataReader reader;
        mysqlReader(out mysqlConn, out reader, "SELECT * FROM accounts WHERE name='" + packet.name + "' AND password='" + packet.password + "' ");
        int resultAmount = 0;
        int id = -1;
        while (reader.Read())
        {
            resultAmount++;
            id = reader.GetInt32("id");
        }
        loadCharacters pack = new loadCharacters();
        if (resultAmount > 0 /*!isAlreadyOnline(packet.name)*/)
        {
            pack.successfull = true;
            pack.name = packet.name;
            int characters = 0;
            List<string> names = new List<string>();
            List<string> color = new List<string>();
            List<int> stats = new List<int>();
            List<int> itemsEquip = new List<int>();
            mysqlReader(out mysqlConn, out reader, "SELECT * FROM characters WHERE accountID = '" + id + "'");
            while (reader.Read())
            {
                names.Add(reader.GetString("characterName"));

                stats.Add(reader.GetInt16("luk"));
                stats.Add(reader.GetInt16("str"));
                stats.Add(reader.GetInt16("int"));
                stats.Add(reader.GetInt16("dex"));
                stats.Add(reader.GetInt16("hp"));
                stats.Add(reader.GetInt16("level"));

                color.Add(reader.GetString("hairColor"));
                color.Add(reader.GetString("eyeColor"));
                color.Add(reader.GetString("skinColor"));
                color.Add(reader.GetString("eyebrowColor"));
            }
            Debug.Log("wew: " + pack.successfull);
            pack.colorScheme = color.ToArray();
            pack.stats = stats.ToArray();
            pack.itemsEquip = itemsEquip.ToArray();
            pack.names = names.ToArray();
           
            if (playerID[packet.name] == null)
            {
                int pID = getPlayerID(packet.name);
                playerID.Add(packet.name, pID);
            }
        }
        if (/*isAlreadyOnline(packet.name)*/ false)
            pack.notSuccessfullReason = "Player already logged in";
        if (resultAmount == 50)
            pack.notSuccessfullReason = "Wrong username or password";
        NetworkServer.SendToClient(msg.conn.connectionId, PacketTypes.LOGIN, pack);
        mysqlConn.Close();
    }
    bool isAlreadyOnline(string name)
    {
        return playerID.ContainsKey(name);
    }

    //#Player Disconnect

    void onDisconnect(NetworkMessage msg)
    {
        DisconnectPacket packet = msg.ReadMessage<DisconnectPacket>();
        Debug.Log("disconnect");
        playerID.Remove(packet.name);
    }

    //# MYSQL TOOLS

    public void mysqlReader(out MySqlConnection mysqlConn, out MySqlDataReader reader, string sql)
    {
        //string connectionString = "Server=" + host + ";Database=" + database + ";Uid="+username+";Pwd="+password+";";
        mysqlConn = new MySqlConnection(connectionString);
        MySqlCommand cmd = new MySqlCommand(sql, mysqlConn);
        mysqlConn.Open();
        reader = cmd.ExecuteReader();
    }
    public void mysqlNonQuerySelector(out MySqlConnection mysqlConn, string sql)
    {
        //string connectionString = "Server=" + host + ";Database=" + database + ";Uid="+username+";Pwd="+password+";";
        mysqlConn = new MySqlConnection(connectionString);
        MySqlCommand cmd = new MySqlCommand(sql, mysqlConn);
        mysqlConn.Open();
        cmd.ExecuteNonQuery();
    }
    void ServerDebug(string msg)
    {
        GameObject.Find("Debug").GetComponent<UnityEngine.UI.Text>().text = msg;
    }
    private int getPlayerIDFromDir(string name)
    {
        if (playerID.ContainsKey(name))
            return playerID[name];

        return getPlayerID(name);
    }
	private GameObject getPlayerObjectFromName(string name){
		
		GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");

		foreach(GameObject o in playerObjects){
			if(o.GetComponent<Player>().playerName.Equals(name)){
				return o;
			}
		}
		return null;
	}
    private int getCharacterIDFromDir(string name)
    {
        if (charactersOnline.ContainsKey(name)) return charactersOnline[name];
        return getCharacterID(name);
    }
    private int getPlayerID(string name)
    {

        //string connectionString = "Server=" + host + ";Database=" + database + ";Uid=Gerry;Pwd=pass;";
        MySqlConnection conn = new MySqlConnection(connectionString);
        Debug.Log("name: " + name);
        MySqlCommand cmd = new MySqlCommand("SELECT id FROM accounts WHERE name = '" + name + "'", conn);
        conn.Open();
        MySqlDataReader reader = cmd.ExecuteReader();
        string id = "-1";
        if (reader.Read())
        {
            id = reader["id"].ToString();
        }
        conn.Close();
        return int.Parse(id);
    }
    private int getCharacterID(string name)
    {
        //string connectionString = "Server=" + host + ";Database=" + database + ";Uid=Gerry;Pwd=pass;";
        MySqlConnection conn = new MySqlConnection(connectionString);
        Debug.Log("name: " + name);
        MySqlCommand cmd = new MySqlCommand("SELECT id FROM characters WHERE characterName = '" + name + "'", conn);
        conn.Open();
        MySqlDataReader reader = cmd.ExecuteReader();
        int id = -1;
        if (reader.Read())
        {
            id = reader.GetInt32("id");
        }
        conn.Close();
        return id;
    }
    private PlayerServer getPlayerObject(int connectionID) {
        return playerObjects[connectionID];
    }
}

[System.Serializable]
public class Monster
{
    public Monster[] Monsters;
    public int id;
    public string name;
    public int[] stats;
    public int level;
    public string pathToModel;
}