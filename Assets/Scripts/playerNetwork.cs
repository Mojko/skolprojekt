using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


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
        con.RegisterHandler(MsgType.Disconnect, OnDisconnectFromServer);
        sendPlayer (player.playerName, login.getCharacterName());

        login.transform.parent.GetComponent<UIHandler>().removeThisFromParent();

        Destroy(login.transform.parent.gameObject);
        Destroy(login_world);
    }

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
		msg.id = this.gameObject.GetComponent<NetworkIdentity>().netId;
		msg.name = name;
        msg.characterName = characterName;
        Debug.Log("players name: " + msg.name);
		con.Send(PacketTypes.LOAD_PLAYER, msg);
	}
    private void OnDisconnect()
    {
        Debug.Log("disconnected");
        DisconnectPacket packet = new DisconnectPacket();
        packet.name = player.playerName;
        con.Send(PacketTypes.DISCONNECT, packet);
    }
    void onLoadCharacter(NetworkMessage msg)
    {
        PlayerInfo m = msg.ReadMessage<PlayerInfo>();
        GameObject playerObj = ClientScene.FindLocalObject(m.id);
        Player player = playerObj.GetComponent<Player>();

		//Initilize skill tree
        List<Skill> skillsToVerifyFromServer = new List<Skill>();
        for(int i=2;i<m.skillProperties.Length;i += 3){
            Skill tempSkill = new Skill();
            tempSkill.id = m.skillProperties[i-2]; //skillId
            tempSkill.currentPoints = m.skillProperties[i-1]; //currentPoints
            tempSkill.maxPoints = m.skillProperties[i]; //maxPoints
            this.player.skillsToVerifyWithFromServer.Add(tempSkill);
        }
        skillTree = Instantiate (skillTreePrefab).GetComponent<SkillTree> ();
		skillTree.initilize (player);
    }
    public void onTalkNPC(int npcID, int state)
    {
        NPCInteractPacket sendObj = new NPCInteractPacket();
        sendObj.sender = this.player.playerName;
        sendObj.npcID = npcID;
        sendObj.state = state;
        Debug.Log("sent message: " + this.player.playerName);
        con.Send(PacketTypes.NPC_INTERACT, sendObj);
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

    public void sendInventory(Inventory invetory){
		con = connectionToServer;
		//con.RegisterHandler (PacketTypes.SAVE_INVENTORY, OnSaveInventory);
		InventoryInfo msg = new InventoryInfo ();
		msg.id = this.gameObject.GetComponent<NetworkIdentity> ().netId;
		Item[] items = invetory.getItems();
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
