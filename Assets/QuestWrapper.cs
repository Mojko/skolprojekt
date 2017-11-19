using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestWrapper : MonoBehaviour {

	public List<QuestContainer> questContainers = new List<QuestContainer>();

	public void onQuestContainerClick(){
		int clickAmount = 0;
		QuestContainer clicked = null;
		foreach(QuestContainer qContainer in questContainers){
			if(qContainer.isClicked){
				qContainer.isClicked = false;
				return;
			}
		}
	}
}
