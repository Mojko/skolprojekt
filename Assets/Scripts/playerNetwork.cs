﻿using System.Collections;
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
		con.RegisterHandler(PacketTypes.INVENTORY_PICKUP_ITEM, onItemPickup);
        con.RegisterHandler(PacketTypes.ITEM_EQUIP, onEquip);
		con.RegisterHandler(PacketTypes.QUEST_COMPLETE, onQuestComplete);
        con.RegisterHandler(PacketTypes.ITEM_UNEQUIP, onUnequipItem);
        con.RegisterHandler(MsgType.Disconnect, OnDisconnectFromServer);
        sendPlayer (player.playerName, login.getCharacterName());

		//questUI = Tools.findInactiveChild(player.getUI(), "Quest_UI").GetComponent<QuestUI>();


        login.transform.parent.GetComponent<UIHandler>().removeThisFromParent();

        //this.onTalkNPC(5000, 0);

        Destroy(login.transform.parent.gameObject);
        Destroy(login_world);
    }

    /*public Item itemOnStandby;
	void onStandbyPickup(NetworkMessage netMsg){
		ItemInfo itemInfo = netMsg.ReadMessage<ItemInfo>();
		itemOnStandby = (Item)Tools.byteArrayToObject(itemInfo.item);
		this.player.pickupEventHandler += new Player.PickupEventHandler(onPickup);
		Debug.Log("added new standby event");
	}
	void onPickup(Item item){
		Debug.Log("PICKING UP ITEM: " + item.getID() + " | " + item.isMoney());
		if(item.isMoney()){
			this.player.money += item.getQuantity();
			return;
		}
	}*/
    void onUnequipItem(NetworkMessage msg) {
        ItemInfo info = msg.ReadMessage<ItemInfo>();
        if (info.netId.Equals(this.player.identity.netId))
        {
        }
        else
        {
            Player player = ClientScene.FindLocalObject(info.netId).GetComponent<Player>();
            player.setEquipModel((Item)Tools.byteArrayToObject(info.item));
            player.removeEquipModel((Item)Tools.byteArrayToObject(info.item));
        }
    }
    void onEquip(NetworkMessage msg) {
        ItemInfo info = msg.ReadMessage<ItemInfo>();
        if (info.netId.Equals(this.player.identity.netId)) {
        }
        else {
            Player player = ClientScene.FindLocalObject(info.netId).GetComponent<Player>();
            player.setEquipModel((Item)Tools.byteArrayToObject(info.item));
        }
    }

    void onItemPickup(NetworkMessage netMsg){
		ItemInfo itemInfo = netMsg.ReadMessage<ItemInfo>();
		Item item = (Item)Tools.byteArrayToObject(itemInfo.item);
		Debug.Log("PICKING UP ITEM: " + item + " | " + item.isMoney());
		if(item.isMoney()){
			this.player.money += item.getQuantity();
			return;
		}
	}

    void onItemUse(NetworkMessage netMsg) {
        ItemInfo info = netMsg.ReadMessage<ItemInfo>();
        Item item = (Item)Tools.byteArrayToObject(info.item);
        ItemVariables vars = (ItemVariables)Tools.byteArrayToObject(info.itemVariables);
        Item orgItem = (Item)Tools.byteArrayToObject(info.oldItem);
        if (item.getInventoryType() != (int)e_ItemTypes.EQUIP)
        {
            if (item.getID().isItemType(e_itemTypes.USE)) {
                onPotUse(vars);
            }
            if (item.getQuantity() == 0)
            {
                player.getInventory().removeItem(orgItem);
                return;
            }
            player.getInventory().updateItem(orgItem, item);
        }
    }
    void onPotUse(ItemVariables vars) {
        this.player.setHealth(Mathf.Min(this.player.stats.health + vars.getInt("health"), player.stats.maxHealth));
        this.player.setMana(Mathf.Min(this.player.stats.mana + vars.getInt("mana"), player.stats.maxMana));
    }
    public void damageEnemy(GameObject enemy, int damage, e_Objects impactEffect)
    {
        DamageInfo damageInfo = new DamageInfo();
        damageInfo.clientNetworkInstanceId = this.GetComponent<NetworkIdentity>().netId;
        damageInfo.enemyNetworkInstanceId = enemy.GetComponent<NetworkIdentity>().netId;
        damageInfo.damage = damage;
        damageInfo.damageType = e_DamageType.MOB;

        Vector3 pos = new Vector3(enemy.transform.position.x, enemy.transform.position.y+0.5f, enemy.transform.position.z);
        Instantiate(ResourceStructure.getGameObjectFromObject(impactEffect)).transform.position = pos;
        player.CmdSpawnGameObjectLocally(ResourceStructure.getPathForObject(impactEffect), pos);

        con.Send(PacketTypes.DEAL_DAMAGE, damageInfo);
    }

    public NetworkConnection getConnection()
    {
        return this.con;
    }

	public void onQuestStart(NetworkMessage netMsg){
		startQuest((Quest)Tools.byteArrayToObject(netMsg.ReadMessage<QuestInfo>().questClassInBytes));
		Debug.Log("Quest recieved from server");
	}

	public void startQuest(Quest quest){
		this.player.startNewQuest(quest);
		Debug.Log("STATUS_CLIENT: " + quest.getStatus());
		if(quest.getStatus() == e_QuestStatus.STARTED){
			this.player.getQuestInformationData().addNewQuestPanel(quest);
		}
		Debug.Log("QUEST STARTED");
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

	public void onQuestComplete(NetworkMessage netMsg){
		QuestInfo questInfo = netMsg.ReadMessage<QuestInfo>();
		Quest quest = (Quest)Tools.byteArrayToObject(questInfo.questClassInBytes);
		if(this.player.hasQuest(quest)){
			this.player.completeQuest(quest);
		}
	}

	public void sendQuestToServer(Quest quest)
    {
		QuestInfo questInfo = new QuestInfo();
		questInfo.questClassInBytes = Tools.objectToByteArray(quest);
		con.Send(PacketTypes.QUEST_START, questInfo);
		Debug.Log("QUEST SENT");    
	}

	public void loadQuests(PlayerInfo m){
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

	public void destroyGameObject(){

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
	public void sendSkillCast(string pathToEffect, Vector3 spawnPosition, Vector3 rotationInEuler, string type)
    {
		SkillCastInfo skillInfo = new SkillCastInfo();
		skillInfo.pathToEffect = pathToEffect;
		skillInfo.spawnPosition = spawnPosition;
		skillInfo.rotationInEuler = rotationInEuler;
		skillInfo.netId = this.GetComponent<NetworkIdentity>().netId;
		skillInfo.skillType = type;
		skillInfo.range = 10;
		Debug.Log("casting skill to server");
		con.Send(PacketTypes.CREATE_SKILL, skillInfo);
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
				player.stats.health += value;
				Debug.Log("You just got healed!");
			break;
			case e_StatType.MANA:
				player.stats.mana += value;
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

    void sendPlayer(string name, string characterName) {
        PlayerInfo msg = new PlayerInfo {
            name = name,
            characterName = characterName,
            id = this.gameObject.GetComponent<NetworkIdentity>().netId
        };
        Debug.Log("players name: " + PacketTypes.LOAD_PLAYER + " : " + con);
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
		Debug.Log("Character pre- loaded");
        PlayerInfo m = msg.ReadMessage<PlayerInfo>();
        PlayerStats stats = (PlayerStats)Tools.byteArrayToObject(m.stats);
        Debug.Log("log in from own packet!!!!!!!!!!");
        this.player.updateStats(stats);
        GameObject playerObj = ClientScene.FindLocalObject(m.id);
        Player player;
        if(playerObj != null){
            player = playerObj.GetComponent<Player>();
        } else {
            player = this.GetComponent<Player>();
        }


		Quest[] questArray = (Quest[])Tools.byteArrayToObjectArray(m.questClasses);
		QuestJson clientJson = JsonManager.readJson<QuestJson>(e_Paths.JSON_QUESTS);

		foreach(Quest quest in questArray){
			startQuest(quest);
		}
        
        GameObject[] objects = GameObject.FindGameObjectsWithTag("NPC");
        foreach(GameObject o in objects) {
            NPCMain main = o.GetComponent<NPCMain>();
            main.questMark.SetActive(false);
            int questCompletedCount = 0;
            for(int i=0;i<main.questIds.Length;i++){
                if (this.player.canTakeQuest(this.player.lookupQuest(main.questIds[i]))) {
                    main.questMark.SetActive(true);
                }
            }
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
		Debug.Log("Character loaded");
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
        info.netId = this.player.identity.netId;
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
