using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Xml;
using System.IO;


public enum e_StatType {
	HEALTH,
	MANA,
	ARMOR,
	DAMAGE
}


public class playerNetwork : NetworkBehaviour{
	Player player;
	NetworkConnection con;

    public GameObject skillTreePrefab;
	public SkillTree skillTree;
	//public QuestUI questUI;

    bool characterLoaded = false;

	public void initialize(Player player){
		this.player = player;
        con = connectionToServer;
        Login login = GameObject.Find("Login").GetComponent<Login>();
        GameObject login_world = GameObject.Find("login_Word");
        this.player.playerName = login.getCharacterName();

        Debug.Log("name here m8 sk8 l8sdf sdf sdf sd fsdf sdf sdf sdf: " + this.player.playerName);
		con.RegisterHandler(PacketTypes.LOAD_PLAYER, onLoadCharacter);
		con.RegisterHandler (PacketTypes.LOAD_INVENTORY, onLoadInventory);
        con.RegisterHandler(PacketTypes.SEND_MESSAGE, onReciveMessage);
        con.RegisterHandler(PacketTypes.NPC_INTERACT, onReciveNPCText);
        con.RegisterHandler(PacketTypes.VERIFY_SKILL, onVerifySkill);
        con.RegisterHandler(PacketTypes.ERROR_SKILL, onErrorSkill);
		con.RegisterHandler(PacketTypes.PLAYER_BUFF, onPlayerBuff);
		con.RegisterHandler(PacketTypes.QUEST_START, onQuestStart);
		con.RegisterHandler(PacketTypes.QUEST_UPDATE, onQuestUpdate);
        con.RegisterHandler(PacketTypes.ITEM_USE,onItemUse);
        con.RegisterHandler(PacketTypes.ITEM_UNEQUIP, onUnEquip);
        con.RegisterHandler(PacketTypes.ITEM_EQUIP, onEquip);
        con.RegisterHandler(MsgType.Disconnect, OnDisconnectFromServer);
        sendPlayer (player.playerName, login.getCharacterName());

		//questUI = Tools.findInactiveChild(player.getUI(), "Quest_UI").GetComponent<QuestUI>();


        login.transform.parent.GetComponent<UIHandler>().removeThisFromParent();

        //this.onTalkNPC(5000, 0);

        Destroy(login.transform.parent.gameObject);
        Destroy(login_world);
    }
    void onItemUse(NetworkMessage netMsg) {
        ItemInfo info = netMsg.ReadMessage<ItemInfo>();
        Item item = (Item)Tools.byteArrayToObject(info.item);
        ItemVariables vars = (ItemVariables)Tools.byteArrayToObject(info.itemVariables);
        Item orgItem = item;
        if (item.getInventoryType() != (int)e_ItemTypes.EQUIP)
        {
            if (item.getID().isItemType(e_itemTypes.USE)) {
                onPotUse(vars);
            }
            if (item.getQuantity() == 0)
            {
                player.getInventory().removeItem(item);
                return;
            }
            item.setQuantity(item.getQuantity() - 1);
            player.getInventory().updateItem(orgItem, item);
        }
    }
    void onPotUse(ItemVariables vars) {
        this.player.setHealth(Mathf.Min(this.player.health + vars.getInt("health"), player.maxHealth));
        this.player.setMana(Mathf.Min(this.player.mana + vars.getInt("mana"), player.maxMana));
    }
    void onUnEquip(NetworkMessage netMsg)
    {

    }
    void onEquip(NetworkMessage netMsg)
    {

    }
    public void damageEnemy(GameObject enemy, int damage)
    {
        DamageInfo damageInfo = new DamageInfo();
        damageInfo.clientNetworkInstanceId = this.GetComponent<NetworkIdentity>().netId;
        damageInfo.enemyNetworkInstanceId = enemy.GetComponent<NetworkIdentity>().netId;

        //damageInfo.enemyUniqueId = enemy.GetComponent<MobManager>().getUniqueId();
        damageInfo.damage = damage;
        damageInfo.damageType = e_DamageType.MOB;
        con.Send(PacketTypes.DEAL_DAMAGE, damageInfo);
    }

    public NetworkConnection getConnection()
    {
        return this.con;
    }

