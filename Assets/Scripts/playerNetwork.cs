using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Xml;
using System.IO;
using UnityEngine.UI;

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
		con.RegisterHandler(PacketTypes.LOAD_OTHER_PLAYER, onClientConnect);
        con.RegisterHandler(PacketTypes.SEND_MESSAGE, onReciveMessage);
        con.RegisterHandler(PacketTypes.NPC_INTERACT, onReciveNPCText);
        con.RegisterHandler(PacketTypes.VERIFY_SKILL, onVerifySkill);
        con.RegisterHandler(PacketTypes.ERROR_SKILL, onErrorSkill);
		con.RegisterHandler(PacketTypes.PLAYER_BUFF, onPlayerBuff);
		con.RegisterHandler(PacketTypes.QUEST_START, onQuestStart);
		con.RegisterHandler(PacketTypes.QUEST_UPDATE, onQuestUpdate);
		con.RegisterHandler(PacketTypes.QUEST_TURN_IN, onQuestTurnIn);
        con.RegisterHandler(PacketTypes.ITEM_USE,onItemUse);
		con.RegisterHandler(PacketTypes.PICKUP, onItemPickup);
        con.RegisterHandler(PacketTypes.ITEM_EQUIP, onEquip);
		con.RegisterHandler(PacketTypes.QUEST_COMPLETE, onQuestComplete);
        con.RegisterHandler(PacketTypes.ITEM_UNEQUIP, onUnequipItem);
		con.RegisterHandler(PacketTypes.GIVE_EXP, onGiveExp);
		con.RegisterHandler(PacketTypes.LEVEL_UP, onLevelUp);
		con.RegisterHandler(PacketTypes.MONSTER_KILL, onMonsterKill);
		con.RegisterHandler(PacketTypes.CREATE_SKILL, onReturnSkill);
		con.RegisterHandler(PacketTypes.DEAL_DAMAGE, onDealDamage);
        con.RegisterHandler(MsgType.Disconnect, OnDisconnectFromServer);
		con.RegisterHandler(PacketTypes.ENEMY_SYNC_MOVEMENT, onEnemySyncMovement);
        sendPlayer (player.playerName, login.getCharacterName());

        login.transform.parent.GetComponent<UIHandler>().removeThisFromParent();

        Destroy(login.transform.parent.gameObject);
        Destroy(login_world);

		this.player.nameTag = Instantiate(this.player.nameTag);
		player.nameTag.transform.SetParent(GameObject.Find("WorldSpaceCanvas").transform);
		player.nameTag.GetComponent<Text>().text = this.player.playerName;
		player.nameTag.GetComponent<NameTag>().player = this.player;
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
	void onEnemySyncMovement(NetworkMessage netMsg){
		MonsterInfo mInfo = netMsg.ReadMessage<MonsterInfo>();
		GameObject o = ClientScene.FindLocalObject(mInfo.netId);
		o.transform.position = mInfo.position;
		o.transform.rotation = Quaternion.Euler(mInfo.rotation);
	}
	void onMonsterKill(NetworkMessage netMsg){
		MonsterKill monsterInfo = new MonsterKill();
		this.player.giveExp(monsterInfo.exp, monsterInfo.rewardTextPosition);

	}
	public void onLevelUp(NetworkMessage netMsg){
		this.player.levelUp(netMsg.ReadMessage<LevelUpInfo>().expRequiredForNextLevel);
	}
	public void sendLevelUp(int level){
		this.con.Send(PacketTypes.EMPTY, new EmptyInfo());
	}
	void onGiveExp(NetworkMessage netMsg){
		KillInfo info = netMsg.ReadMessage<KillInfo>();
		this.player.giveExp(info.exp, info.rewardTextPosition);
	}
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
        Debug.Log("IJTENJDSFGJSDIKJF GBHNSKJDHLFGBHNKHJSDNFGBJKHLSDFGB LJKHSDFG B");
        if (item.isMoney())
        {
            this.player.stats.money += item.getQuantity();
            return;
        }
        else {
            this.player.getInventory().addItem(item);
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
	public void damageEnemy(GameObject enemy, e_Objects impactEffect, e_DamageType type, float damageMultiplier)
    {
        DamageInfo damageInfo = new DamageInfo();
        damageInfo.clientNetworkInstanceId = this.GetComponent<NetworkIdentity>().netId;
        damageInfo.enemyNetworkInstanceId = enemy.GetComponent<NetworkIdentity>().netId;
		damageInfo.damageType = type;
		damageInfo.damageTarget = e_DamageTarget.MOB;
		damageInfo.damageMultiplier = damageMultiplier;

		Vector3 pos = new Vector3(enemy.transform.position.x, enemy.transform.position.y+0.5f, enemy.transform.position.z);
		GameObject o = Instantiate(ResourceStructure.getGameObjectFromObject(impactEffect));
		o.transform.position = pos;
		o.transform.localScale = new Vector3(enemy.transform.lossyScale.x*2, enemy.transform.lossyScale.y*2, enemy.transform.lossyScale.z*2);

        player.CmdSpawnGameObjectLocally(ResourceStructure.getPathForObject(impactEffect), pos);

        con.Send(PacketTypes.DEAL_DAMAGE, damageInfo);
    }

	public void sendExpToServer(int exp){
		PlayerInfo info = new PlayerInfo();
		info.exp = exp;
		con.Send(PacketTypes.GIVE_EXP, info);
	}

	private void onDealDamage(NetworkMessage netMsg){
		DamageInfo damageinfo = netMsg.ReadMessage<DamageInfo>();
		if(damageinfo.damageTarget == e_DamageTarget.PLAYER){
			this.player.damage(damageinfo.damage);
		} else if(damageinfo.damageTarget == e_DamageTarget.MOB || damageinfo.damageTarget == null) {
			this.player.UIPlayer.spawnActionText(damageinfo.textPosition, damageinfo.damage.ToString(), Color.red, Tools.TEXT_SPEED);
		}
	}

	public void sendRespawn(){
		this.con.Send(PacketTypes.RESPAWN, new PlayerInfo());
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
		if(quest.getStatus() != e_QuestStatus.NOT_STARTED){
			this.player.getQuestInformationData().addNewQuestPanel(quest);
		}
		Debug.Log("QUEST STARTED " + quest.getStatus());
	}

	public void onQuestUpdate(NetworkMessage netMsg){
		QuestInfo questInfo = netMsg.ReadMessage<QuestInfo>();

		Quest quest = (Quest)Tools.byteArrayToObject(questInfo.questClassInBytes);

		//Quest[] quests = (Quest[])Tools.byteArrayToObjectArray(questInfo.questClassInBytes);
		foreach(Quest playerQ in player.getQuests()){
			int[] ids = quest.getMobIds();
			for(int i=0;i<ids.Length;i++){
				playerQ.setMobKills(ids[i], quest.getMobKills(ids[i]));
			}
			foreach(QuestContainer container in player.getQuestWrapper().questContainers){
				container.setQuestInformation();
			}
		}
		Debug.Log("================QUEST UPDATED================");
	}

	public void onQuestTurnIn(NetworkMessage netMsg){
		QuestInfo questInfo = netMsg.ReadMessage<QuestInfo>();
		Quest quest = (Quest)Tools.byteArrayToObject(questInfo.questClassInBytes);
		Quest q = this.player.lookupQuest(quest.getId());
		q.setStatus(e_QuestStatus.TURNED_IN);
		this.player.npcTalkingTo.GetComponent<NPCMain>().onQuestTurnIn(q);
		this.player.getQuestInformationData().removeQuestPanel(q);
		this.sendExpToServer(quest.expReward);
	}

	public void onQuestComplete(NetworkMessage netMsg){
		QuestInfo questInfo = netMsg.ReadMessage<QuestInfo>();
		Quest quest = (Quest)Tools.byteArrayToObject(questInfo.questClassInBytes);
		if(this.player.hasQuest(quest)){
			this.player.completeQuest(quest);
		}
	}

	public void sendQuestToServer(Quest quest, short type)
    {
		QuestInfo questInfo = new QuestInfo();
		questInfo.questClassInBytes = Tools.objectToByteArray(quest);
		con.Send(type, questInfo);
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

	private GameObject spawnSkillEffect(SkillCastInfo skillcastInfo){
		GameObject skillEffect = Instantiate((GameObject)Resources.Load(skillcastInfo.pathToEffect));
		skillEffect.transform.position = skillcastInfo.targetPosition;
		skillEffect.transform.rotation = Quaternion.Euler(skillcastInfo.rotationInEuler);
		return skillEffect;
	}

	private void onReturnSkill(NetworkMessage netMsg){
		SkillCastInfo skillcastInfo = netMsg.ReadMessage<SkillCastInfo>();
		if(skillcastInfo.netId == this.GetComponent<NetworkIdentity>().netId) return;

		GameObject skillEffect = spawnSkillEffect(skillcastInfo);
		skillEffect.GetComponent<KillParticles>().fromServer = true;
		if(skillcastInfo.skillType.Equals("target")){
			StartCoroutine(updateSkillPosition(ClientScene.FindLocalObject(skillcastInfo.enemyNetId), skillEffect, skillcastInfo.offset));
		}

	}
	private IEnumerator updateSkillPosition(GameObject enemy, GameObject skill, Vector3 offset){
		while(enemy != null || skill != null){
			if(skill == null || enemy == null) yield break;
			skill.transform.position = new Vector3(enemy.transform.position.x+offset.x, enemy.transform.position.y+offset.y, enemy.transform.position.z+offset.z);
			yield return null;
		}
	}

	private void createSkill(string pathToEffect, Vector3 spawnPositionOffset, GameObject target, Vector3 rotationInEuler, string type, int range, Vector3 spawnPosition, MobManager m){
		SkillCastInfo skillInfo = new SkillCastInfo();
		skillInfo.pathToEffect = pathToEffect;
		skillInfo.offset = spawnPositionOffset;
		skillInfo.targetPosition = new Vector3(spawnPosition.x + spawnPositionOffset.x,spawnPosition.y + spawnPositionOffset.y, spawnPosition.z + spawnPositionOffset.z);//new Vector3(target.transform.position.x + spawnPositionOffset.x, target.transform.position.y + spawnPositionOffset.y, target.transform.position.z + spawnPositionOffset.z);
		skillInfo.rotationInEuler = rotationInEuler;
		skillInfo.netId = this.GetComponent<NetworkIdentity>().netId;
		skillInfo.skillType = type;
		skillInfo.range = range;
		GameObject o = spawnSkillEffect(skillInfo);
		o.transform.localScale = new Vector3(target.transform.lossyScale.x * 4, target.transform.lossyScale.y * 4, target.transform.lossyScale.z * 4);
		StartCoroutine(updateSkillPosition(target, o, spawnPositionOffset));
		if(target != null){
			skillInfo.enemyNetId = target.GetComponent<NetworkIdentity>().netId;
			o.GetComponent<SkillCastManager>().targetEntity = m;
			m.targeted = true;
		}
		con.Send(PacketTypes.CREATE_SKILL, skillInfo);
	}

	public void sendSkillCast(string pathToEffect, Vector3 relativePosition, Vector3 rotationInEuler, string type, int range, e_DamageType damageType, float damageMultiplier)
    {
		Collider[] targetColliders = Physics.OverlapSphere(this.transform.position, range);
		GameObject target = null;
		if(type.Equals("aoe")){
			createSkill(pathToEffect, relativePosition, null, rotationInEuler, type, range, this.transform.position, null);
		}
		foreach(Collider c in targetColliders){
			if(c.CompareTag("Enemy")){
				target = c.gameObject;
				MobManager m = target.GetComponent<MobManager>();
				if(type.Equals("target")){
					if(m.targeted) continue;
					damageEnemy(target, e_Objects.VFX_IMPACT_SKILL_MAGE_DEFUALT, damageType, damageMultiplier);
					createSkill(pathToEffect, relativePosition, target, rotationInEuler, type, range, target.transform.position, m);
					m.targeted = true;
					Debug.Log("found some enemies, targeting");
				}
				if(type.Equals("aoe")){
					damageEnemy(target, e_Objects.VFX_IMPACT_MELEE_1, damageType, damageMultiplier);
				}
			}
		}
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
	void onClientConnect(NetworkMessage msg) {
		OtherPlayerInfo info = msg.ReadMessage<OtherPlayerInfo>();
		if (!info.id.Equals(this.player.identity.netId)) {
			Player player = ClientScene.FindLocalObject(info.id).GetComponent<Player>();
			List<Equip> equips = (List<Equip>)Tools.byteArrayToObject(info.equipment);
            List<string> color = (List<string>)Tools.byteArrayToObject(info.color);
            player.setColor(color);
			player.playerName = info.characterName;
            foreach (Equip equip in equips)
			{
				if (equip == null) continue;
				player.setEquipModel(equip);
			}
			player.nameTag = (GameObject)Instantiate(Resources.Load("Prefabs/NameTag"));
			player.nameTag.transform.SetParent(GameObject.Find("WorldSpaceCanvas").transform);
			player.nameTag.GetComponent<Text>().text = info.characterName;
			this.player.otherPlayers.Add(player);
		}
	}
    private void equipOtherPlayers(PlayerNetworkObject[] playerObjects) {
        PlayerNetworkObject playerNetwork;
        NetworkInstanceId netID = this.GetComponent<NetworkIdentity>().netId;
        Debug.Log("length; " + playerObjects.Length);
        for (int i = 0; i < playerObjects.Length; i++) {
            playerNetwork = playerObjects[i];
            if (netID == playerNetwork.netID) continue;
            Player player = ClientScene.FindLocalObject(playerNetwork.netID).GetComponent<Player>();
            List<int> equips = playerNetwork.equipsID;
            player.setColor(new List<string>() { playerNetwork.colors[0], playerNetwork.colors[1], playerNetwork.colors[2] });
            player.playerName = playerNetwork.name;
            for (int j = 0; j < equips.Count; j++)
            {
                if (equips[j] == null) continue;
                player.setEquipModel(new Item(equips[j]));
            }
        }
    }
    void onLoadCharacter(NetworkMessage msg)
    {
		Debug.Log("Character pre- loaded");
        PlayerInfo m = msg.ReadMessage<PlayerInfo>();
        PlayerNetworkObject[] otherPlayers = (PlayerNetworkObject[])Tools.byteArrayToObject(m.otherPlayers);
        equipOtherPlayers(otherPlayers);
        PlayerStats stats = (PlayerStats)Tools.byteArrayToObject(m.stats);
		this.player.updateStats(stats);
		List<Equip> equipments = (List<Equip>)(Tools.byteArrayToObject(m.equipment));
		List<Item> inventory = (List<Item>)(Tools.byteArrayToObject(m.items));
        List<string> color = (List<string>)(Tools.byteArrayToObject(m.color));
        this.player.setInventory(inventory);
		this.player.setEquips(equipments);
        this.player.setColor(color);
		for (int i = 0; i < equipments.Count; i++) {
			if (equipments[i] == null) continue;
            if(equipments[i].getID().isItemType(e_itemTypes.HATS) || equipments[i].getID().isItemType(e_itemTypes.WEAPON))
			    this.player.setEquipModel(equipments[i]);
            else
                this.player.setClothesModel(equipments[i]);
        }
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
			main.setQuestMarker(this.player);
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
		foreach(Skill s in skillsToVerifyFromServer){
			this.player.stats.increment(s.str, s.dex, s.intell, 0,0,0,0,0,0,0,this.player.uiStats);
		}
		Debug.Log("Character loaded");
        skillTree = Instantiate (skillTreePrefab).GetComponent<SkillTree> ();
		skillTree.initilize (player);
		this.transform.position = new Vector3(-30, 0, 16);
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
		/*
        NPCInteractPacket obj = msg.ReadMessage<NPCInteractPacket>();
        NPC npc;
        Debug.Log("recived message");
        if ((npc = this.player.isTalkingToNpc()) != null)
        {
            npc.startDialogue(obj.npcText);
        }
        */
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
}
