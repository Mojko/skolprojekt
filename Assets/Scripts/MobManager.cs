using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public enum e_States {
	IDLE,
	CHASE,
	ATTACK,
	DIE
}

public class MobManager : NetworkBehaviour {
	
	[SyncVar] int health = 3;
	[SyncVar] int mana;
	private PlayerServer targetNetwork;
	private float flashTimer = 1;
	private int id;
	private SkillCastManager skillCastManager;
	private Monster monster;

	[Header("MobManager")]
	[Header("Leave these alone")]
    public MobHolder mobHolder;
	public Vector3 newPos;
	public Respawner respawner;
    public Spawner spawner;
	public GameObject target;
	public Server server;
	public int targetId = -1;
	public Vector3 rootPos;
	[SyncVar] public bool targeted;
	public Animator animator;
	public e_States state = e_States.IDLE;

	[Header("Fill these in")]
	[Space(10)]
	[Tooltip("Prefab name: Drop")] public GameObject drop;
	[Tooltip("Prefab name: Part")] public GameObject part;
    public GameObject impactHitPrefab;
	public GameObject rewardText;


	public void setId(int id)
    {
        this.id = id;
    }

    public int getId()
    {
        return this.id;
    }

    private void Start()
    {
        if(this.id == 0) {
			this.id = 10000;
        }
		this.monster = lookupMonster(this.id);
    }

	public void setServer(Server server){
		this.server = server;
	}

    public void stun(int damage, int duration) {

	}

	public void heal(int amount) {

	}

    public void setTarget(int connectionId, GameObject target)
    {
        this.target = target;
        this.targetId = connectionId;
    }

	public void damage (int damage, GameObject damager, PlayerServer playerServer) {
        if(!isServer) return;
		targetNetwork = playerServer;
		this.target = target;
		this.targetId = playerServer.connectionID;
        StartCoroutine(flash());
		health -= damage;
        //Server.spawnObject(e_Objects.VFX_IMPACT_MELEE_1, new Vector3(this.transform.position.x, this.transform.position.y+0.5f, this.transform.position.z));
        //GameObject o = Instantiate((GameObject)Resources.Load("Special Effects/Skills/"+this.impactHitPrefab.name));
        //o.transform.position = new Vector3(this.transform.position.x, this.transform.position.y+0.5f, this.transform.position.z);
        //NetworkServer.Spawn(o);
        if(damager != null){
            target = damager;
        }
		if (health <= 0) {
			//kill();
			this.state = e_States.DIE;
		}
	}

    public void setSkillCastManager(SkillCastManager skillCastManager)
    {
        this.skillCastManager = skillCastManager;
    }

    [Command]
    void CmdSpawnDrop(string nameOfDrop, Vector3 position)
    {
        GameObject prefab = (GameObject)Resources.Load("Prefabs/Drops/"+"D_"+nameOfDrop);
        GameObject o = Instantiate(prefab);
        Drop drop = o.GetComponent<Drop>();

		Monster monster = JsonManager.readJson<Monster>(e_Paths.JSON_MONSTERS);
		int[] dropIds = monster.dropIds;
		int[] dropQuantity = monster.dropQuantity;
		int randomIndex = Random.Range(0,0);
		int itemId = dropIds[randomIndex];
		int itemQuantity = dropQuantity[randomIndex];

		Item item = new Item(itemId);
		item.setQuantity(itemQuantity);

        o.transform.position = position;
        NetworkServer.Spawn(o);
    }

	public Monster lookupMonster(int monsterId){
		Monster monster = JsonManager.readJson<Monster>(e_Paths.JSON_MONSTERS);
		//Monster monster = JsonUtility.FromJson<Monster>(File.ReadAllText("C:/Users/Jesper/AppData/LocalLow/Wojon/GameServer/Resources/Visuals/Monster.json"));
		foreach(Monster m in monster.Monsters){
			if(m.id == this.getId()){
				return m;
			}
		}
		return null;
	}
    bool isUnder(float chance) {
     return (Random.Range(0, 100) <= chance*100);
    }
    void spawnDrop(Monster monster, Vector3 position)
    {
        for (int i = 0; i < monster.dropIds.Length; i++)
        {
            if (!isUnder(monster.dropChance[i])) continue;
            int itemID = monster.dropIds[i];
            Debug.Log("item ID here!: " + itemID + " : " + i);
            GameObject prefab;      
            prefab = (GameObject)Instantiate(ResourceStructure.getGameObjectFromPath(ItemDataProvider.getInstance().getStats(itemID).getString("pathToDropModel")));
            //Drop drop = prefab.AddComponent<Drop>();
            NetworkIdentity identity = prefab.GetComponent<NetworkIdentity>();
            int itemQuantity = monster.dropQuantity[i];
            Item item = new Item(itemID);
            item.setQuantity(itemQuantity);
            Drop drop = prefab.GetComponent<Drop>();
            drop.initilize(item);
            //drop.initilize(item, this.targetNetwork);
            prefab.transform.position = position;
            NetworkServer.Spawn(prefab);
            ItemInfo packet = new ItemInfo();
            packet.item = Tools.objectToByteArray(item);
            packet.netId = identity.netId;
            NetworkServer.SendToAll(PacketTypes.DROP_INIT, packet);
        }
    }

