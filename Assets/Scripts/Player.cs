using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

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
    public NetworkIdentity identity;
	public delegate void PickupEventHandler(Item item);
	public event PickupEventHandler pickupEventHandler;

    [Header("Player Attributes")]
    public PlayerStats stats;
    public string playerName;
    public int money = 0;
    private GameObject[] playerEquipSlots;

    [Header("Quests")]
    [Space(20)]
    public List<Quest> quests = new List<Quest>();
    public GameObject quest_UI;

    [Header("Skills")]
    [Space(20)]
    public GameObject skillPrefab;
    public GameObject skillEffectPrefab;
    public GameObject skillTreeUi;
    public List<Skill> skills = new List<Skill>();
    public List<Skill> skillsToVerifyWithFromServer = new List<Skill>();

    [Header("System")]
    [Space(20)]
    public Camera camera;
	public playerNetwork network;
	public GameObject npcTalkingTo;
    public Chat chat;
    public GameObject[] prefabsToRegister;

    [Header("Special Effects")]
    [Space(20)]
    public GameObject impactHitPrefab;
	public GameObject levelUpPrefab;
	public GameObject magicEmitEffect;


    [Header("Leave these alone")]
    [Space(20)]
    public NPCMain npcMain;
	public NPCController npcController;
    public void Start()
    {
        playerEquipSlots = Tools.getChildren(this.gameObject, "hatStand", "armorStand");
    }
    public static void setEquipModel(Item item, GameObject[] origins)
    {
        int index = (item.getID() / Tools.ITEM_INTERVAL) - 2;
        Debug.Log("item index: ");
        GameObject itemEquip = Instantiate(Resources.Load<GameObject>(ItemDataProvider.getInstance().getStats(item.getID()).getString("pathToModel")));
        itemEquip.transform.SetParent(origins[index].transform);
        itemEquip.transform.localScale = Vector3.one;
        itemEquip.transform.localPosition = Vector3.zero;
    }
    public void setEquipModel(Item item) {
        setEquipModel(item, playerEquipSlots);
    }
    public void removeEquipModel(Item item) {
        int index = (item.getID() / Tools.ITEM_INTERVAL) - 2;
        foreach (Transform child in getEquipSlot(index).transform.getAllChildren())
        {
            Destroy(child.gameObject);
        }
    }
    public GameObject getEquipSlot(int index) {
        return playerEquipSlots[index];
    }
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
        this.skillUi = Tools.findInactiveChild(UICanvas,"Actionbar_UI").GetComponent<SkillUIManager>();
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
		this.npcController = GameObject.FindWithTag("NPCManager").GetComponent<NPCController>();
		npcController.initilize(this);

        identity = this.GetComponent<NetworkIdentity>();
        Debug.Log("INFO1!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
    }
	public bool hasCompletedQuest(Quest quest){
		return quest.getStatus() == e_QuestStatus.COMPLETED;
	}
	public bool hasTurnedInQuest(Quest quest){
		return quest.getStatus() == e_QuestStatus.TURNED_IN;
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
		return quest.getStatus() == e_QuestStatus.NOT_STARTED && !hasQuest(quest.getId());
		//return (quest != null && quest.getStatus() == e_QuestStatus.NOT_STARTED);
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
	public bool hasQuest(int questId){
		foreach(Quest q in quests.ToArray()){
			if(questId.Equals(q.getId())){
				return true;
			}
		}
		return false;
	}
	public void completeQuest(Quest quest){
		foreach(Quest q in this.quests.ToArray()){
			if(q.getId().Equals(quest.getId())){
				q.setStatus(e_QuestStatus.COMPLETED);
			}
		}
		npcController.getNpcWithQuest(quest).setQuestMarker(this);
		/*Quest q = lookupQuest(quest.getId());
		q.setStatus(quest.getStatus());*/
		Debug.Log("Quest completed, not removed yet.");
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
        equip.setEquips(equips);
        equip.updateSlots();
    }
    public void updateStats(PlayerStats stats) {
        this.stats = stats;
        Debug.Log(stats.health + " : " + stats.maxHealth + " || " + stats.health + " : " + stats.maxHealth + " - INFO2!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        this.UIPlayer.updateInfo();
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
			GameObject o = Instantiate(levelUpPrefab);
			o.transform.position = this.transform.position;
			ParticleScaler s = o.GetComponent<ParticleScaler>();
			s.objectAttachedTo = this.gameObject;
			if(s.levelUpUI != null){
				Text t = s.levelUpUI.transform.Find("Panel").Find("LevelText").GetComponent<Text>();
				t.text = "Level 3";
			}

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
    [Command]
    public void CmdSpawnGameObject(string path)
    {
        GameObject o = Instantiate((GameObject)Resources.Load(path));
        NetworkServer.Spawn(o);
    }
    [Command]
    public void CmdSyncGameObject(NetworkInstanceId netId, Vector3 position, Quaternion rot)
    {
        GameObject o = NetworkServer.FindLocalObject(netId);
        o.transform.position = position;
        o.transform.rotation = rot;
        RpcSyncGameObject(netId, position, rot);
    }
    [ClientRpc]
    public void RpcSyncGameObject(NetworkInstanceId netId, Vector3 position, Quaternion rot)
    {
        GameObject o = ClientScene.FindLocalObject(netId);
        o.transform.position = position;
        o.transform.rotation = rot;
    }
    [Command]
    public void CmdSpawnGameObjectLocally(string path, Vector3 position)
    {
        RpcSpawnGameObjectLocallyOnAllClients(path, position);
    }
    [ClientRpc]
    public void RpcSpawnGameObjectLocallyOnAllClients(string path, Vector3 position)
    {
        Instantiate((GameObject)Resources.Load(path)).transform.position = position;
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