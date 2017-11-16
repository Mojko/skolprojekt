using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AI;

public class MobManager : NetworkBehaviour {
	
	[SyncVar] int health = 3;
	[SyncVar] int mana;

    public MobHolder mobHolder;

	public Vector3 newPos;
	public Respawner respawner;
    public Spawner spawner;
	
	public GameObject drop;
	public GameObject part;
    public GameObject target;
    private PlayerServer targetNetwork;
    public int targetId;

	public Server server;


    float flashTimer = 1;

    public Vector3 rootPos;
    private int id;

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
        drop.setName(nameOfDrop);
        Item item = Item.getEmptyItem(0);
        drop.setItem(item);
        o.transform.position = position;
        NetworkServer.Spawn(o);
    }

    void spawnDrop(string nameOfDrop, Vector3 position)
    {
        GameObject prefab = (GameObject)Resources.Load("Prefabs/Drops/"+"D_"+nameOfDrop);
        GameObject o = Instantiate(prefab);
        Drop drop = o.GetComponent<Drop>();
        drop.setName(nameOfDrop);
        Item item = Item.getEmptyItem(0);
        drop.setItem(item);
        o.transform.position = position;
        NetworkServer.Spawn(o);
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

		Player p = this.target.GetComponent<Player>();
        
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

        foreach(Quest q in targetNetwork.questList.ToArray()){

			Debug.Log("A QUEST: " + " | " + q.getMobId() + " | " + getId());
			if(q.getMobId() == getId()){
			    q.increaseMobKills();
                questsToSend.Add(q);
		    }
	    }
        if(questsToSend.Count > 0){
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