	public void onQuestUpdate(NetworkMessage netMsg){
		QuestInfo questInfo = netMsg.ReadMessage<QuestInfo>();

		Quest[] quests = (Quest[])Tools.byteArrayToObjectArray(questInfo.questClassInBytes);

		//Quest[] quests = (Quest[])Tools.byteArrayToObjectArray(questInfo.questClassInBytes);
		foreach(Quest q in quests){
			foreach(Quest playerQ in player.getQuests()){
				int[] ids = q.getMobIds();
				for(int i=0;i<ids.Length;i++){
					playerQ.setMobKills(ids[i], q.getMobKills(ids[i]));
				}
				foreach(QuestContainer container in player.getQuestWrapper().questContainers.ToArray()){
					container.setQuestInformation();
				}
			}
            /*
             * 
             * 
                FIX THIS SHITTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTT
             */
                         /*
             * 
             * 
                FIX THIS SHITTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTT
             */
                         /*
             * 
             * 
                FIX THIS SHITTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTT
             */
            
            //this.questUI.addNewQuestToolTip(q.getTooltip());
		}
		Debug.Log("QUEST UPDATED");
	}

	public void onQuestStart(NetworkMessage netMsg){
		QuestInfo questInfo = netMsg.ReadMessage<QuestInfo>();
		Quest quest = (Quest)Tools.byteArrayToObject(questInfo.questClassInBytes);
		this.player.startNewQuest(quest);
		this.player.getQuestInformationData().addNewQuestPanel(quest);
        //this.questUI.addNewQuestPanel(quest);
		Debug.Log("QUEST STARTED");
	}

	public void sendQuestToServer(Quest quest)
    {
		QuestInfo questInfo = new QuestInfo();
		questInfo.questClassInBytes = Tools.objectToByteArray(quest);
		con.Send(PacketTypes.QUEST_START, questInfo);
		Debug.Log("QUEST SENT");    
	}

    /*public void sendNpcInteractionRequest(int npcId, int playerConnectionId, int state)
    {
        NPCInfo npcInfo = new NPCInfo();
        npcInfo.npcId = npcId;
        npcInfo.connectionId = playerConnectionId;
        npcInfo.state = state;
        con.Send(PacketTypes.NPC_INTERACT, npcInfo);
    }

    public void onNpcResponse(NetworkMessage netMsg)
    {
        NPCResponse response = netMsg.ReadMessage<NPCResponse>();
        Debug.Log("RESPONSE: " + Encoding.Default.GetString(response.textInBytes));
    }*/

    public void OnDisconnectFromServer(NetworkMessage netMsg) {
        if (isLocalPlayer) {
            player.reloadScene();
        }
    }

    //#Spawn monster
    public void spawnMobFromClient(int mobId, int amount)
    {
        MobInfo mobInfo = new MobInfo();
        mobInfo.mobId = mobId;
        mobInfo.amount = amount;
        con.Send(PacketTypes.MONSTER_SPAWN, mobInfo);
    }

    //#Skill
    public void sendProjectile(string path, string pathToEffect, Vector3 spawnPosition, Vector3 rotationInEuler)
    {
        ProjectTileInfo projectTileInfo = new ProjectTileInfo();
        projectTileInfo.pathToObject = path;
        projectTileInfo.pathToEffect = pathToEffect;
        projectTileInfo.spawnPosition = spawnPosition;
        projectTileInfo.rotationInEuler = rotationInEuler;
        con.Send(PacketTypes.PROJECTILE_CREATE, projectTileInfo);
    }

	//#STAT ALLOCATOR
	public void buffPlayer(string playerName, int value, e_StatType statType){
		StatInfo statInfo = new StatInfo();
		statInfo.value = value;
		statInfo.statType = statType;
		statInfo.playerName = playerName;
		con.Send(PacketTypes.PLAYER_BUFF, statInfo);
	}
	private void buff(string playerName, e_StatType statType, int value){
		GameObject playerObject = getPlayerObjectFromName(playerName);
		Player player = playerObject.GetComponent<Player>();
		switch(statType){
			case e_StatType.HEALTH:
				player.health += value;
				Debug.Log("You just got healed!");
			break;
			case e_StatType.MANA:
				player.mana += value;
				Debug.Log("You just got mana!");
			break;
			case e_StatType.DAMAGE:
			break;
			case e_StatType.ARMOR:
			break;
		}
	}
	private void onPlayerBuff(NetworkMessage netMsg){
		StatInfo statInfo = netMsg.ReadMessage<StatInfo>();
		buff(statInfo.playerName, statInfo.statType, statInfo.value);
	}

