using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NPCController : NetworkBehaviour {

	[HideInInspector] public GameObject UI;
	[HideInInspector] public GameObject questGiverObject;

	public GameObject dialogueUI;
	public Text dialogueUI_questDescription;
	public Text dialogueUI_questGiverName;
	public Image dialogueUI_questGiverFace;

	public GameObject[] npcs;

	private List<NPCMain> npcMain = new List<NPCMain>();
	private Player player;

	private QuestWrapper wrapper;
	private Image img;

	public void initilize(Player player) {
		this.player = player;
		UI = player.getUI();
		questGiverObject = player.getQuestInformationData().transform.GetChild(3).GetChild(0).gameObject;
		img = questGiverObject.GetComponent<Image>();
		wrapper = player.getQuestInformationData().questWrapper.GetComponent<QuestWrapper>();

		for(int i=0;i<npcs.Length;i++){
			NPCMain main = npcs[i].GetComponent<NPCMain>();
			main.uniqueId = i; 
			main.dialogueUI_questDescription = this.dialogueUI_questDescription;
			main.dialogueUI_questGiverFace = this.dialogueUI_questGiverFace;
			main.dialogueUI_questGiverName = this.dialogueUI_questGiverName;
			main.dialogueUI = this.dialogueUI;
			main.gameObject.SetActive(true);
			npcMain.Add(main);
		}
	}
		
	public NPCMain getNpcWithQuest(Quest quest){
		foreach(NPCMain main in npcMain){
			for(int i=0;i<main.questIds.Length;i++){
				if(quest.getId().Equals(main.questIds[i])){
					return main;
				}
			}
		}
		return null;
	}

	public Sprite getSprite(int questId, QuestContainer container){
		foreach(NPCMain main in npcMain){
			for(int i=0;i<main.questIds.Length;i++){
				if(container.getQuest().getId().Equals(main.questIds[i])){
					return main.getSprite();
				}
			}
		}
		return null;
	}

	public void updateSprite(Sprite sprite){
		img.sprite = sprite;
	}
}
