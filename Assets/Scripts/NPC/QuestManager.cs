using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;

public enum e_RequirementType 
{
	MIN_LEVEL,
	MAX_LEVEL,
	ITEM,
	MOB
}

public enum e_QuestTypes {
	MOB,
	ITEM,
	UNDEFINED
}


public enum e_RequirementTypesFromJson 
{
	TYPE,
	DATA,
	ID
}
public enum e_CompletionTypesJson 
{
	VALUE,
	ID
}

public enum e_QuestStatus {
	COMPLETED,
	NOT_STARTED,
	STARTED,
	TURNED_IN
}

/*public class QuestRequirement 
{
	Quest quest;
	e_RequirementType requirementType;
	PlayerServer playerServer;
	int[] requirementData;

	public QuestRequirement(Quest quest, e_RequirementType requirementType, PlayerServer playerServer, int[] requirementData){
		this.requirementType = requirementType;
		this.quest = quest;
		this.playerServer = playerServer;
		this.requirementData = requirementData;
	}


	//For finish and start?
	public bool check(){
		switch(this.requirementType){
			case e_RequirementType.MIN_LEVEL:
				return playerServer.level >= requirementData[(int)e_RequirementTypesFromJson.DATA];

			case e_RequirementType.MAX_LEVEL:
				return playerServer.level < requirementData[(int)e_RequirementTypesFromJson.DATA];

			case e_RequirementType.MOB:
				return quest.getMobKills() >= requirementData[(int)e_RequirementTypesFromJson.DATA];

			case e_RequirementType.ITEM:
				return quest.getItemCount() >= requirementData[(int)e_RequirementTypesFromJson.DATA];
		}
		return false;
	}
}*/

[System.Serializable]
public class Quest
{

	string characterName;
    string name;
	public int id;
	public int expReward;

	//int mobKillsOfSpecifiedMobId = 0;
	Dictionary<int, int> mobKills = new Dictionary<int, int>();
	Dictionary<int, int> itemCount = new Dictionary<int, int>();
    string description;

	//int[] requirementData;
	QuestJson questJson;
	public e_QuestStatus status;
	QuestSQLData sqlData;

	public Quest(int id, string characterName)
    {
		this.characterName = characterName;
		this.id = id;
		status = e_QuestStatus.NOT_STARTED;
		sqlData = new QuestSQLData();
    }

	public Quest(int id)
	{
		this.id = id;
		status = e_QuestStatus.NOT_STARTED;
		sqlData = new QuestSQLData();
	}

	public void start(/*int[] requirementData,*/ QuestJson questJson){
		//this.requirementData = requirementData;
		this.questJson = questJson;
        this.description = questJson.description;
		this.status = e_QuestStatus.STARTED;

		for(int i=0;i<questJson.completionData.completionId.Count;i++){
			if(isCompletionIdMobId(i)){
				Debug.Log("MOB_KILLS: " + mobKills);
				this.mobKills.Add(questJson.completionData.completionId[i], 0);
			}
			if(isCompletionIdItemId(i)){
				setItemCount(questJson.completionData.completionId[i], 0);
			}
		}
		this.expReward = questJson.expReward;
		checkForCompletion();
	}

	public QuestSQLData getData(){
		return this.sqlData;
	}

	public int getId(){
		return this.id;
	}

    public string getName()
    {
        return this.questJson.name;
    }

    public string getDescription()
    {
        return this.questJson.description;
    }

	public void turnIn(){

	}


	public string getTooltip(int indexOfCompletionId){
		if(isCompletionIdMobId(indexOfCompletionId)) return "Mobs killed: " + getMobKills(getMobIds()[indexOfCompletionId]) + "/" + questJson.completionData.completionValue[indexOfCompletionId]; 
		if(isCompletionIdItemId(indexOfCompletionId)) return "Items collected: " + "N/A" + "/" + questJson.completionData.completionValue[indexOfCompletionId];
		return "Error QuestManager > Quest > public string getToolTip()";
	}

	public CompletionData getCompletionData(){
		return questJson.completionData;
	}
		
	/*public int[] getRequirements(){
		return this.requirementData;
	}
	public int getRequirement(e_RequirementTypesFromJson type){
		return this.requirementData[(int)type];
	}*/

    public void initilizeMobQuest(int mobId, int kills)
    {
		setMobKills(mobId, kills);
		checkForCompletion();
    }

	public void setItemCount(int itemId, int count){
		this.itemCount.Add(itemId, count);
	}

	public int getItemCount(int itemId){
		return this.itemCount[itemId];
	}

	public void setMobKills(int mobId, int mobKills){
		if(this.mobKills.ContainsKey(mobId)){
			this.mobKills[mobId] = mobKills;
		}
	}
    public int getItemId()
    {
		int[] ids = questJson.completionData.completionId.ToArray();
		for(int i=0;i<ids.Length;i++){
			if(isCompletionIdItemId(i)){
				return ids[i];
			}
		}
		//Item doesnt exist in this quest
		Debug.LogError("Item doesnt exist in this quest");
		return -1;
    }
		

