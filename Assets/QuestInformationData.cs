using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestInformationData : MonoBehaviour {

	[Header("Quests")]
    public GameObject npcObject;
    public GameObject questWrapper;
    public GameObject questContainer;
	public GameObject closeButton;

	private Dictionary<int, GameObject> questPanels = new Dictionary<int, GameObject>();
	private List<QuestContainer> questPanelContainers = new List<QuestContainer>();
	private Image image;

	void Start () {
		this.transform.SetAsFirstSibling();
		image = this.GetComponent<Image>();
		toggleActive(false);
	}

	public void toggleActive(bool toggle){
		this.image.enabled = toggle;
		this.closeButton.SetActive(toggle);

		foreach(QuestContainer container in questPanelContainers){
			container.isClicked = toggle;
		}

		foreach(Transform childTransform in this.transform.getAllChildren()){
			childTransform.gameObject.SetActive(toggle);
		}
	}
	public void toggleActive(bool toggle, QuestContainer container){
		this.image.enabled = toggle;
		this.closeButton.SetActive(toggle);
		container.isClicked = toggle;
		foreach(Transform childTransform in this.transform.getAllChildren()){
			childTransform.gameObject.SetActive(toggle);
		}
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

		QuestContainer container = tempQuestPanel.GetComponent<QuestContainer>();
		container.init(quest);
		questPanelContainers.Add(container);
		questPanels.Add(quest.getId(), tempQuestPanel);
		Debug.Log("new quest panel added");
    }

	public void removeQuestPanel(Quest quest){
		if(questPanels.ContainsKey(quest.getId())){
			questPanels[quest.getId()].GetComponent<QuestContainer>().delete();
			questPanels.Remove(quest.getId());
			toggleActive(false);
		}
	}
}

