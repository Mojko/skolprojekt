using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
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

        Debug.Log("name here m8: " + this.player.playerName);
		con.RegisterHandler(PacketTypes.LOAD_PLAYER, onLoadCharacter);
		con.RegisterHandler (PacketTypes.LOAD_INVENTORY, onLoadInventory);
        con.RegisterHandler(PacketTypes.SEND_MESSAGE, onReciveMessage);
        con.RegisterHandler(PacketTypes.NPC_INTERACT, onReciveNPCText);
        con.RegisterHandler(PacketTypes.VERIFY_SKILL, onVerifySkill);
        con.RegisterHandler(PacketTypes.ERROR_SKILL, onErrorSkill);
        sendPlayer (player.playerName, login.getCharacterName());
        Destroy(login.transform.parent.gameObject);
        Destroy(login_world);
    }

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
		Debug.LogError("Trying to add more skill points while full!");
	}

	public void sendSkills(Skill skill){
		con = connectionToServer;
		SkillInfo msg = new SkillInfo();
		Debug.Log("Skills sent");

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
		msg.items = invetory.itemsToArray (items);
		msg.name = player.playerName;
		con.Send (PacketTypes.SAVE_INVENTORY, msg);
	}
    public void moveItem(int[] itemMoved, int[] itemReplaced, short packetType, Player player) {
        moveItem item = new moveItem();
        item.item1 = itemMoved;
        item.item2 = itemReplaced;
        item.player = player.playerName;
        item.position = new float[] { player.transform.position.x, player.transform.position.y, player.transform.position.z};
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
    }
	void onLoadInventory(NetworkMessage msg){
        InventoryInfo info = msg.ReadMessage<InventoryInfo>();
        player.setInventory (info.items);
        Debug.Log ("inventory loaded");
        Debug.Log(info.items.Length);
	}
}
