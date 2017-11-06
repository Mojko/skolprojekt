using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class Player : NetworkBehaviour
{
    NetworkConnection con;
    public string playerName;
    public int level;
    public Camera camera;
	public playerNetwork network;
	private Inventory inventory;
	public GameObject npcTalkingTo;
	public GameObject[] prefabsToRegister;
    private GameObject UICanvas;
    public Chat chat;
    private NPC currentNPC;
    private SkillUIManager skillUi;
    private SkillManager skillManager;
    public GameObject skillPrefab;
    private PlayerMovement movement;
    

    public List<Skill> skills = new List<Skill>();
    public List<Skill> skillsToVerifyWithFromServer = new List<Skill>();

    public int health = 100;
    public int mana = 100;

    public override void OnStartLocalPlayer ()
	{
        ClientScene.RegisterPrefab((GameObject)Resources.Load("Prefabs/Enemy"));
        ClientScene.RegisterPrefab((GameObject)Resources.Load("Particles/Hit"));
        ClientScene.RegisterPrefab((GameObject)Resources.Load("Particles/ImpactOnGround"));

        this.movement = GetComponent<PlayerMovement>();

        this.skillUi = GameObject.Find("Actionbar_UI").GetComponent<SkillUIManager>();
        this.skillUi.init(this);
        this.skillManager = GetComponent<SkillManager>();
        this.skillManager.init(this);

        UICanvas = GameObject.Find("UI");
        chat = Tools.getChild(UICanvas, "Chat_UI").GetComponent<Chat>();
		network = GetComponent<playerNetwork>();
        network.initialize (this);
        chat.setPlayer(this);
		camera = Camera.main;
		this.inventory = this.GetComponent<Inventory> ();
		this.inventory.init (this);

		camera.GetComponent<MainCamera> ().player = this.gameObject;
		camera.GetComponent<MainCamera> ().setState ((int)e_cameraStates.DEFAULT);
		for (int i = 0; i < prefabsToRegister.Length; i++) {
			//ClientScene.RegisterPrefab (prefabsToRegister [i]);
		}
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
	public void setInventory(int[] items){
		inventory.setInventory (items);
	}
    public Inventory getInventory() {
        return inventory;
    }
    public void Update() {
        if (!isLocalPlayer) return;
        if (Input.GetKeyDown(KeyCode.C)) {
            chat.Enable(!chat.isEnabled());
        }
		if (Input.GetKeyDown (KeyCode.K)) {
		}
		activateInventory ();

    }
	public void activateInventory(){
		if (Input.GetKeyUp (KeyCode.I)) {
			this.inventory.show();
		}
	}

	void OnCollisionEnter (Collision col) {
		if (col.gameObject.CompareTag ("NPC")) {
			npcTalkingTo = col.gameObject;
            setActiveNPC(npcTalkingTo.GetComponent<NPC>());
            this.getNetwork().onTalkNPC(this.currentNPC.getID(),0);
		}
	}
    public Chat getChat() {
        return chat;
    }
    public void damage(int dmg)
    {
        this.health -= dmg;
    }
}