	public GameObject getPlayerObjectFromName(string name){

		GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");

		foreach(GameObject o in playerObjects){
			Debug.Log("NAME: " + o.name);
			if(o.GetComponent<Player>().playerName.Equals(name)){
				return o;
			}
		}
		return null;
	}
	//#END OF STAT ALLOCATOR



    // # CHAT
    void onReciveMessage(NetworkMessage msg) {
        PacketMessage message = msg.ReadMessage<PacketMessage>();
        string chatMessage = message.sender + ": " + message.message;
        this.player.getChat().addMessage(chatMessage);
    }


	//#Skills
	void onVerifySkill(NetworkMessage netMsg){
		SkillInfo skillInfo = netMsg.ReadMessage<SkillInfo>();
		this.skillTree.confirmSkills();
		Debug.Log("Skills verified");
	}
	void onErrorSkill(NetworkMessage netMsg){
		Debug.Log("Trying to add more skill points while full!");
	}

	public void sendSkills(Skill skill){
		con = connectionToServer;
		SkillInfo msg = new SkillInfo();

		msg.id = skill.id;
		msg.playerName = this.player.playerName;
		msg.currentPoints = skill.currentPoints;
		msg.maxPoints = skill.maxPoints;
		con.Send(PacketTypes.SAVE_SKILLS, msg);
	}

	public void sendSkill(GameObject skillObject){
		con = connectionToServer;
		SkillInfo msg = new SkillInfo();

		SkillId skill = skillObject.GetComponent<SkillId>();

		msg.id = skill.id;
        Debug.Log("PLAYER: " + this.player);
		msg.playerName = this.player.playerName;
		msg.currentPoints = skill.points;
		msg.maxPoints = skill.maxPoints;
		con.Send(PacketTypes.VERIFY_SKILL, msg);
	}

	void onLoadSkills(NetworkMessage msg){
		Debug.Log("skills loaded");
    }

    //# PLAYER

	void sendPlayer(string name, string characterName){
		PlayerInfo msg = new PlayerInfo();
		msg.name = name;
        msg.characterName = characterName;
        msg.id = this.gameObject.GetComponent<NetworkIdentity>().netId;
        Debug.Log("players name: " + msg.name);
		con.Send(PacketTypes.LOAD_PLAYER, msg);
	}
    private void OnDisconnect()
    {
        Debug.Log("disconnected");
        DisconnectPacket packet = new DisconnectPacket();
        packet.name = player.playerName;
        List<byte[]> questList = (List<byte[]>)Tools.byteArrayToObject(packet.questListInBytes);
        foreach(Quest q in player.getQuests()) { 
            questList.Add(Tools.objectToByteArray(q));
        }
        con.Send(PacketTypes.DISCONNECT, packet);
    }
    void onLoadCharacter(NetworkMessage msg)
    {
        PlayerInfo m = msg.ReadMessage<PlayerInfo>();
        GameObject playerObj = ClientScene.FindLocalObject(m.id);
        Player player;
        if(playerObj != null){
            player = playerObj.GetComponent<Player>();
        } else {
            player = this.GetComponent<Player>();
        }

		Quest[] questArray = (Quest[])Tools.byteArrayToObjectArray(m.questClasses);
		QuestJson clientJson = JsonManager.readJson<QuestJson>(e_Paths.JSON_QUESTS);
		//QuestJson clientJson = JsonUtility.FromJson<QuestJson> (File.ReadAllText("Assets/XML/Quests.json"));

		Debug.Log("CLIENTJSON: " + clientJson.quests.Length);
		this.sendMessage("acess0? " + clientJson.quests.Length, MessageTypes.CHAT, "");

		foreach(QuestJson quests in clientJson.quests){
			//this.sendMessage("acess1? " + quests.id + " | " + clientJson.quests.Length, MessageTypes.CHAT, "");
			foreach(Quest quest in questArray){
				//this.sendMessage("acess? " + quest.getId() + " | " + quests.id + " | " + clientJson.quests.Length, MessageTypes.EVERYONE, "");
				Debug.Log("acess here? " + quest.getId() + " | " + quests.id);
				if(quest.getId() == quests.id){
					player.quests.Add(quest);
					this.player.getQuestInformationData().addNewQuestPanel(quest);
                    //this.questUI.addNewQuestPanel(quest);
				}
			}
			/*for(int i=0;i<m.questIds.Length;i++){
				if(m.questIds[i] == quests.id){
					Quest q = new Quest(m.questIds[i], quests.name);
					player.quests.Add(q);
					this.questUI.addNewQuestPanel(q);
				}
			}*/
		}
		//Initilize skill tree
        List<Skill> skillsToVerifyFromServer = new List<Skill>();
        for(int i=2;i<m.skillProperties.Length;i += 3){
            Skill tempSkill = new Skill();
            tempSkill.id = m.skillProperties[i-2]; //skillId
            tempSkill.currentPoints = m.skillProperties[i-1]; //currentPoints
            tempSkill.maxPoints = m.skillProperties[i]; //maxPoints
            this.player.skillsToVerifyWithFromServer.Add(tempSkill);
        }
        Debug.Log("SKILLTREE INITILIZED");
        skillTree = Instantiate (skillTreePrefab).GetComponent<SkillTree> ();
		skillTree.initilize (player);
    }
   
