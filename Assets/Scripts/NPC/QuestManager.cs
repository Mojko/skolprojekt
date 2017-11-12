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
	int id;

	int mobKillsOfSpecifiedMobId = 0;
	int itemCountOfSpecifiedItemId = 0;

	//int[] requirementData;
	QuestJson questJson;
	string type;
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

	public int getId(){
		return id;
	}

	public void start(string type, /*int[] requirementData,*/ QuestJson questJson){
		this.type = type;
		//this.requirementData = requirementData;
		this.questJson = questJson;
		this.status = e_QuestStatus.STARTED;
	}

	public string getType(){
		return this.type;
	}

	public string getTooltip(){
		//string type = requirementData[(int)e_RequirementTypesFromJson.TYPE].ToString();
		int completionRequirement = questJson.completionData.completionValue;
		return "Mobs killed: " + getMobKills() + "/" + completionRequirement;
	}



	/*public int[] getRequirements(){
		return this.requirementData;
	}
	public int getRequirement(e_RequirementTypesFromJson type){
		return this.requirementData[(int)type];
	}*/

	public e_QuestStatus getStatus(){
		return this.status;
	}
		
	public void increaseMobKills(){
		Debug.Log("MOB KILLS: " + mobKillsOfSpecifiedMobId);
		this.mobKillsOfSpecifiedMobId += 1;
	}
	public void increaseItemCount(){
		this.itemCountOfSpecifiedItemId += 1;
	}

	public int getMobKills(){
		return this.mobKillsOfSpecifiedMobId;
	}

	public int getItemCount(){
		return this.itemCountOfSpecifiedItemId;
	}
		
	public string getCharacterName(){
		return this.characterName;
	}

	public int getCompleted(){
		if(this.status == e_QuestStatus.COMPLETED) return 1; else return 0;
	}

	/*public e_QuestTypes questParser(){
		if(getRequirement(e_RequirementTypesFromJson.DATA).Equals("mob")) return e_QuestTypes.MOB;
		else if(getRequirement(e_RequirementTypesFromJson.DATA).Equals("item")) return e_QuestTypes.ITEM;
		else return e_QuestTypes.UNDEFINED;
	}*/
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
			quest.start(qJson.type, qJson);
			server.addOrUpdateQuestStatusToDatabase(quest, connectionId);
		}


		/*foreach(QuestJson q in questJson.Quests){
			if(quest.getId() == q.id){
				//Quest is valid
				//QuestRequirement requirements = new QuestRequirement(quest, (e_RequirementType)q.requirementTypeList[(int)e_RequirementTypesFromJson.TYPE], playerServer, q.requirementTypeList);
				//if(requirements.check()){
				quest.start(q.type, /*q.requirementTypeList,*/ //q);
				//server.addOrUpdateQuestStatusToDatabase(quest, connectionId);
				//}
			//}
		//}
	}
	public void startQuest(Quest quest){
		QuestJson qJson = lookUpQuest(quest);
		if(qJson != null){
			quest.start(qJson.type, qJson);
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
	public CompletionData completionData;
}

[System.Serializable]
public class CompletionData {
	public int completionValue;
	public int completionId;
}

[System.Serializable]
public class QuestJsonClient {

	public QuestJsonClient[] Quests;
	public int id;
	public string name;
}
