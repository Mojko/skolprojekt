using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NPCController : NetworkBehaviour {

	public GameObject UI;
	public GameObject questGiverObject;
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

		foreach(GameObject o in npcs){
			npcMain.Add(o.GetComponent<NPCMain>());
		}
	}

	public NPCMain getNpc(QuestContainer container){
		foreach(NPCMain main in npcMain){
			for(int i=0;i<main.questIds.Length;i++){
				if(container.getQuest().getId().Equals(main.questIds[i])){
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