    public void onTalkNPC(int npcID, int state)
    {
        NPCInteractPacket sendObj = new NPCInteractPacket();
        sendObj.sender = this.player.playerName;
        sendObj.npcID = npcID;
        sendObj.state = state;
        sendObj.playerInstanceId = this.player.GetComponent<NetworkIdentity>().netId;
        Debug.Log("sent message: " + this.player.playerName);
        //con.Send(PacketTypes.NPC_INTERACT, sendObj);
    }
    void onReciveNPCText(NetworkMessage msg)
    {
        NPCInteractPacket obj = msg.ReadMessage<NPCInteractPacket>();
        NPC npc;
        Debug.Log("recived message");
        if ((npc = this.player.isTalkingToNpc()) != null)
        {
            npc.startDialogue(obj.npcText);
        }
    }
    //# INVENTORY
    public void sendItem(short packetType, Item item) {
        ItemInfo info = new ItemInfo();
        info.item = Tools.objectToByteArray(item);
        con.Send(packetType, info);
    }
    public void unEquipItem(Equip equip) {
        sendItem(PacketTypes.ITEM_UNEQUIP, equip);
    }
    public void equipItem(Equip equip) {
        sendItem(PacketTypes.ITEM_EQUIP, equip);
    }
    public void onItemUse(Item item) {
        sendItem(PacketTypes.ITEM_USE, item);
    }
    public void sendInventory(Inventory invetory){
		con = connectionToServer;
		//con.RegisterHandler (PacketTypes.SAVE_INVENTORY, OnSaveInventory);
		InventoryInfo msg = new InventoryInfo ();
		msg.id = this.gameObject.GetComponent<NetworkIdentity> ().netId;
		//Item[] items = invetory.getItems();
		msg.name = player.playerName;
		con.Send (PacketTypes.SAVE_INVENTORY, msg);
	}
    public void sendItem(Item item)
    {
        ItemInfo itemInfo = new ItemInfo();
        itemInfo.item = Tools.objectToByteArray(item);
        con.Send(PacketTypes.SPAWN_ITEM, itemInfo);
    }
    public void moveItem(Item itemMoved, Item itemReplaced, short packetType, Player player) {
        moveItem item = new moveItem();
        item.item1 = Tools.objectToByteArray(itemMoved);
        item.item2 = Tools.objectToByteArray(itemReplaced);
        item.player = player.playerName;
        //item.position = new float[] { player.transform.position.x, player.transform.position.y, player.transform.position.z};
        con.Send(packetType, item);
    }
	public void loadInventory(){
		InventoryInfo msg = new InventoryInfo ();
		msg.name = player.playerName;
        con.Send (PacketTypes.LOAD_INVENTORY, msg);
	}
    public void sendMessage(string message, MessageTypes type, string reciver = "") {
        PacketMessage msg = new PacketMessage();
        msg.sender = this.player.playerName;
        msg.reciver = reciver;
        msg.message = message;
        msg.type = type;
        con.Send(PacketTypes.SEND_MESSAGE, msg);

        if (message.StartsWith("/")) {
            this.player.getCommandManager().listenForCommand(message);
        }
    }

    //körs när spelaren har fått tillbaka sitt inventory från servern. 
	void onLoadInventory(NetworkMessage msg){
        InventoryInfo info = msg.ReadMessage<InventoryInfo>();
        List<Equip> equipments = (List<Equip>)(Tools.byteArrayToObject(info.equipment));
        List<Item> inventory = (List<Item>)(Tools.byteArrayToObject(info.items));
        Debug.Log("equipment: " + equipments.Count);
        player.setInventory (inventory);
        player.setEquips(equipments);
        Debug.Log ("inventory loaded");
        Debug.Log(info.items.Length);
	}
}
