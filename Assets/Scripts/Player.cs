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
<<<<<<< HEAD
    private UIPlayerHandler UIPlayer;
=======
	private GameObject npcManager;
>>>>>>> db5a129cae6e71d9f951cf02ea86eaba50f274d6

    [Header("Player Attributes")]
    public string playerName;
    public int level;
    public int health = 100;
    public int mana = 100;
    public int money = 0;
    public int maxHealth = 3000;
    public int maxMana = 3000;

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
        this.maxHealth = 1000;
        this.maxMana = 1000;
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
        this.equip = Tools.getChild(UICanvas, "Equipment_UI").GetComponent<EquipmentHandler>();
        equip.setEquipmentUI(Tools.getChild(UICanvas, "Equipment_UI"));
        equip.setPlayer(this);

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

<<<<<<< HEAD
        //UIPlayer
        this.UIPlayer = Tools.findInactiveChild(UICanvas, "Footer_UI").GetComponent<UIPlayerHandler>();
        this.UIPlayer.setPlayer(this);
        this.UIPlayer.gameObject.SetActive(true);
=======
		//QuestManager
		this.npcManager = GameObject.FindWithTag("NPCManager");
		npcManager.GetComponent<NPCController>().initilize(this);
>>>>>>> db5a129cae6e71d9f951cf02ea86eaba50f274d6

    }
	public bool hasQuest(Quest quest){
		foreach(Quest q in quests.ToArray()){
			if(quest.getId().Equals(q.getId())){
				return true;
			}
		}
		return false;
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
        this.health = health;
        UIPlayer.onHealthChange();
    }
    public void setMana(int mana)
    {
        this.mana = mana;
        UIPlayer.onManaChange();
    }
    public int getMaxHealth() {
        return this.maxHealth;
    }
    public void pickup(Item item, e_ItemTypes type)
    {
        switch (type) {
            case e_ItemTypes.MONEY:
                this.money += 10;
                break;
        }
    }
    public void damage(int dmg, GameObject damager)
    {
        this.health -= dmg;
		StartCoroutine(flash());
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
}