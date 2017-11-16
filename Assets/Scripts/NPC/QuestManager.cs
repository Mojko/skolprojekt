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
	STARTED
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
	int id;

	int mobKillsOfSpecifiedMobId = 0;
    int mobId = 0;
    int itemId = 0;
    string description;

	//int[] requirementData;
	QuestJson questJson;
	e_QuestStatus status;

	public Quest(int id, string characterName)
    {
		this.characterName = characterName;
		this.id = id;
		status = e_QuestStatus.NOT_STARTED;
    }

	public Quest(int id)
	{
		this.id = id;
		status = e_QuestStatus.NOT_STARTED;
	}

	public void start(/*int[] requirementData,*/ QuestJson questJson){
		//this.requirementData = requirementData;
		this.questJson = questJson;
        this.description = questJson.description;
		this.status = e_QuestStatus.STARTED;
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


	public string getTooltip(int indexOfCompletionId){
		if(isCompletionIdMobId(indexOfCompletionId)) return "Mobs killed: " + getMobKills() + "/" + questJson.completionData.completionValue[indexOfCompletionId]; 
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
        this.mobId = mobId;
        this.mobKillsOfSpecifiedMobId = kills;
    }

    public void setMobId(int mobId)
    {
        this.mobId = mobId;
    }
	public void setMobKills(int mobKills){
		mobKillsOfSpecifiedMobId = mobKills;
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

    public int getMobId()
    {
		int[] ids = questJson.completionData.completionId.ToArray();
		Debug.Log("IDSLENGTH: " + ids.Length);
		for(int i=0;i<ids.Length;i++){
			if(isCompletionIdMobId(i)){
				return ids[i];
			}
		}
		//Mob doesnt exist in this quest
		Debug.LogError("Mob doesnt exist in this quest");
        return -1;
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

	public e_QuestStatus getStatus(){
		return this.status;
	}
		
	public void increaseMobKills(){
		this.mobKillsOfSpecifiedMobId += 1;
        Debug.Log("MOB KILLS: " + mobKillsOfSpecifiedMobId);
	}

	public int getMobKills(){
		return this.mobKillsOfSpecifiedMobId;
	}
		
	public string getCharacterName(){
		return this.characterName;
	}

	public int getCompleted(){
		if(this.status == e_QuestStatus.COMPLETED) return 1; else return 0;
	}
}
	
public class QuestManager {

	Server server;
	QuestJson root;

	public QuestManager(Server server){
		this.server = server;
		root = JsonUtility.FromJson<QuestJson> (File.ReadAllText("Assets/XML/Quests.json"));
	}

	public void checkValidQuest(Quest quest, int connectionId, PlayerServer playerServer){
		QuestJson qJson = lookUpQuest(quest);

		if(qJson != null){
			quest.start(qJson);
			server.addOrUpdateQuestStatusToDatabase(quest, connectionId);
		}
	}
	public void startQuest(Quest quest){
		QuestJson qJson = lookUpQuest(quest);
		if(qJson != null){
			quest.start(qJson);
		}
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
public class QuestJson {
	public QuestJson[] quests;
	public int id;
	public string name;
	public string type;
    public string description;
	public CompletionData completionData;
	public int imageIndex;
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
