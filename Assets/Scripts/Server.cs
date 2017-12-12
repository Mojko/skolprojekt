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

public enum e_DamageType
{
	STRENGTH,
	MAGIC
}
public enum e_DamageTarget
{
	PLAYER,
	MOB
}

public class Server : NetworkManager
{
    delegate bool Mark();

    public GameObject npcManagerPrefab;

    private Dictionary<string, int> playerID = new Dictionary<string, int>();
    private Dictionary<string, int> charactersOnline = new Dictionary<string, int>();
    private Dictionary<int, string> characterConnections = new Dictionary<int, string>();
    private Dictionary<int, string> connections = new Dictionary<int, string>();
    private Dictionary<string, int> conns = new Dictionary<string, int>();
    private Dictionary<string, NPCObject> npcActive = new Dictionary<string, NPCObject>();
    public static Dictionary<int, PlayerServer> playerObjects = new Dictionary<int, PlayerServer>();
    private NPCMain npc;
    public int port;
    public NPCCompiler npcCompiler = new NPCCompiler();
    public string database, username, password, host;
	private QuestManager questManager;
    string connectionString;
    new void Start()
    {
        UnityEngine.Object[] registeredPrefabs = Resources.LoadAll("Prefabs/Drops/");
        GameObject obj;
        for (int i = 0; i < registeredPrefabs.Length; i++) {
            obj = (GameObject)registeredPrefabs[i];
            this.spawnPrefabs.Add(obj);
        }
    }
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
            PlayerServer pServer = getPlayerObject(conn.connectionId);
            foreach(Quest q in pServer.questList.ToArray()){
				addOrUpdateQuestStatusToDatabase(q, pServer, false, PacketTypes.QUEST_START);
				Debug.Log("Updating quest... " + q.getId());
            }
            /*
			MySqlConnection mysqlConn;
			mysqlNonQuerySelector(out mysqlConn, "UPDATE characters SET money = '"+pServer.getPlayerStats().money+"' WHERE id = '"+pServer.getPlayerID()+"'");
			mysqlConn.Close();
			mysqlNonQuerySelector(out mysqlConn, "UPDATE characters SET exp = '"+pServer.getPlayerStats().exp+"' WHERE id = '"+pServer.getPlayerID()+"'");
			mysqlConn.Close();
			mysqlNonQuerySelector(out mysqlConn, "UPDATE characters SET level = '"+pServer.getPlayerStats().level+"' WHERE id = '"+pServer.getPlayerID()+"'");
			mysqlConn.Close();
			Debug.Log("disconnect server func " + pServer.getSkills().Length);
            */
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
		NetworkServer.Reset();
		questManager = new QuestManager(this);
        connectionString = "Server=" + host + ";Database=" + database + ";Uid=" + username + ";Pwd=" + password + ";";
        base.OnStartServer();
        NetworkServer.RegisterHandler(PacketTypes.LOAD_PLAYER, onLoadCharacter);
        NetworkServer.RegisterHandler(PacketTypes.LOGIN, onLogin);
        NetworkServer.RegisterHandler(PacketTypes.DISCONNECT, onDisconnect);
        NetworkServer.RegisterHandler(PacketTypes.SAVE_INVENTORY, onSaveInventory);
        NetworkServer.RegisterHandler(PacketTypes.INVENTORY_MOVE_ITEM, onChangeItem);
        NetworkServer.RegisterHandler(PacketTypes.INVENTORY_DROP_ITEM, onDropItem);
        NetworkServer.RegisterHandler(PacketTypes.PICKUP, onPickupItem);
        NetworkServer.RegisterHandler(PacketTypes.SEND_MESSAGE, onReciveMessage);
        NetworkServer.RegisterHandler(PacketTypes.NPC_INTERACT, onNPCInteract);
        NetworkServer.RegisterHandler(PacketTypes.PICK_CHAR, onCharPicked);
        NetworkServer.RegisterHandler(PacketTypes.VERIFY_SKILL, onVerifySkills);
		NetworkServer.RegisterHandler(PacketTypes.PLAYER_BUFF, onPlayerBuff);
        NetworkServer.RegisterHandler(PacketTypes.MONSTER_SPAWN, onMonsterSpawn);
        NetworkServer.RegisterHandler(PacketTypes.SPAWN_ITEM, onSpawnItem);
		NetworkServer.RegisterHandler(PacketTypes.QUEST_START, OnQuestRecieveFromClient);
		NetworkServer.RegisterHandler(PacketTypes.QUEST_TURN_IN, onQuestTurnInFromClient);
        NetworkServer.RegisterHandler(PacketTypes.DEAL_DAMAGE, onDealDamage);
        NetworkServer.RegisterHandler(PacketTypes.ITEM_USE, onUseItem);
        NetworkServer.RegisterHandler(PacketTypes.ITEM_UNEQUIP, onUnequipItem);
        NetworkServer.RegisterHandler(PacketTypes.ITEM_EQUIP, onEquipItem);
		NetworkServer.RegisterHandler(PacketTypes.DESTROY, onObjectDestroy);
		NetworkServer.RegisterHandler(PacketTypes.TEST, onTest);
		NetworkServer.RegisterHandler(PacketTypes.EMPTY, onLevelUp);
        NetworkServer.RegisterHandler(PacketTypes.CHARACTER_CREATE, onCharacterCreate);
		NetworkServer.RegisterHandler(PacketTypes.CREATE_SKILL, onSkillCreate);
		NetworkServer.RegisterHandler(PacketTypes.RESPAWN, onRespawn);
		NetworkServer.RegisterHandler(PacketTypes.GIVE_EXP, onGiveExp);
        