	public void kill(){
		if(!isServer) return;
        //if(this.respawner == null) this.respawner = GameObject.FindWithTag("Respawner").GetComponent<Respawner>();
        //if(this.spawner == null) this.spawner = GameObject.FindWithTag("Spawner").GetComponent<Spawner>();

        Vector3 pos = new Vector3(this.transform.position.x, this.transform.position.y + 0.5f, this.transform.position.z) + this.transform.forward;
        Server.spawnObject(e_Objects.PARTICLE_DEATH, pos);
		Monster m = lookupMonster(this.id);
		if(this.id != 0){
			Server.giveExpToPlayer(m.exp, this.targetNetwork);
			Debug.Log("EXP GIVEN: " + m.exp);
		}
        spawnDrop(m, pos);
		List<Quest> questsToSend = new List<Quest>();

		bool shouldUpdate = true;
        foreach(Quest q in targetNetwork.questList.ToArray()){
			Debug.Log("is this array going " + targetNetwork.questList.Count);
			if(q.getStatus() == e_QuestStatus.TURNED_IN) continue;
			int[] ids = q.getMobIds();
			for(int i=0;i<ids.Length;i++){
				if(ids[i] == getId()){
					q.increaseMobKills(this.getId());
	                questsToSend.Add(q);
					if(q.checkForCompletion()){
						this.server.addOrUpdateQuestStatusToDatabase(q, targetNetwork, true, PacketTypes.QUEST_UPDATE);
						QuestInfo qInfo = new QuestInfo();
						qInfo.questClassInBytes = Tools.objectToByteArray(q);
						NetworkServer.SendToClient(targetNetwork.connectionID, PacketTypes.QUEST_COMPLETE, qInfo);
						shouldUpdate = true;
					}
			    }
			}
		}
		if(shouldUpdate){
            QuestInfo qInfo = new QuestInfo();
			foreach(Quest q in questsToSend){
            	qInfo.questClassInBytes = Tools.objectToByteArray(q);
				Debug.Log("TARGET_ID: " + targetId);
				NetworkServer.SendToClient(targetId, PacketTypes.QUEST_UPDATE, qInfo);
			}
        }

		//Server.spawnObject(drop, this.transform.position);
		//Server.spawnObject(part, new Vector3(this.transform.position.x, this.transform.position.y + 0.5f, this.transform.position.z) + this.transform.forward);
        spawner.totalEnemiesInArea--;
		GameObject o = Instantiate(rewardText);
		o.transform.position = this.transform.position;
		o.transform.SetParent(GameObject.Find("WorldSpaceCanvas").transform);
		if(this.skillCastManager != null){
			NetworkServer.Destroy(this.skillCastManager.gameObject);
		}
		NetworkServer.Destroy(this.gameObject);
	}
	
	public int getHealth (){
		return health;
	}
	public int getMana (){
		return mana;
	}
	public void setHealth (int health){
		this.health = health;
	}
	public void setMana (int mana){
		this.mana = mana;
	}

	void toggleFlash(int state){
		this.transform.Find("Body").GetComponent<Renderer>().material.SetFloat("_Flash", state);
	}

    [ClientRpc]
    void RpcToggleFlash(int state)
    {
        GameObject[] children = Tools.getChildrenByTag(this.gameObject, "Body");
        foreach(GameObject o in children) {
            o.GetComponent<Renderer>().material.SetFloat("_Flash", state);
        }
    }

    IEnumerator flash()
    {
        RpcToggleFlash(0);
        yield return new WaitForSeconds(0.1f);
        RpcToggleFlash(1);
    }
}

[System.Serializable]
public class Monster
{
	public Monster[] Monsters;
	public int id;
	public string name;
	public int[] stats;
    public float[] dropChance;
	public int level;
	public string pathToModel;
	public int[] dropIds;
	public int[] dropQuantity;
	public int exp;

}