    public int[] getMobIds()
    {
		int[] ids = questJson.completionData.completionId.ToArray();
		List<int> listOfIds = new List<int>();

		for(int i=0;i<ids.Length;i++){
			if(isCompletionIdMobId(i)){
				listOfIds.Add(ids[i]);
			}
		}
		return listOfIds.ToArray();
    }
		
	public bool isCompletionIdMobId(int index){
		return getCompletionData().completionId[index] >= 10000;
	}
	public bool isCompletionIdItemId(int index){
		return getCompletionData().completionId[index] >= 0 && getCompletionData().completionId[index] <= 5500;
	}

    public QuestJson getQuestJson()
    {
        return this.questJson;
    }

	public int getImageIndex(){
		return this.questJson.imageIndex;
	}

	public bool checkForCompletion(){
		
		bool completed = false;
		for(int i=0;i<getCompletionData().completionId.Count;i++){
			int id = getCompletionData().completionId[i];
			int value = getCompletionData().completionValue[i];

			if(isCompletionIdMobId(i)){
				if(this.getMobKills(id) >= value){
					completed = true;
				} else {
					completed = false;
				}
			}

			if(isCompletionIdItemId(i)){
				if(this.getItemCount(id) >= value){
					completed = true;
				} else {
					completed = false;
				}
			}
		}
		if(completed){
			this.status = e_QuestStatus.COMPLETED;
			return true;
		}
		return false;
	}

	public e_QuestStatus getStatus(){
		return this.status;
	}
		
	public void increaseMobKills(int mobId){
		if(this.mobKills[mobId] < getCompletionValue(mobId)){
			this.mobKills[mobId] += 1;
			checkForCompletion();
		}
	}

	public int getCompletionValue(int id){
		for(int i=0;i<getCompletionData().completionId.Count;i++){
			if(id == getCompletionData().completionId[i]){
				return getCompletionData().completionValue[i];
			}
		}
		return -1;
	}

	public int getMobKills(int mobId){
		if(this.mobKills.ContainsKey(mobId)){
			return this.mobKills[mobId];
		}
		return -1;
		//return this.mobKillsOfSpecifiedMobId;
	}
		
	public string getCharacterName(){
		return this.characterName;
	}

	public void setStatus(e_QuestStatus status){
		this.status = status;
	}

	public e_QuestStatus intToQuestStatus(int status){
		if(status == 1) return e_QuestStatus.COMPLETED;
		else if(status == 0) return e_QuestStatus.STARTED;
		return e_QuestStatus.NOT_STARTED;
	}
	/*public int questStatusToInt(){

	}*/

	public int getCompleted(){
		if(this.status == e_QuestStatus.COMPLETED) return 1;
		if(this.status == e_QuestStatus.TURNED_IN) return 2;
		return 0;
	}
}
	
public class QuestManager {

	Server server;
	QuestJson root;

	public void onQuestCompleted(Quest quest){
		//Server.completeQuest(quest);
	}

	public QuestManager(Server server){
		this.server = server;
		root = JsonManager.readJson<QuestJson>(e_Paths.JSON_QUESTS);

	}

	public void checkValidQuest(Quest quest, int connectionId, PlayerServer playerServer){
		QuestJson qJson = lookUpQuest(quest);

		if(qJson != null){
			quest.start(qJson);
			server.addOrUpdateQuestStatusToDatabase(quest, playerServer, true, PacketTypes.QUEST_START);
			playerServer.questList.Add(quest);
		}
	}
	public void startQuest(Quest quest){
		QuestJson qJson = lookUpQuest(quest);

		if(qJson != null){
			quest.start(qJson);
		}
	}

	public void turnInQuest(Quest quest, PlayerServer pServer){
		bool isSame = false;
		foreach(Quest q in pServer.questList.ToArray()){
			if(quest.getId().Equals(q.getId())){
				isSame = true;
			}
		}
		if(!isSame){
			Debug.LogError("Error cannot complete quest - Quest Invalid");
			return;
		}
		quest.setStatus(e_QuestStatus.TURNED_IN);
		server.addOrUpdateQuestStatusToDatabase(quest, pServer, true, PacketTypes.QUEST_TURN_IN);
	}

	public QuestJson lookUpQuest(Quest quest){
		Debug.Log("ROOT:"  + root.quests);
		foreach(QuestJson questData in root.quests){
			if(quest.getId() == questData.id){
				return questData;
			}
		}
		return null;
	}
}

[System.Serializable]
public class QuestSQLData {
	public int queststatusId;
}


[System.Serializable]
public class QuestJson {
	public QuestJson[] quests;
	public int id;
	public string name;
	public string type;
    public string description;
	public CompletionData completionData;
	public int imageIndex;
	public int expReward;
}

[System.Serializable]
public class CompletionData {
	public List<int> completionValue;
	public List<int> completionId;
}

[System.Serializable]
public class QuestJsonClient {

	public QuestJsonClient[] Quests;
	public int id;
	public string name;
}
