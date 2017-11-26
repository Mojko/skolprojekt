using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class MobManager : NetworkBehaviour {
	
	[SyncVar] int health = 3;
	[SyncVar] int mana;
	private PlayerServer targetNetwork;
	private float flashTimer = 1;
	private int id;

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

	[Header("Fill these in")]
	[Space(10)]
	[Tooltip("Prefab name: Drop")] public GameObject drop;
	[Tooltip("Prefab name: Part")] public GameObject part;


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
        this.server = GameObject.FindWithTag("Server").GetComponent<Server>();
        if(this.id == 0) {
            setId(10000);
        }
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
        targetNetwork = playerServer;
        StartCoroutine(flash());
		health -= 1;
        if(damager != null){
            target = damager;
        }
		if (health <= 0) {
			kill();
		}
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
		Monster monster = JsonUtility.FromJson<Monster>(File.ReadAllText(JsonManager.getPath(e_Paths.JSON_MONSTERS)));
		foreach(Monster m in monster.Monsters){
			if(m.id == this.getId()){
				return m;
			}
		}
		return null;
	}

    void spawnDrop(string nameOfDrop, Vector3 position)
    {
        GameObject prefab = (GameObject)Resources.Load("Prefabs/Drops/"+"D_"+nameOfDrop);
        GameObject o = Instantiate(prefab);
        Drop drop = o.GetComponent<Drop>();

		Monster monster = lookupMonster(getId());
		int randomIndex = Random.Range(0,monster.dropIds.Length);
		int itemId = monster.dropIds[randomIndex];
		int itemQuantity = monster.dropQuantity[randomIndex];

		Item item = new Item(itemId);
		item.setQuantity(itemQuantity);
		Debug.Log("ITEM_ID: " + item.getID() + " | QUANITY: " + item.getQuantity());
		drop.initilize(item, this.targetNetwork);

        o.transform.position = position;
        NetworkServer.Spawn(o);
		DropInfo dropInfo = new DropInfo();
		dropInfo.netId = o.GetComponent<NetworkIdentity>().netId;
		dropInfo.item = Tools.objectToByteArray(drop.getItem());
		NetworkServer.SendToClient(this.targetNetwork.connectionID, PacketTypes.DROP_INIT, dropInfo);
    }

	void kill(){
		if(!isServer) return;
        if(this.respawner == null) this.respawner = GameObject.FindWithTag("Respawner").GetComponent<Respawner>();
        if(this.spawner == null) this.spawner = GameObject.FindWithTag("Spawner").GetComponent<Spawner>();

        Vector3 pos = new Vector3(this.transform.position.x, this.transform.position.y + 0.5f, this.transform.position.z) + this.transform.forward;
        Server.spawnObject(e_Objects.PARTICLE_DEATH, pos);
        for(int i=0;i<4;i++){
            spawnDrop("Coin_gold", pos);
        }

		/*Player p = this.target.GetComponent<Player>();
		foreach(Quest quest in p.getQuests()){
			if(quest.getType().Equals("mob")){
				quest.increaseMobKills();
				p.getQuestUI().addNewQuestToolTip(quest.getTooltip());
			}
		}*/
        
		//QuestInfo qInfo = new QuestInfo();
		//PlayerServer pServer = Server.getPlayerObject(targetConnectionId);
		List<Quest> questsToSend = new List<Quest>();//Server.getQuestArrayFromPlayerName(server.getPlayerObject(p.connectionToServer.connectionId).playerName);
		//List<Quest> questsToSend = new List<Quest>();
		/*foreach(Quest q in pServer.questList.ToArray()){
			if(q.getType().Equals("mob")){
				q.increaseMobKills();
				questsToSend.Add(q);
			}
		}
		qInfo.questClassInBytes = Tools.objectArrayToByteArray(questsToSend.ToArray());
		NetworkServer.SendToClient(p.connectionToServer.connectionId, PacketTypes.QUEST_UPDATE, qInfo);*/

		bool shouldUpdate = true;
        foreach(Quest q in targetNetwork.questList.ToArray()){
			int[] ids = q.getMobIds();
			for(int i=0;i<ids.Length;i++){
				if(ids[i] == getId()){
					q.increaseMobKills(this.getId());
	                questsToSend.Add(q);
					if(q.checkForCompletion()){
						this.server.addOrUpdateQuestStatusToDatabase(q, targetNetwork, false);
						QuestInfo qInfo = new QuestInfo();
						qInfo.questClassInBytes = Tools.objectToByteArray(q);
						NetworkServer.SendToClient(targetNetwork.connectionID, PacketTypes.QUEST_COMPLETE, qInfo);
						shouldUpdate = false;
						Debug.Log("completing quest..");
					}
			    }
			}
	    }
		if(questsToSend.Count > 0 && shouldUpdate && isServer){
            QuestInfo qInfo = new QuestInfo();
            qInfo.questClassInBytes = Tools.objectArrayToByteArray(questsToSend.ToArray());
            NetworkServer.SendToClient(targetId, PacketTypes.QUEST_UPDATE, qInfo);
        }

		//Server.spawnObject(drop, this.transform.position);
		//Server.spawnObject(part, new Vector3(this.transform.position.x, this.transform.position.y + 0.5f, this.transform.position.z) + this.transform.forward);
        spawner.totalEnemiesInArea--;
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
	public int level;
	public string pathToModel;
	public int[] dropIds;
	public int[] dropQuantity;

}