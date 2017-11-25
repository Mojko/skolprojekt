using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{

    //Private variables
    private Inventory inventory;
    private GameObject UICanvas;
    private NPC currentNPC;
    private SkillUIManager skillUi;
    private SkillManager skillManager;
    private PlayerMovement movement;
    private CommandManager commandManager;
    private Login login;
    private GameObject[] worlds = new GameObject[2];
    private NetworkConnection con;
    private EquipmentHandler equip;
    private QuestInformationData questInformationData;
    private GameObject questInformationObject;
	private QuestWrapper questWrapper;
	private GameObject npcManager;
	private UIPlayerHandler UIPlayer;
	public delegate void PickupEventHandler(Item item);
	public event PickupEventHandler pickupEventHandler;

    [Header("Player Attributes")]
    public PlayerStats stats;
    public string playerName;
    public int money = 0;


    [Space(20)]
    [Header("Quests")]
    public List<Quest> quests = new List<Quest>();
    public GameObject quest_UI;

    [Space(20)]
    [Header("Skills")]
    public GameObject skillPrefab;
    public GameObject skillEffectPrefab;
    public GameObject skillTreeUi;
    public List<Skill> skills = new List<Skill>();
    public List<Skill> skillsToVerifyWithFromServer = new List<Skill>();

    [Space(20)]
    [Header("System")]

    public Camera camera;
	public playerNetwork network;
	public GameObject npcTalkingTo;
    public Chat chat;
    public GameObject[] prefabsToRegister;

    [Space(20)]
    [Header("Leave these alone")]
    public NPCMain npcMain;

    public override void OnStartLocalPlayer ()
	{
        if(!isLocalPlayer) return;
        commandManager = new CommandManager(this);
		for (int i = 0; i < prefabsToRegister.Length; i++) {
			ClientScene.RegisterPrefab (prefabsToRegister [i]);
		}
        /*
        ClientScene.RegisterPrefab((GameObject)Resources.Load("Prefabs/Enemy"));
        ClientScene.RegisterPrefab((GameObject)Resources.Load("Particles/Hit"));
        ClientScene.RegisterPrefab((GameObject)Resources.Load("Particles/ImpactOnGround"));
        ClientScene.RegisterPrefab(skillPrefab);
        ClientScene.RegisterPrefab(skillEffectPrefab);*/
        //UI
        UICanvas = GameObject.Find("UI");
        login = Tools.findInactiveChild(UICanvas,"Login_UI").GetComponent<Login>();
        this.movement = GetComponent<PlayerMovement>();
        //skills
        this.skillUi = GameObject.Find("Actionbar_UI").GetComponent<SkillUIManager>();
        this.skillUi.init(this);
        this.skillManager = GetComponent<SkillManager>();
		this.skillManager.init(this, getSkillUiManager());

        //this.skillTreeUi = GameObject.Find(this.skillTreeUi.name)

        //network
		network = GetComponent<playerNetwork>();
        network.initialize (this);

        this.skillTreeUi = Tools.getChild(UICanvas, this.skillTreeUi.name);

        //chat
        chat = Tools.getChild(UICanvas, "Chat_UI").GetComponent<Chat>();
        chat.setPlayer(this);

        //inventory
		this.inventory = this.GetComponent<Inventory> ();
		this.inventory.init (this);

        //equip
        equip = Tools.getChild(UICanvas, "Equipment_UI").GetComponent<EquipmentHandler>();
        equip.setEquipmentUI(Tools.getChild(UICanvas, "Equipment_UI"));
        equip.setPlayer(this);
        equip.setPlayerSlots(this);
        //camera
        camera = Camera.main;
        camera.GetComponent<MainCamera> ().player = this.gameObject;
		camera.GetComponent<MainCamera> ().setState ((int)e_cameraStates.DEFAULT);

        //worlds
        worlds[0] = GameObject.Find("login_World");
        worlds[1] = GameObject.Find("World");


        //TempQuestUI
        GameObject tempQuestUI = Instantiate(this.quest_UI);
        this.questInformationData = tempQuestUI.GetComponentInChildren<QuestInformationData>();
        tempQuestUI.transform.SetParent(this.getUI().transform);
        tempQuestUI.transform.SetAsLastSibling();
        questInformationObject = tempQuestUI;

		//QuestWrapper
		questWrapper = getUI().transform.GetChild(getUI().transform.childCount-1).GetChild(1).GetChild(0).GetComponent<QuestWrapper>();

        //UIPlayer
        this.UIPlayer = Tools.findInactiveChild(UICanvas, "Footer_UI").GetComponent<UIPlayerHandler>();
        this.UIPlayer.setPlayer(this);
        this.UIPlayer.gameObject.SetActive(true);

		//QuestManager
		this.npcManager = GameObject.FindWithTag("NPCManager");
		npcManager.GetComponent<NPCController>().initilize(this);

    }
	public bool hasCompletedQuest(Quest quest){
		foreach(Quest q in quests.ToArray()){
			if(q.getStatus() == e_QuestStatus.COMPLETED){
				return true;
			}
		}
		return false;
	}
    public bool hasStartedQuest(Quest quest)
    {
        if(quest.getStatus() == e_QuestStatus.STARTED) {
            return true;
        }
        return false;
    }
    public bool canTakeQuest(Quest quest)
    {
        return (quest == null || quest.getStatus() == e_QuestStatus.NOT_STARTED);
    }
    public Quest lookupQuest(int questId)
    {
        foreach(Quest quest in this.quests) {
            if(quest.getId() == questId) {
                return quest;
            }
        }
        return null;
    }
	public bool hasQuest(Quest quest){
		foreach(Quest q in quests.ToArray()){
			if(quest.getId().Equals(q.getId())){
				return true;
			}
		}
		return false;
	}
	public void completeQuest(Quest quest){
		this.quests.Remove(quest);
		this.getQuestInformationData().removeQuestPanel(quest);
		Debug.Log("Quest removed AND completed");
	}
	public GameObject getNpcManager(){
		return this.npcManager;
	}
	public QuestWrapper getQuestWrapper(){
		return this.questWrapper;
	}
	public QuestInformationData getQuestInformationData(){
        return this.questInformationData;
	}
    public GameObject getQuestInformationObject()
    {
        return this.questInformationObject;
    }
	public Quest[] getQuests(){
		return this.quests.ToArray();
	}
	public void startNewQuest(Quest quest){
		this.quests.Add(quest);
	}
	public string getCharacterName(){
		return this.playerName;
	}
    public EquipmentHandler getEquipHandler() {
        return equip;
    }
    public CommandManager getCommandManager()
    {
        return this.commandManager;
    }
    public GameObject getSkillTreeUi()
    {
        return this.skillTreeUi;
    }
	public Animator getAnimator(){
		return getPlayerMovement().animator;
	}
    public PlayerMovement getPlayerMovement()
    {
        return this.movement;
    }
    public SkillUIManager getSkillUiManager()
    {
        return this.skillUi;
    }
    public SkillManager getSkillManager()
    {
        return this.skillManager;
    }
    public NPC isTalkingToNpc() {
        return currentNPC;
    }
    public void setActiveNPC(NPC npc) {
        currentNPC = npc;
    }
    public GameObject getUI() {
        return UICanvas;
    }
	public playerNetwork getNetwork(){
		return network;
	}
	public void setInventory(List<Item> items){
		inventory.setInventory (items);
	}
    public Inventory getInventory() {
        return inventory;
    }
    public void reloadScene() {
        inventory.clearInventory();
        inventory.gameObject.SetActive(false);
        equip.clear();
        equip.gameObject.SetActive(false);
        chat.clear();
        login.gameObject.SetActive(true);
        worlds[0].SetActive(true);
        worlds[1].SetActive(false);

        Debug.Log("reloaded scene.");
    }
    public void setEquips(List<Equip> equips) {
        Debug.Log("equips set!!!!!!!!!!!!!!!!");
        equip.setEquips(equips);
        equip.updateSlots();
    }

    public void Update() {
        if (!isLocalPlayer) return;
        if (Input.GetKeyDown(KeyCode.C)) {
            chat.Enable(!chat.isEnabled());
        }
		if (Input.GetKeyDown (KeyCode.K)) {
            skillTreeUi.SetActive(!skillTreeUi.activeInHierarchy);
		}
		if (Input.GetKeyUp (KeyCode.I)) {
			this.inventory.show();
		}
        if (Input.GetKeyDown(KeyCode.E)) {
            this.equip.gameObject.SetActive(!this.equip.gameObject.activeInHierarchy);
        }
        if (Input.GetKeyDown(KeyCode.Q)) {
            getQuestInformationObject().SetActive(!getQuestInformationObject().activeInHierarchy);
            //this.questUI.gameObject.SetActive(!this.questUI.gameObject.activeInHierarchy);
        }
		if(Input.GetKeyDown(KeyCode.B)){
			//Send pickup packet to server

			/*
			 	(C Press B) --> (S checks if player standing on coin)
			 								if(true)
											(S attempts to pickup)
											if(true)
											(mySql) <-- (S) --> (C) 
			 */
		}
    }

	void OnCollisionEnter (Collision col) {
		if (col.gameObject.CompareTag ("NPC")) {
			npcTalkingTo = col.gameObject;
            setActiveNPC(npcTalkingTo.GetComponent<NPC>());
            //this.getNetwork().onTalkNPC(this.currentNPC.getID(),0);
		}
	}
    public Chat getChat() {
        return chat;
    }
    public void setHealth(int health) {
        this.stats.health = health;
        UIPlayer.onHealthChange();
    }
    public void setMana(int mana)
    {
        this.stats.mana = mana;
        UIPlayer.onManaChange();
    }
    public int getMaxHealth() {
        return this.stats.maxHealth;
    }
    public void damage(int dmg, GameObject damager)
    {
        this.stats.health -= dmg;
		StartCoroutine(flash());
    }
	public void canPickup(GameObject drop){
		if(Input.GetKey(KeyCode.B)){

		}
	}

	[ClientRpc]
	void RpcToggleFlash(int state)
	{
		GameObject[] children = Tools.getChildrenByTag(this.gameObject, "Body");
		foreach(GameObject o in children) {
			o.GetComponent<Renderer>().sharedMaterial.SetFloat("_Flash", state);
		}
	}

	IEnumerator flash()
	{
		RpcToggleFlash(0);
		yield return new WaitForSeconds(0.1f);
		RpcToggleFlash(1);
	}
	[Command]
	public void CmdDestroyObject(NetworkIdentity identity){
		Debug.Log("destroying");
		NetworkServer.Destroy(NetworkServer.FindLocalObject(identity.netId));
	}
}

public class PlayerData {
	public int money;
	public string characterName;
	public PlayerData(string characterName, int money){
		this.characterName = characterName;
		this.money = money;
	}
}