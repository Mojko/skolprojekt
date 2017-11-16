using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestWrapper : MonoBehaviour {

	public List<QuestContainer> questContainers = new List<QuestContainer>();

	public void onQuestContainerClick(){
		int clickAmount = 0;
		QuestContainer clicked = null;
		foreach(QuestContainer qContainer in questContainers){
			if(qContainer.isClicked){
				clickAmount++;
				if(clickAmount > 1){
					clicked.isClicked = false;
					clickAmount = 0;
					return;
				} else {
					clicked = qContainer;
				}
			}
		}
	}
}