        //Create system objects
		ResourceStructure.initilize();
        Instantiate(ResourceStructure.getGameObjectFromObject(e_Objects.SYSTEM_RESPAWNER));

    }
    public void onCharacterCreate(NetworkMessage netMsg) {
        loadCharacters packet = netMsg.ReadMessage<loadCharacters>();
        int id = getPlayerIDFromDir(packet.playerName);
        Item[] equips = (Item[])Tools.byteArrayToObject(packet.itemsEquip);
        string[] color = (string[])Tools.byteArrayToObject(packet.colorScheme);

        MySqlConnection conn;

        mysqlNonQuerySelector(out conn, "INSERT INTO characters (characterName, accountID, health,maxHealth,mana,maxMana,money,level, skinColor,eyeColor) Values('"+packet.name+ "','"+ id + "','"+100+ "','"+100+ "','"+100+"','"+100+ "','"+100+ "','"+1+ "','"+ color[0]+ "','"+ color[1] + "')");
        int characterID = getCharacterID(packet.name);
        conn.Close();
        for (int i = 0; i < equips.Length; i++)
        {
            if (equips[i] == null) continue;
            Item item = loadBasicItem(equips[i].getID());
            int position = item.getID().getEquipPosition();
            mysqlNonQuerySelector(out conn, "INSERT INTO inventory (characterID, itemID, inventoryType, quantity, position) Values('"+ characterID + "','" + item.getID() + "','" + item.getInventoryType() + "','" + 1 + "','" + position + "')");
			conn.Close();
            int inventoryID = getInventoryID(item.getID(), position);
            mysqlNonQuerySelector(out conn, "INSERT INTO inventoryequipment (inventoryID, Watt, Matt, luk, str,dex,intell) Values('" + inventoryID + "','" + item.getVariables().getInt("Watt") + "','" + item.getVariables().getInt("Matt") + "','" + item.getVariables().getInt("luk") + "','" + item.getVariables().getInt("str") + "','" + item.getVariables().getInt("dex") + "','" + item.getVariables().getInt("Int") + "')");
			conn.Close();
        }
    }
	private void onRespawn(NetworkMessage netMsg){
		this.getPlayerObject(netMsg.conn.connectionId).getPlayerStats().health = this.getPlayerObject(netMsg.conn.connectionId).getPlayerStats().maxHealth;
	}
    private int getInventoryID(int id, int position) {
        MySqlConnection conn;
        MySqlDataReader reader;

        mysqlReader(out conn, out reader, "SELECT id FROM inventory WHERE itemID = '" + id + "' AND position = '"+position+"'");
        int invId = -1;
        while (reader.Read()) {
            invId = reader.GetInt32("id");
        }
        return invId;
    }
    private Item loadBasicItem(int id) {
        ItemVariables variables = ItemDataProvider.getInstance().getStats(id);
        Item item = new Item(id);
        item.setItemVariables(variables);
        return item;
    }
	public void onObjectDestroy(NetworkMessage netMsg){
		NetworkInstanceIdInfo info = new NetworkInstanceIdInfo();
		NetworkServer.Destroy(NetworkServer.FindLocalObject(info.netId));
	}
	public void onTest(NetworkMessage netMsg){
		Debug.Log("WEW U HAVE REACHED THE SERVER DSFDSFSDFSD");
	}

    public void onDealDamage(NetworkMessage netMsg)
    {
        DamageInfo damageInfo = netMsg.ReadMessage<DamageInfo>();
        //ClientScene.FindLocalObject(netMsg.conn);
        GameObject player = NetworkServer.FindLocalObject(damageInfo.clientNetworkInstanceId);
        GameObject enemy = NetworkServer.FindLocalObject(damageInfo.enemyNetworkInstanceId);

        PlayerServer pServer = playerObjects[netMsg.conn.connectionId];

		MobManager enemyMobManager = enemy.GetComponent<MobManager>();
		damageInfo.textPosition = new Vector3(enemy.transform.position.x, enemy.transform.position.y + (1 * (enemy.transform.lossyScale.y)), enemy.transform.position.z);


		if(damageInfo.damageType == e_DamageType.STRENGTH){
			int minAttackDamage = (pServer.getPlayerStats().s_dex/2) + pServer.getPlayerStats().s_watt;
			int maxAttackDamage = (pServer.getPlayerStats().s_str/2) + pServer.getPlayerStats().s_watt;
			int attackDamage = UnityEngine.Random.Range(minAttackDamage, minAttackDamage + maxAttackDamage);
			enemyMobManager.damage(attackDamage, player, pServer);
			damageInfo.damage = Mathf.CeilToInt(attackDamage * damageInfo.damageMultiplier);
		} else if(damageInfo.damageType == e_DamageType.MAGIC){
			int minMagicDamage = Mathf.Clamp((pServer.getPlayerStats().s_matt - pServer.getPlayerStats().s_int), 0, Tools.MAX_DAMAGE);
			int maxMagicDamage = (pServer.getPlayerStats().s_matt + pServer.getPlayerStats().s_int);
			int magicDamage = UnityEngine.Random.Range(minMagicDamage, minMagicDamage + maxMagicDamage);
			enemyMobManager.damage(magicDamage, player, pServer);
			damageInfo.damage = Mathf.CeilToInt(magicDamage * damageInfo.damageMultiplier);
		} else {
			Debug.LogError("Damage error Server.cs");
		}
		NetworkServer.SendToClient(netMsg.conn.connectionId, PacketTypes.DEAL_DAMAGE, damageInfo);
		enemyMobManager.setTarget(netMsg.conn.connectionId, player.gameObject);
    }
		
    public void OnQuestRecieveFromClient(NetworkMessage netMsg)
    {

		QuestInfo questInfo = netMsg.ReadMessage<QuestInfo>();
		Quest quest = (Quest)Tools.byteArrayToObject(questInfo.questClassInBytes);
		questManager.checkValidQuest(quest, netMsg.conn.connectionId, playerObjects[netMsg.conn.connectionId]);
    }

	public void onQuestTurnInFromClient(NetworkMessage netMsg){
		QuestInfo questInfo = netMsg.ReadMessage<QuestInfo>();
		Quest quest = (Quest)Tools.byteArrayToObject(questInfo.questClassInBytes);
		questManager.turnInQuest(quest, this.getPlayerObject(netMsg.conn.connectionId));
	}

	public static void giveExpToPlayer(int exp, PlayerServer pServer){
		KillInfo killInfo = new KillInfo();
		killInfo.exp = exp;
		pServer.giveExp(exp);
		NetworkServer.SendToClient(pServer.connectionID, PacketTypes.GIVE_EXP, killInfo);
	}
	public static void giveExpToPlayer(int exp, PlayerServer pServer, Vector3 position){
		KillInfo killInfo = new KillInfo();
		killInfo.exp = exp;
		pServer.giveExp(exp);
		killInfo.rewardTextPosition = position;
		NetworkServer.SendToClient(pServer.connectionID, PacketTypes.GIVE_EXP, killInfo);
	}
	public void onGiveExp(NetworkMessage netMsg){
		PlayerInfo info = netMsg.ReadMessage<PlayerInfo>();
		giveExpToPlayer(info.exp, playerObjects[netMsg.conn.connectionId], info.position);
	}


	public void onLevelUp(NetworkMessage netMsg){
		playerObjects[netMsg.conn.connectionId].levelUp();
	}

	public static void sendLevelUp(int expRequiredForNextLevel, int connectionID){
		LevelUpInfo info = new LevelUpInfo();
		info.expRequiredForNextLevel = expRequiredForNextLevel;
		NetworkServer.SendToClient(connectionID, PacketTypes.LEVEL_UP, info);
	}

	/*public void completeQuest(Quest quest){
		MySqlConnection conn;
		MySqlDataReader reader;
		int characterId = getCharacterIDFromDir(quest.getCharacterName());
		mysqlNonQuerySelector(out conn, "UPDATE queststatus SET status = '" + quest.getCompleted() + "' WHERE questID = '" + quest.getId() + "' AND characterID = '"+characterId+"'");
	}
		
	public void turnInQuest(Quest quest){
		MySqlConnection conn;
		MySqlDataReader reader;
		int characterId = getCharacterIDFromDir(quest.getCharacterName());
		mysqlReader(out conn, out reader, "SELECT id FROM queststatus WHERE characterID = '"+characterId+"'");
		mysqlNonQuerySelector(out conn, "DELETE FROM queststatusmobs WHERE queststatusID = '"+reader.GetInt32("id")+"'");

	}*/

	public bool addOrUpdateQuestStatusToDatabase(Quest quest, PlayerServer pServer, bool sendOutToClient, short type){
		MySqlConnection conn;
		MySqlDataReader reader;

		int connectionId = pServer.connectionID;
		int characterId = pServer.getPlayerID();

		mysqlReader(out conn, out reader, "SELECT questID, id FROM queststatus WHERE characterID = '"+characterId+"' AND questID = '"+quest.getId()+"'");

		int queststatusId = -1;
		int questID = -1;
		int status = -1;
		if(reader.Read()){
			//UPDATE
			queststatusId = reader.GetInt32("id");
			mysqlNonQuerySelector(out conn, "UPDATE queststatus SET status = '" + quest.getCompleted() + "' WHERE questID = '" + quest.getId() + "' AND characterID = '"+characterId+"'");
			conn.Close();
            
		} else {
			//INSERT
			mysqlNonQuerySelector(out conn, "INSERT INTO queststatus(questID, characterID, status) VALUES('"+quest.getId()+"', '"+characterId+"', '"+quest.getCompleted()+"')");
			conn.Close();
		}
		Debug.Log("TRYING TO ADD / UPDATE: " + characterId + " | " + quest.getCharacterName() + " | " + queststatusId);

		if(quest.getStatus() == e_QuestStatus.TURNED_IN){
			mysqlNonQuerySelector(out conn, "DELETE FROM queststatusmobs WHERE queststatusID = '"+queststatusId+"'");
			conn.Close();
			QuestInfo questInfo = new QuestInfo();
			questInfo.questClassInBytes = Tools.objectToByteArray(quest);
			NetworkServer.SendToClient(connectionId, type, questInfo);

			Debug.Log("DELETED QUEST");
			return true;
		}

		reader.Close();
		reader = null;
		if(queststatusId == -1){
			mysqlReader(out conn, out reader, "SELECT id FROM queststatus WHERE questID = '"+quest.getId()+"' AND characterID = '"+characterId+"'");
			if(reader.Read()) queststatusId = reader.GetInt32("id");
			conn.Close();
			reader.Close();
		}

		if(queststatusId != -1){
			if(reader == null){
				for(int i=0;i<quest.getMobIds().Length;i++){
					mysqlNonQuerySelector(out conn, "UPDATE queststatusmobs SET count = '" + quest.getMobKills(quest.getMobIds()[i]) + "' WHERE queststatusID = '" + queststatusId + "'");
					conn.Close();
				}
	        } else {
				for(int i=0;i<quest.getMobIds().Length;i++){
					Debug.Log("inserting quest to database..");
					mysqlNonQuerySelector(out conn, "INSERT INTO queststatusmobs(queststatusID, mob, count) VALUES('"+queststatusId+"', '"+quest.getMobIds()[i]+"', '"+quest.getMobKills(quest.getMobIds()[i])+"')");
					conn.Close();
				}
			}
		}

		if(sendOutToClient){
			QuestInfo questInfo = new QuestInfo();
			questInfo.questClassInBytes = Tools.objectToByteArray(quest);
			NetworkServer.SendToClient(connectionId, type, questInfo);
		}
		return true;
		conn.Close();
		reader.Close();
	}

	/*public Quest[] getQuestArrayFromPlayerServer(PlayerServer playerServer){
		MySqlConnection conn;
		MySqlDataReader reader;

		int characterID = getCharacterID(playerServer.playerName);
		mysqlReader(out conn, out reader, "SELECT questID FROM queststatus WHERE characterID = " + characterID);
		Debug.Log("CID: " + characterID);
		List<Quest> quests = new List<Quest>();
		while(reader.Read()){
			Debug.Log("I'm reading!");
			quests.Add();
			//Quest q = new Quest(reader.GetInt32("questID"), playerServer.playerName);
			//questManager.startQuest(q);
			//quests.Add(q);
		}
		return quests.ToArray();
	}*/

    private void onSpawnItem(NetworkMessage netMsg)
    {
        ItemInfo itemInfo = netMsg.ReadMessage<ItemInfo>();
		Item item = (Item)(Tools.byteArrayToObject(itemInfo.item));
		if(!item.isMoney()){
			playerObjects[netMsg.conn.connectionId].addItem(item);
			return;
		}
		playerObjects[netMsg.conn.connectionId].setMoney(item.getQuantity());
		NetworkServer.SendToClient(netMsg.conn.connectionId, PacketTypes.SPAWN_ITEM, itemInfo);
    }

    private void onMonsterSpawn(NetworkMessage netMsg)
    {
        MobInfo mobInfo = netMsg.ReadMessage<MobInfo>();
        for(int i=0;i<mobInfo.amount;i++){
			spawnMonster(mobInfo.mobId);
         }
    }

    public void onSkillCreate(NetworkMessage netMsg)
    {
		SkillCastInfo skillInfo = netMsg.ReadMessage<SkillCastInfo>();
		NetworkServer.SendToAll(PacketTypes.CREATE_SKILL, skillInfo);
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
		Monster monsters = JsonManager.readJson<Monster>(e_Paths.JSON_MONSTERS);
		//Monster monsters = JsonUtility.FromJson<Monster>(File.ReadAllText("Assets/XML/Monster.json"));
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
            Debug.Log("M_ID: " + monsterToFind.id);
			MobManager m = enemy.GetComponent<MobManager>();
			m.setId(monsterToFind.id);
            NetworkServer.Spawn(enemy);
        }
    }
    public static MobManager spawnMonster(int id, Vector3 pos)
    {
        Monster monsterToFind = getMonsterFromJson(id);
        if(monsterToFind != null){
            GameObject enemy = Instantiate(ResourceStructure.getGameObjectFromPath(monsterToFind.pathToModel));
            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
			MobManager m = enemy.GetComponent<MobManager>();
			m.setId(monsterToFind.id);

            if(agent){
                agent.Warp(pos);
            }

            NetworkServer.Spawn(enemy);
			return m;
        }
		return null;
    }
	/*public static MobManager spawnMonsterWithEffect(int id, Vector3 pos, e_Objects effect)
	{
		Monster monsterToFind = getMonsterFromJson(id);
		if(monsterToFind != null){
			GameObject enemy = Instantiate(ResourceStructure.getGameObjectFromPath(monsterToFind.pathToModel));
			NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
			MobManager m = enemy.GetComponent<MobManager>();
			m.setId(monsterToFind.id);

			if(agent){
				agent.Warp(pos);
			}

			NetworkServer.Spawn(enemy);
			MonsterInfo info = new MonsterInfo();
			info.position = enemy.transform.position;
			NetworkServer.SendToAll(PacketTypes.MONSTER_SPAWN, info);
			return m;
		}
		return null;
	}*/
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
		int playerID = getCharacterIDFromDir(skillInfo.playerName);

        //Read
        mysqlReader(out conn, out reader, "SELECT * FROM skills");
        int idFound = -1;
        while (reader.Read() && idFound == -1)
        {
            if (reader.GetInt32("characterID").Equals(playerID) && reader.GetInt32("skillId").Equals(skillInfo.id))
            {
                idFound = skillInfo.id;
                break;
            }
        }
        if (idFound != -1)
        {
            mysqlNonQuerySelector(out conn, "UPDATE skills SET skillId='" + idFound + "', currentPoints='" + skillInfo.currentPoints + "', maxPoints='" + skillInfo.maxPoints + "' WHERE characterID='" + playerID + "' AND skillId='" + idFound + "'");
			conn.Close();
        }
        else
        {
            mysqlNonQuerySelector(out conn, "INSERT INTO skills(characterID, skillId, currentPoints, maxPoints) VALUES('" + playerID + "', '" + skillInfo.id + "', '" + skillInfo.currentPoints + "', '" + skillInfo.maxPoints + "')");
			conn.Close();
        }
        //Finish
        NetworkServer.SendToClient(netMsg.conn.connectionId, PacketTypes.LOAD_SKILLS, skillInfo);
    }

    void onVerifySkills(NetworkMessage netMsg)
    {
        SkillInfo skillInfo = netMsg.ReadMessage<SkillInfo>();
        MySqlConnection conn;
        MySqlDataReader reader;
		int playerID = getCharacterIDFromDir(skillInfo.playerName);
		mysqlReader(out conn, out reader, "SELECT * FROM skills WHERE skillId='" + skillInfo.id + "' AND characterID = '"+playerID+"'");

        int skillIdFromDatabase = 0;
        int playerIdFromDatabase = 0;
        int maxPointsFromDatabase = 0;
        int results = 0;

        while (reader.Read())
        {
            results++;
            skillIdFromDatabase = reader.GetInt32("skillId");
            playerIdFromDatabase = reader.GetInt32("characterID");
            maxPointsFromDatabase = reader.GetInt32("maxPoints");
        }

        if (results > 0)
        {
            if (maxPointsFromDatabase > skillInfo.currentPoints)
            {
                mysqlNonQuerySelector(out conn, "UPDATE skills SET skillId='" + skillInfo.id + "', currentPoints='" + (skillInfo.currentPoints+1) + "', maxPoints='" + skillInfo.maxPoints + "' WHERE characterID='" + playerID + "' AND skillId='" + skillInfo.id + "'");
				conn.Close();
                NetworkServer.SendToClient(netMsg.conn.connectionId, PacketTypes.VERIFY_SKILL, skillInfo);
            }
            else
            {
                NetworkServer.SendToClient(netMsg.conn.connectionId, PacketTypes.ERROR_SKILL, skillInfo);
            }
        }
        else
        {
            mysqlNonQuerySelector(out conn, "INSERT INTO skills(characterID, skillId, currentPoints, maxPoints) VALUES('" + playerID + "', '" + skillInfo.id + "', '" + (skillInfo.currentPoints+1) + "', '" + skillInfo.maxPoints + "')");
			conn.Close();
            NetworkServer.SendToClient(netMsg.conn.connectionId, PacketTypes.VERIFY_SKILL, skillInfo);
        }
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



        //NPCInteractPacket senderObj = msg.ReadMessage<NPCInteractPacket>();
        //npc.startConversation(getPlayerObjectFromId(senderObj.playerInstanceId), npcCompiler.objManager);



        /*if (!npcActive.ContainsKey(senderObj.sender))
            npcActive.Add(senderObj.sender, npcCompiler.compileNPC(senderObj.npcID, msg.conn));

        NPCObject npc = npcActive[senderObj.sender];
        if (npc.Invoke(npcCompiler.objManager, senderObj.state))
        {
            npcActive.Remove(senderObj.sender);
        }
        */
    }
    PlayerServer loadCharacterInfoFromDatabase(PlayerServer player) {
        MySqlConnection conn;
        MySqlDataReader reader;
        mysqlReader(out conn, out reader, "SELECT * FROM characters WHERE characterName = '" + player.playerName + "'");
        Debug.Log("player name: !!!" + player.playerName);
        string hair = "";
        while (reader.Read()) {
            player.getPlayerStats().health = reader.GetInt16("health");
            player.getPlayerStats().maxHealth = reader.GetInt16("maxHealth");
            player.getPlayerStats().mana = reader.GetInt16("mana");
            player.getPlayerStats().maxMana = reader.GetInt16("maxMana");
            player.getPlayerStats().level = reader.GetInt16("level");
			player.getPlayerStats().exp = reader.GetInt16("exp");
            player.getPlayerStats().money = reader.GetInt16("money");
            player.getPlayerStats().expRequiredForNextLevel = ((player.getPlayerStats().level * 2))*10;
			Debug.Log("SERVER: EXP REQUIRED: " + player.getPlayerStats().expRequiredForNextLevel);


            player.getPlayerStats().s_luk = reader.GetInt16("luk");
            player.getPlayerStats().s_int = reader.GetInt16("intell");
            player.getPlayerStats().s_str = reader.GetInt16("str");
            player.getPlayerStats().s_dex = reader.GetInt16("dex");

            player.getPlayerStats().hairColor = reader.GetString("hairColor");
            player.getPlayerStats().eyeColor = reader.GetString("eyeColor");
            player.getPlayerStats().skinColor = reader.GetString("skinColor");
        }

		PlayerServer playerDatabase = getInventoryFromDatabase(player);
		player.items = playerDatabase.getItems();
		player.equips = playerDatabase.getEquips();
		conn.Close();
		reader.Close();
        return player;
    }
    void onLoadCharacter(NetworkMessage msg)
    {
        PlayerInfo packet = msg.ReadMessage<PlayerInfo>();
        int id = getCharacterID(packet.characterName);
        //PlayerServer player = getPlayerObject(msg.conn.connectionId);
        PlayerServer player = PlayerServer.GetDefaultCharacter(msg.conn.connectionId, packet.id);
        int characterID = getCharacterID(packet.characterName);
        PlayerServer playerReal = new PlayerServer(id, msg.conn.connectionId, packet.id);
        playerReal.setPlayerID(characterID);
		playerReal.netId = packet.id;

        connections.Add(msg.conn.connectionId, packet.name);
        conns.Add(packet.name, msg.conn.connectionId);
        charactersOnline.Add(packet.characterName, getCharacterID(packet.characterName));

        characterConnections.Add(msg.conn.connectionId, packet.characterName);
        playerReal.playerName = packet.characterName;
        player = loadCharacterInfoFromDatabase(playerReal);

		packet.items = Tools.objectToByteArray(player.getItems());
		packet.equipment = Tools.objectToByteArray(player.getEquips());

        playerObjects.Add(msg.conn.connectionId, playerReal);
        packet.stats = Tools.objectToByteArray(player.getPlayerStats());
        MySqlConnection conn;
        MySqlDataReader reader;

        mysqlReader(out conn, out reader, "SELECT * FROM skills WHERE characterID = '" + id + "'");
        List<int> skillProperties = new List<int>();
        while (reader.Read()) {
            skillProperties.Add(reader.GetInt16("skillId"));
            skillProperties.Add(reader.GetInt16("currentPoints"));
            skillProperties.Add(reader.GetInt16("maxPoints"));
        }
        packet.skillProperties = skillProperties.ToArray();
		conn.Close();
		reader.Close();

		//Quests
		mysqlReader(out conn, out reader, "SELECT * FROM queststatus WHERE characterID = '"+characterID+"'");

		while (reader.Read()) {
			Quest q = new Quest(reader.GetInt32("questID"), playerReal.playerName);
			q.getData().queststatusId = reader.GetInt32("id");
			questManager.startQuest(q);
			q.setStatus(q.intToQuestStatus(reader.GetInt32("status")));
			playerReal.questList.Add(q);
        }
		conn.Close();
		reader.Close();

		mysqlReader(out conn, out reader, "SELECT * FROM queststatusmobs");
        while (reader.Read()) {
            foreach(Quest q in playerReal.questList.ToArray()) {
				if(reader.GetInt32("queststatusID") == q.getData().queststatusId){
                    q.initilizeMobQuest(reader.GetInt32("mob"), reader.GetInt32("count"));
                }
            }
        }

		OtherPlayerInfo oInfo = new OtherPlayerInfo();
        List<string> color = new List<string>();
        color.Add(player.getPlayerStats().hairColor);
        color.Add(player.getPlayerStats().skinColor);
        color.Add(player.getPlayerStats().eyeColor);
        Debug.Log("color: " + color.Count + " : " + color[1] + " : " + color[2]);
        oInfo.equipment = packet.equipment;
        packet.color = Tools.objectToByteArray(color);
        PlayerNetworkObject[] objs = getAllPlayerInformation();
        packet.otherPlayers = Tools.objectToByteArray(objs);
        oInfo.color = packet.color;
        oInfo.id = packet.id;
		oInfo.characterName = playerReal.playerName;
		NetworkServer.SendToAll(PacketTypes.LOAD_OTHER_PLAYER, oInfo);

        packet.questClasses = Tools.objectArrayToByteArray(playerReal.questList.ToArray());
        NetworkServer.SendToClient(msg.conn.connectionId, PacketTypes.LOAD_PLAYER, packet);
		Debug.Log("loaded character on server, sending to client");
        conn.Close();
		reader.Close();

    }
    private PlayerNetworkObject[] getAllPlayerInformation() {
        PlayerNetworkObject[] equipments = new PlayerNetworkObject[playerObjects.Count];
        int i = 0;
        PlayerServer player;
        foreach (KeyValuePair<int, PlayerServer> entry in playerObjects)
        {
            player = entry.Value;
            List<Equip> equips = player.getEquips();
            List<int> item = new List<int>();
            for (int x = 0; x < equips.Count; x++) {
                if (equips[x] == null) { item.Add(-1); continue; }
                item.Add(equips[x].getID());
            }
            equipments[i] = new PlayerNetworkObject(player.netID, item,new string[] { player.getPlayerStats().hairColor, player.getPlayerStats().skinColor, player.getPlayerStats().eyeColor}, player.playerName);
            i++;
        }
        return equipments;
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

	private PlayerServer getInventoryFromDatabase() {
		return getInventoryFromDatabase(getPlayerObject(-1));
	}

    //# INVENTORY

	private PlayerServer getInventoryFromDatabase(int connectionId) {
		return getInventoryFromDatabase(getPlayerObject(connectionId));
    }
    private PlayerServer getInventoryFromDatabase(PlayerServer player)
    {

		int characterID = getCharacterIDFromDir(player.playerName);
        //string connectionString = "Server=" + host + ";Database=" + database + ";Uid=Gerry;Pwd=pass;";
        MySqlConnection conn;
        MySqlCommand cmd;
        MySqlDataReader reader;
        mysqlReader(out conn, out reader, "SELECT * FROM inventory LEFT JOIN inventoryEquipment ON inventory.id = inventoryEquipment.inventoryID WHERE characterID = '" + characterID + "'");
        while (reader.Read())
        {   
            int invType = reader.GetInt32("inventoryType");
            if (invType == (int)inventoryTabs.EQUIP)
            {
                int position = reader.GetInt32("position");
                Debug.Log("item psotions: " + position);
                    int[] item = new int[] {
                        reader.GetInt32("itemID"),
                        reader.GetInt32("Watt"),
                        reader.GetInt32("Matt"),
                        reader.GetInt32("str"),
                        reader.GetInt32("intell"),
                        reader.GetInt32("dex"),
                        reader.GetInt32("luk"),
                    };
                    if (position > (int)inventoryTabs.EQUIPPED)
						player.addItem(new Equip(reader.GetInt32("id"), position, invType, item));
                    else
						player.addEquip(new Equip(reader.GetInt32("id"), position, invType, item));
            }
            else
            {
                int[] item = new int[] {
                    reader.GetInt32("itemID"),
                };
                player.addItem(new Item(reader.GetInt32("id"), reader.GetInt32("position"), invType, reader.GetInt32("quantity"), item));
            }
        }
        conn.Close();
		reader.Close();
		return player;
    }

    void onUseItem(NetworkMessage netMsg) {
        //hämat spelar objektet
        PlayerServer player = netMsg.getPlayer();
        //hämtar paketet som kom med.
        ItemInfo info = netMsg.ReadMessage<ItemInfo>();
        //omvandlar byte arrayen till ett Item objekt som kom från spelaren.
        Item item = (Item)Tools.byteArrayToObject(info.item);
        info.oldItem = info.item;
        MySqlConnection conn;
        //kollar om spelaren har itemet i spelarens inventory på servern.
        if (!player.hasItem(item))
        {
            sendError(player, ErrorID.INVALID_ITEM, true, "changed item values");
        }
        ItemVariables vars = player.useItem(item);
        info.itemVariables = Tools.objectToByteArray(vars);
        //om spelaren har använt ett item och det finns noll kvar ska det tas bort från databasen. annars ska den minska med 1 i databasen.
        if (item.getQuantity() == 0)
        {
            player.removeItem(item);
            mysqlNonQuerySelector(out conn, "DELETE FROM inventory WHERE (quantity-1) < 1 AND id = '" + item.getKeyID() + "'");
			conn.Close();
        }
        else
        {
            mysqlNonQuerySelector(out conn, "UPDATE inventory SET quantity = '" + item.getQuantity() + "' WHERE  id = '" + item.getKeyID() + "'");
			conn.Close();
        }
        info.item = Tools.objectToByteArray(item);
        //skicka tillbaka det till spelaren.
        NetworkServer.SendToClient(netMsg.conn.connectionId, PacketTypes.ITEM_USE, info);
    }

    void onUnequipItem(NetworkMessage netMsg) {
        MySqlConnection con;
        ItemInfo info = netMsg.ReadMessage<ItemInfo>();
        Equip equip = (Equip)Tools.byteArrayToObject(info.item);
        //-((equip.getID() / Tools.ITEM_INTERVAL) - 3) räknar ut position som den ska ha i inventoryt.
        Debug.Log("INVENTORY EQUIP!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        mysqlNonQuerySelector(out con, "UPDATE inventory SET position = '" + equip.getPosition() + "' WHERE id = '" + equip.getKeyID() + "'");
        con.Close();
        NetworkServer.SendToAll(PacketTypes.ITEM_UNEQUIP, info);
    }
    void onEquipItem(NetworkMessage netMsg)
    {
        MySqlConnection con;
        ItemInfo info = netMsg.ReadMessage<ItemInfo>();
        Equip equip = (Equip)Tools.byteArrayToObject(info.item);
        Debug.Log("INVENTORY EQUIP!!!!!!!!!!!!!!!!!!!!!!!!!!! " + equip.getKeyID() + " | " + ((equip.getID() / Tools.ITEM_INTERVAL) * -1 + 1));
        //-((equip.getID() / Tools.ITEM_INTERVAL) - 3) räknar ut position som den ska ha i inventoryt.
        mysqlNonQuerySelector(out con, "UPDATE inventory SET position = '" + ((equip.getID() / Tools.ITEM_INTERVAL) * -1 + 1) + "' WHERE id = '" + equip.getKeyID() + "'");
        con.Close();
        NetworkServer.SendToAll(PacketTypes.ITEM_EQUIP, info);
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
        PlayerServer player = playerObjects[netMsg.conn.connectionId];

        Item item1 = (Item)Tools.byteArrayToObject(item.item1);
        Item item2 = (Item)Tools.byteArrayToObject(item.item2);

        if (player.hasItem(item1) || player.hasItem(item2)) {
            sendError(player,ErrorID.INVALID_ITEM, true, "invalid item in database");
        }

        //Debug.Log("wew from drop: " + item.name + " : " + sqlItemStatement(item.item1));
        mysqlReader(out conn, out reader, "SELECT * FROM inventory WHERE characterID = " + player.getPlayerID() + " AND " + sqlItemStatement(item1) + "");
		conn.Close();
		reader.Close();
        mysqlNonQuerySelector(out conn, "DELETE FROM inventory WHERE characterID = " + player.getPlayerID() + " AND " + sqlItemStatement(item2) + "");
        conn.Close();
        GameObject itemObject = spawnObject("Drop", new Vector3(item.position[0], item.position[1], item.position[2]));
    }

    public void onPickupItem(NetworkMessage netMsg)
    {
		MySqlConnection conn = null;
		MySqlDataReader reader;
        //moveItem moveItem = netMsg.ReadMessage<moveItem>();
		ItemInfo itemInfo = netMsg.ReadMessage<ItemInfo>();
		Item item = (Item)Tools.byteArrayToObject(itemInfo.item);
        Debug.Log("PICKUP!!!!!!!!!!!!!: " + item.isMoney() + " : " + item.getID().isItemType(e_itemTypes.EQUIP));
        PlayerServer player = getPlayerObject(netMsg.conn.connectionId);
        int playerID = player.getPlayerID();
        if (item.isMoney())
        {
            mysqlNonQuerySelector(out conn, "UPDATE characters SET money = '" + item.getQuantity() + "' WHERE id = '" + playerID + "'");
            NetworkServer.SendToClient(netMsg.conn.connectionId, PacketTypes.INVENTORY_PICKUP_ITEM, itemInfo);
            Debug.Log("MONEY ADDED!!! " + item.getQuantity());
            conn.Close();
            return;
        }
        int closest = player.getClosestSlot(item.getInventoryType());
        int id = 0;
        if (item.getID().isItemType(e_itemTypes.EQUIP))
        {
            Debug.Log("itemid: " + item.getID());
            ItemVariables stats = ItemDataProvider.getInstance().getStats(item.getID());
            item.setItemVariables(stats);
            Debug.Log("closest: " + closest);
            mysqlNonQuerySelector(out conn, "INSERT INTO inventory (characterID, itemID, inventoryType, quantity, position) VALUES ('" + player.getPlayerID() + "','" + item.getID() + "','" + item.getInventoryType() + "','" + item.getQuantity() + "','" + closest + "')");
            //int itemID =
            mysqlReader(out conn, out reader, "SELECT id FROM inventory WHERE position = '" + closest + "' AND inventoryType = '" + item.getInventoryType() + "'");
            id = 0;
            while (reader.Read()) {
                id = reader.GetInt32("id");
            }
            mysqlNonQuerySelector(out conn, "INSERT INTO inventoryequipment (inventoryID, Watt, Matt, luk, str, dex, intell) VALUES ('" + id + "','" + stats.getInt("Watt") + "','" + stats.getInt("Matt") + "','" + stats.getInt("luk") + "','" + stats.getInt("str") + "','" + stats.getInt("dex") + "','" + stats.getInt("Int") + "')");
        }
        else {
            mysqlNonQuerySelector(out conn, "INSERT INTO inventory (playerID, itemID, inventoryType, quantity, position) VALUES ('" + player.getPlayerID() + "','" + item.getID() + "','" + item.getInventoryType() + "','" + item.getQuantity() + "','" + closest + "')");
        }
        item.setPosition(closest);
        if (item.getID().isItemType(e_itemTypes.EQUIP)) {
            Item temp;
            temp = new Equip(id, item.getPosition(), item.getInventoryType(), item.stats);
            temp.setPosition(closest);
            temp.setItemVariables(item.getVariables());
            temp.stats = item.stats;
            temp.setID(item.getID());
            temp.setKeyID(id);
            item = temp;
        }
        player.items.Add(item);
        itemInfo.item = Tools.objectToByteArray(item);
        NetworkServer.SendToClient(netMsg.conn.connectionId,PacketTypes.PICKUP, itemInfo);
    }
    public void onChangeItem(NetworkMessage netMsg)
    {
        PlayerServer player = netMsg.getPlayer();
        moveItem item = netMsg.ReadMessage<moveItem>();
        MySqlConnection conn;
        Item item1 = (Item)Tools.byteArrayToObject(item.item1);
        Item item2 = (Item)Tools.byteArrayToObject(item.item2);
        Debug.Log("player connection: " + player.connectionID + " : " + netMsg.conn.connectionId);

        if ((!player.hasItem(item1) || !player.hasItem(item2)) && item2.getID() != -1) {
            Debug.Log(player.hasItem(item1) + " | " + player.hasItem(item2));
            sendError(player,ErrorID.INVALID_ITEM, true, "Item doesnt match database");
        }
        Debug.Log(item1.getKeyID() + " : " + item1.getPosition() + " | " + item2.getKeyID() + " : " + item2.getPosition());

        mysqlNonQuerySelector(out conn, "UPDATE inventory SET position = '" + item1.getPosition() + "' WHERE id = '" + item1.getKeyID() + "'");
        player.items[player.items.IndexOf(player.findItemWithKey(item1.getKeyID()))].setPosition(item1.getPosition());
		conn.Close();
        if (item2.getID() != -1)
        {
            mysqlNonQuerySelector(out conn, "UPDATE inventory SET position = '" + item2.getPosition() + "' WHERE id = '" + item2.getKeyID() + "'");
            player.items[player.items.IndexOf(player.findItemWithKey(item2.getKeyID()))].setPosition(item2.getPosition());
        }
        conn.Close();
    }


    private string sqlItemStatement(Item item)
    {
        return "itemID = '" + item.getStats()[0] + "' AND inventoryType = '" + item.getInventoryType() + "'";
    }

    //#PLAYER LOGIN

    void onLogin(NetworkMessage msg)
    {

        Debug.Log("message ID: " + msg.conn.connectionId);

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
            List<string[]> color = new List<string[]>();
            List<int[]> stats = new List<int[]>();
            List<List<Equip>> itemsEquip =new List<List<Equip>>();
            List<int> ids = new List<int>();
			mysqlConn.Close();
			reader.Close();

            mysqlReader(out mysqlConn, out reader, "SELECT * FROM characters WHERE accountID = '" + id + "'");
            while (reader.Read())
            {
                int[] stat = new int[6];
                string[] colors = new string[3];
                names.Add(reader.GetString("characterName"));

                stat[0] = -1;
                stat[1] = -1;
                stat[2] = -1;
                stat[3] = -1;
                stat[4] = -1;
                stat[5] = -1;

                colors[0] = (reader.GetString("hairColor"));
                colors[1] = (reader.GetString("eyeColor"));
                colors[2] = (reader.GetString("skinColor"));
                int idP = reader.GetInt32("id");
                ids.Add(idP);
                color.Add(colors);
                stats.Add(stat);
            }
            mysqlConn.Close();
            reader.Close();
            for (int i = 0; i < ids.Count; i++)
            {
				mysqlConn.Close();
				reader.Close();
                mysqlReader(out mysqlConn, out reader, "SELECT * FROM inventory LEFT JOIN inventoryEquipment ON inventory.id = inventoryEquipment.inventoryID WHERE characterID = '" + ids[i] + "' AND inventory.position <= " + (int)inventoryTabs.EQUIPPED + "");
                List<Equip> eqps = new List<Equip>();
                while (reader.Read())
                {
                    int invType = reader.GetInt32("inventoryType");
                    if (invType == (int)inventoryTabs.EQUIP)
                    {
                        int[] item = new int[] {
                        reader.GetInt32("itemID"),
                        reader.GetInt32("Watt"),
                        reader.GetInt32("Matt"),
                        reader.GetInt32("str"),
                        reader.GetInt32("intell"),
                        reader.GetInt32("dex"),
                        reader.GetInt32("luk"),
                    };
                        eqps.Add(new Equip(reader.GetInt32("id"), reader.GetInt32("position"), invType, item));
                    }
                }
                itemsEquip.Add(eqps);
                mysqlConn.Close();
                reader.Close();
            }
            pack.itemsEquip = Tools.objectToByteArray(itemsEquip);
            pack.stats = Tools.objectToByteArray(stats);
            pack.colorScheme = Tools.objectToByteArray(color);
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
    private Player getPlayerObjectFromId(NetworkInstanceId netId)
    {
        return (NetworkServer.FindLocalObject(netId)).GetComponent<Player>();
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
		reader.Close();
        return int.Parse(id);
    }

	private string getCharacterName(int id){
		//string connectionString = "Server=" + host + ";Database=" + database + ";Uid=Gerry;Pwd=pass;";
		MySqlConnection conn = new MySqlConnection(connectionString);
		MySqlCommand cmd = new MySqlCommand("SELECT characterName FROM characters WHERE id = '" + id + "'", conn);
		conn.Open();
		MySqlDataReader reader = cmd.ExecuteReader();
		string name = "";
		if (reader.Read())
		{
			name = reader.GetString("characterName");
		}
		conn.Close();
		return name;
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
		reader.Close();
        conn.Close();
        return id;
    }
    public PlayerServer getPlayerObject(int connectionID) {
        return playerObjects[connectionID];
    }
    private void sendError(PlayerServer player, ErrorID errorID, bool shouldKick, string message)
    {
        ErrorMessage error = new ErrorMessage();
        error.message = message;
        error.errorID = (int)errorID;
        error.shouldKick = shouldKick;
        //NetworkServer.SendToClient(player.connectionID, PacketTypes.ERROR, error);
    }
}

public class QuestStatusMobData
{
    public QuestStatusMobData(int questId, int mobId, int count)
    {
        this.questID = questId;
        this.mobID = mobId;
        this.count = count;
    }
    public int questID;
    public int mobID;
    public int count;
}
[Serializable]
public class PlayerNetworkObject
{
    public NetworkInstanceId netID;
    public List<int> equipsID;
    public string[] colors;
    public string name;
    public PlayerNetworkObject(NetworkInstanceId netID, List<int> equipsID, string[] colors, string name)
    {
        this.netID = netID;
        this.colors = colors;
        this.equipsID = equipsID;
        this.name = name;
    }
}