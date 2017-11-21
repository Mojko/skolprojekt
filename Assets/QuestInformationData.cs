using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestInformationData : MonoBehaviour {

	[Header("Quests")]
    public GameObject npcObject;
    public GameObject questWrapper;
    public GameObject questContainer;
 
	private Dictionary<int, GameObject> questPanels = new Dictionary<int, GameObject>();


	void Start () {
		this.transform.SetAsFirstSibling();
	}

    public void addNewQuestPanel(Quest quest)
    {
        GameObject tempQuestPanel = Instantiate(this.questContainer);
		tempQuestPanel.transform.SetParent(this.questWrapper.transform);
		tempQuestPanel.GetComponentInChildren<Text>().text = quest.getName();

		RectTransform rect = tempQuestPanel.GetComponent<RectTransform>();
        rect.anchoredPosition = this.questContainer.GetComponent<RectTransform>().anchoredPosition;
		rect.localPosition = new Vector2(0,170+(rect.sizeDelta.y*-questPanels.Count));
		rect.sizeDelta = new Vector2(0,50);

		tempQuestPanel.GetComponent<QuestContainer>().init(quest);
		questPanels.Add(quest.getId(), tempQuestPanel);
		Debug.Log("new quest panel added");
    }

	public void removeQuestPanel(Quest quest){
		if(questPanels.ContainsKey(quest.getId())){
			questPanels[quest.getId()].GetComponent<QuestContainer>().delete();
			Destroy(questPanels[quest.getId()]);
			questPanels.Remove(quest.getId());
		}
	}
}

